using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ArtGallery.Application.DTOs.Etl;
using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;
using Oracle.ManagedDataAccess.Client;

namespace ArtGallery.Infrastructure.Services;

/// <summary>
/// Service for executing Oracle PL/SQL procedures for ETL and data management.
/// 
/// This service provides two ETL execution modes:
/// 1. PL/SQL Procedure-based (if procedures are deployed to database)
/// 2. Code-based SQL execution (fallback, always works)
/// 
/// The code-based approach uses MERGE statements for idempotent ETL operations.
/// </summary>
public class OracleProcedureService : IOracleProcedureService
{
    private readonly AppDbContext _oltpContext;
    private readonly DwDbContext _dwContext;
    private readonly ILogger<OracleProcedureService> _logger;
    private readonly IConfiguration _configuration;

    public OracleProcedureService(
        AppDbContext oltpContext,
        DwDbContext dwContext,
        ILogger<OracleProcedureService> logger,
        IConfiguration configuration)
    {
        _oltpContext = oltpContext;
        _dwContext = dwContext;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Propagates OLTP data to DW using direct SQL MERGE statements.
    /// This approach works regardless of whether PL/SQL procedures are deployed.
    /// </summary>
    public async Task<EtlPropagationResult> PropagateOltpToDwAsync(
        EtlMode mode = EtlMode.Incremental,
        CancellationToken cancellationToken = default)
    {
        var result = new EtlPropagationResult
        {
            StartTime = DateTime.UtcNow,
            Status = "Running"
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting ETL propagation using direct SQL. Mode: {Mode}", mode);

            // Execute ETL using direct SQL (code-based approach)
            result.TableResults = await ExecuteCodeBasedEtlAsync(cancellationToken);

            // Calculate totals
            result.LoadedRecords = result.TableResults.Sum(t => t.RecordsProcessed);
            result.InsertedRecords = result.TableResults.Sum(t => t.RecordsInserted);
            result.UpdatedRecords = result.TableResults.Sum(t => t.RecordsUpdated);

            // Check for errors
            var errors = result.TableResults.Where(t => t.Status == "Error").ToList();
            if (errors.Any())
            {
                result.Status = "Partial";
                result.Errors.AddRange(errors.Select(e => $"{e.TableName}: {e.ErrorMessage}"));
            }
            else
            {
                result.Status = "Success";
            }

            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.EndTime = DateTime.UtcNow;

            _logger.LogInformation(
                "ETL propagation completed. Records: {Records}, Duration: {Duration}ms, Status: {Status}",
                result.LoadedRecords, result.DurationMs, result.Status);
        }
        catch (OracleException ex)
        {
            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.EndTime = DateTime.UtcNow;
            result.Status = "Error";
            result.Errors.Add($"Oracle Error ORA-{ex.Number:D5}: {ex.Message}");

            _logger.LogError(ex, "ETL propagation failed with Oracle error ORA-{Number}", ex.Number);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.EndTime = DateTime.UtcNow;
            result.Status = "Error";
            result.Errors.Add(ex.Message);

            _logger.LogError(ex, "ETL propagation failed");
        }

        return result;
    }

    /// <summary>
    /// Executes ETL using direct SQL MERGE statements from C# code.
    /// This is the primary ETL method that works without requiring PL/SQL procedures.
    /// </summary>
    private async Task<List<TableSyncResult>> ExecuteCodeBasedEtlAsync(CancellationToken cancellationToken)
    {
        var results = new List<TableSyncResult>();

        // Sync dimensions in dependency order, then fact table
        var syncOperations = new (string TableName, Func<CancellationToken, Task<int>> SyncFunc)[]
        {
            ("DIM_ARTIST", SyncDimArtistAsync),
            ("DIM_COLLECTION", SyncDimCollectionAsync),
            ("DIM_LOCATION", SyncDimLocationAsync),
            ("DIM_EXHIBITOR", SyncDimExhibitorAsync),
            ("DIM_POLICY", SyncDimPolicyAsync),
            ("DIM_EXHIBITION", SyncDimExhibitionAsync),
            ("DIM_ARTWORK", SyncDimArtworkAsync),
            ("FACT_EXHIBITION_ACTIVITY", SyncFactExhibitionActivityAsync)
        };

        foreach (var (tableName, syncFunc) in syncOperations)
        {
            var tableResult = new TableSyncResult { TableName = tableName };
            var sw = Stopwatch.StartNew();

            try
            {
                _logger.LogDebug("Syncing {Table}...", tableName);
                var recordsAffected = await syncFunc(cancellationToken);
                sw.Stop();

                tableResult.RecordsProcessed = recordsAffected;
                tableResult.DurationMs = sw.ElapsedMilliseconds;
                tableResult.Status = "Success";

                _logger.LogDebug("Synced {Table}: {Records} records in {Duration}ms",
                    tableName, recordsAffected, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                tableResult.DurationMs = sw.ElapsedMilliseconds;
                tableResult.Status = "Error";
                tableResult.ErrorMessage = ex.Message;

                _logger.LogError(ex, "Failed to sync {Table}", tableName);
            }

            results.Add(tableResult);
        }

        return results;
    }

    #region Dimension Sync Methods

    private async Task<int> SyncDimArtistAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_ARTIST tgt
            USING (
                SELECT artist_id, name, nationality, birth_year, death_year
                FROM ART_GALLERY_OLTP.Artist
            ) src
            ON (tgt.ARTIST_ID_OLTP = src.artist_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.NAME = src.name,
                    tgt.NATIONALITY = src.nationality,
                    tgt.BIRTH_YEAR = src.birth_year,
                    tgt.DEATH_YEAR = src.death_year
                WHERE tgt.NAME != src.name
                   OR NVL(tgt.NATIONALITY, '~') != NVL(src.nationality, '~')
                   OR NVL(tgt.BIRTH_YEAR, -1) != NVL(src.birth_year, -1)
                   OR NVL(tgt.DEATH_YEAR, -1) != NVL(src.death_year, -1)
            WHEN NOT MATCHED THEN
                INSERT (ARTIST_KEY, ARTIST_ID_OLTP, NAME, NATIONALITY, BIRTH_YEAR, DEATH_YEAR)
                VALUES (src.artist_id, src.artist_id, src.name, src.nationality, src.birth_year, src.death_year)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    private async Task<int> SyncDimCollectionAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_COLLECTION tgt
            USING (
                SELECT 
                    collection_id,
                    name,
                    description,
                    CASE WHEN created_date IS NOT NULL 
                         THEN TO_NUMBER(TO_CHAR(created_date, 'YYYYMMDD'))
                         ELSE NULL END AS created_date_key
                FROM ART_GALLERY_OLTP.Collection
            ) src
            ON (tgt.COLLECTION_ID_OLTP = src.collection_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.NAME = src.name,
                    tgt.DESCRIPTION = src.description,
                    tgt.CREATED_DATE_KEY = src.created_date_key
                WHERE tgt.NAME != src.name
                   OR NVL(tgt.DESCRIPTION, '~') != NVL(src.description, '~')
                   OR NVL(tgt.CREATED_DATE_KEY, -1) != NVL(src.created_date_key, -1)
            WHEN NOT MATCHED THEN
                INSERT (COLLECTION_KEY, COLLECTION_ID_OLTP, NAME, DESCRIPTION, CREATED_DATE_KEY)
                VALUES (src.collection_id, src.collection_id, src.name, src.description, src.created_date_key)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    private async Task<int> SyncDimLocationAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_LOCATION tgt
            USING (
                SELECT location_id, name, gallery_room, type, capacity
                FROM ART_GALLERY_OLTP.Location
            ) src
            ON (tgt.LOCATION_ID_OLTP = src.location_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.NAME = src.name,
                    tgt.GALLERY_ROOM = src.gallery_room,
                    tgt.TYPE = src.type,
                    tgt.CAPACITY = src.capacity
                WHERE tgt.NAME != src.name
                   OR NVL(tgt.GALLERY_ROOM, '~') != NVL(src.gallery_room, '~')
                   OR NVL(tgt.TYPE, '~') != NVL(src.type, '~')
                   OR NVL(tgt.CAPACITY, -1) != NVL(src.capacity, -1)
            WHEN NOT MATCHED THEN
                INSERT (LOCATION_KEY, LOCATION_ID_OLTP, NAME, GALLERY_ROOM, TYPE, CAPACITY)
                VALUES (src.location_id, src.location_id, src.name, src.gallery_room, src.type, src.capacity)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    private async Task<int> SyncDimExhibitorAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_EXHIBITOR tgt
            USING (
                SELECT exhibitor_id, name, address, city, contact_info
                FROM ART_GALLERY_OLTP.Exhibitor
            ) src
            ON (tgt.EXHIBITOR_ID_OLTP = src.exhibitor_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.NAME = src.name,
                    tgt.ADDRESS = src.address,
                    tgt.CITY = src.city,
                    tgt.CONTACT_INFO = src.contact_info
                WHERE tgt.NAME != src.name
                   OR NVL(tgt.ADDRESS, '~') != NVL(src.address, '~')
                   OR NVL(tgt.CITY, '~') != NVL(src.city, '~')
                   OR NVL(tgt.CONTACT_INFO, '~') != NVL(src.contact_info, '~')
            WHEN NOT MATCHED THEN
                INSERT (EXHIBITOR_KEY, EXHIBITOR_ID_OLTP, NAME, ADDRESS, CITY, CONTACT_INFO)
                VALUES (src.exhibitor_id, src.exhibitor_id, src.name, src.address, src.city, src.contact_info)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    private async Task<int> SyncDimPolicyAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_POLICY tgt
            USING (
                SELECT 
                    policy_id,
                    provider,
                    TO_NUMBER(TO_CHAR(start_date, 'YYYYMMDD')) AS start_date_key,
                    TO_NUMBER(TO_CHAR(end_date, 'YYYYMMDD')) AS end_date_key,
                    total_coverage_amount
                FROM ART_GALLERY_OLTP.Insurance_Policy
            ) src
            ON (tgt.POLICY_ID_OLTP = src.policy_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.PROVIDER = src.provider,
                    tgt.START_DATE_KEY = src.start_date_key,
                    tgt.END_DATE_KEY = src.end_date_key,
                    tgt.TOTAL_COVERAGE_AMT = src.total_coverage_amount
                WHERE tgt.PROVIDER != src.provider
                   OR NVL(tgt.START_DATE_KEY, -1) != NVL(src.start_date_key, -1)
                   OR NVL(tgt.END_DATE_KEY, -1) != NVL(src.end_date_key, -1)
                   OR NVL(tgt.TOTAL_COVERAGE_AMT, -1) != NVL(src.total_coverage_amount, -1)
            WHEN NOT MATCHED THEN
                INSERT (POLICY_KEY, POLICY_ID_OLTP, PROVIDER, START_DATE_KEY, END_DATE_KEY, TOTAL_COVERAGE_AMT)
                VALUES (src.policy_id, src.policy_id, src.provider, src.start_date_key, src.end_date_key, src.total_coverage_amount)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    private async Task<int> SyncDimExhibitionAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_EXHIBITION tgt
            USING (
                SELECT 
                    ex.exhibition_id,
                    ex.title,
                    TO_NUMBER(TO_CHAR(ex.start_date, 'YYYYMMDD')) AS start_date_key,
                    TO_NUMBER(TO_CHAR(ex.end_date, 'YYYYMMDD')) AS end_date_key,
                    de.EXHIBITOR_KEY,
                    ex.description
                FROM ART_GALLERY_OLTP.Exhibition ex
                JOIN DIM_EXHIBITOR de ON de.EXHIBITOR_ID_OLTP = ex.exhibitor_id
            ) src
            ON (tgt.EXHIBITION_ID_OLTP = src.exhibition_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.TITLE = src.title,
                    tgt.START_DATE_KEY = src.start_date_key,
                    tgt.END_DATE_KEY = src.end_date_key,
                    tgt.EXHIBITOR_KEY = src.EXHIBITOR_KEY,
                    tgt.DESCRIPTION = src.description
                WHERE tgt.TITLE != src.title
                   OR NVL(tgt.START_DATE_KEY, -1) != NVL(src.start_date_key, -1)
                   OR NVL(tgt.END_DATE_KEY, -1) != NVL(src.end_date_key, -1)
                   OR NVL(tgt.EXHIBITOR_KEY, -1) != NVL(src.EXHIBITOR_KEY, -1)
                   OR NVL(tgt.DESCRIPTION, '~') != NVL(src.description, '~')
            WHEN NOT MATCHED THEN
                INSERT (EXHIBITION_KEY, EXHIBITION_ID_OLTP, TITLE, START_DATE_KEY, END_DATE_KEY, EXHIBITOR_KEY, DESCRIPTION)
                VALUES (src.exhibition_id, src.exhibition_id, src.title, src.start_date_key, src.end_date_key, src.EXHIBITOR_KEY, src.description)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    private async Task<int> SyncDimArtworkAsync(CancellationToken ct)
    {
        const string sql = @"
            MERGE INTO DIM_ARTWORK tgt
            USING (
                SELECT 
                    aw.artwork_id,
                    aw.title,
                    da.ARTIST_KEY,
                    aw.year_created,
                    aw.medium,
                    dc.COLLECTION_KEY,
                    dl.LOCATION_KEY,
                    aw.estimated_value
                FROM ART_GALLERY_OLTP.Artwork aw
                JOIN DIM_ARTIST da ON da.ARTIST_ID_OLTP = aw.artist_id
                LEFT JOIN DIM_COLLECTION dc ON dc.COLLECTION_ID_OLTP = aw.collection_id
                LEFT JOIN DIM_LOCATION dl ON dl.LOCATION_ID_OLTP = aw.location_id
            ) src
            ON (tgt.ARTWORK_ID_OLTP = src.artwork_id)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.TITLE = src.title,
                    tgt.ARTIST_KEY = src.ARTIST_KEY,
                    tgt.YEAR_CREATED = src.year_created,
                    tgt.MEDIUM = src.medium,
                    tgt.COLLECTION_KEY = src.COLLECTION_KEY,
                    tgt.LOCATION_KEY = src.LOCATION_KEY,
                    tgt.ESTIMATED_VALUE = src.estimated_value
                WHERE tgt.TITLE != src.title
                   OR tgt.ARTIST_KEY != src.ARTIST_KEY
                   OR NVL(tgt.YEAR_CREATED, -1) != NVL(src.year_created, -1)
                   OR NVL(tgt.MEDIUM, '~') != NVL(src.medium, '~')
                   OR NVL(tgt.COLLECTION_KEY, -1) != NVL(src.COLLECTION_KEY, -1)
                   OR NVL(tgt.LOCATION_KEY, -1) != NVL(src.LOCATION_KEY, -1)
                   OR NVL(tgt.ESTIMATED_VALUE, -1) != NVL(src.estimated_value, -1)
            WHEN NOT MATCHED THEN
                INSERT (ARTWORK_KEY, ARTWORK_ID_OLTP, TITLE, ARTIST_KEY, YEAR_CREATED, MEDIUM, COLLECTION_KEY, LOCATION_KEY, ESTIMATED_VALUE)
                VALUES (src.artwork_id, src.artwork_id, src.title, src.ARTIST_KEY, src.year_created, src.medium, src.COLLECTION_KEY, src.LOCATION_KEY, src.estimated_value)";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    #endregion

    #region Fact Table Sync

    private async Task<int> SyncFactExhibitionActivityAsync(CancellationToken ct)
    {
        // This is the critical MERGE that propagates exhibition activity to the fact table
        const string sql = @"
            MERGE INTO FACT_EXHIBITION_ACTIVITY tgt
            USING (
                SELECT 
                    -- Surrogate Keys from Dimension Tables
                    TO_NUMBER(TO_CHAR(ex.start_date, 'YYYYMMDD')) AS DATE_KEY,
                    dex.EXHIBITION_KEY,
                    deb.EXHIBITOR_KEY,
                    daw.ARTWORK_KEY,
                    dar.ARTIST_KEY,
                    daw.COLLECTION_KEY,
                    daw.LOCATION_KEY,
                    dpol.POLICY_KEY,
                    
                    -- Measures
                    aw.estimated_value AS ESTIMATED_VALUE,
                    NVL(ins_agg.total_insured, 0) AS INSURED_AMOUNT,
                    CASE WHEN ln_agg.loan_count > 0 THEN 1 ELSE 0 END AS LOAN_FLAG,
                    NVL(res_agg.restoration_count, 0) AS RESTORATION_COUNT,
                    NVL(rv_agg.review_count, 0) AS REVIEW_COUNT,
                    NVL(rv_agg.avg_rating, 0) AS AVG_RATING
                    
                FROM ART_GALLERY_OLTP.Artwork_Exhibition ax
                
                -- Join OLTP tables
                JOIN ART_GALLERY_OLTP.Artwork aw ON aw.artwork_id = ax.artwork_id
                JOIN ART_GALLERY_OLTP.Exhibition ex ON ex.exhibition_id = ax.exhibition_id
                
                -- Lookup Dimension Surrogate Keys
                JOIN DIM_ARTWORK daw ON daw.ARTWORK_ID_OLTP = aw.artwork_id
                JOIN DIM_ARTIST dar ON dar.ARTIST_ID_OLTP = aw.artist_id
                JOIN DIM_EXHIBITION dex ON dex.EXHIBITION_ID_OLTP = ex.exhibition_id
                JOIN DIM_EXHIBITOR deb ON deb.EXHIBITOR_ID_OLTP = ex.exhibitor_id
                
                -- Insurance aggregation
                LEFT JOIN (
                    SELECT artwork_id, SUM(insured_amount) AS total_insured, MIN(policy_id) AS primary_policy_id
                    FROM ART_GALLERY_OLTP.Insurance
                    GROUP BY artwork_id
                ) ins_agg ON ins_agg.artwork_id = aw.artwork_id
                
                -- Policy lookup
                LEFT JOIN DIM_POLICY dpol ON dpol.POLICY_ID_OLTP = ins_agg.primary_policy_id
                
                -- Loan count
                LEFT JOIN (
                    SELECT artwork_id, COUNT(*) AS loan_count
                    FROM ART_GALLERY_OLTP.Loan
                    GROUP BY artwork_id
                ) ln_agg ON ln_agg.artwork_id = aw.artwork_id
                
                -- Restoration count
                LEFT JOIN (
                    SELECT artwork_id, COUNT(*) AS restoration_count
                    FROM ART_GALLERY_OLTP.Restoration
                    GROUP BY artwork_id
                ) res_agg ON res_agg.artwork_id = aw.artwork_id
                
                -- Review aggregation
                LEFT JOIN (
                    SELECT exhibition_id, artwork_id, COUNT(*) AS review_count, AVG(rating) AS avg_rating
                    FROM ART_GALLERY_OLTP.Gallery_Review
                    GROUP BY exhibition_id, artwork_id
                ) rv_agg ON rv_agg.exhibition_id = ex.exhibition_id 
                        AND (rv_agg.artwork_id = aw.artwork_id)
            ) src
            ON (tgt.ARTWORK_KEY = src.ARTWORK_KEY AND tgt.EXHIBITION_KEY = src.EXHIBITION_KEY)
            WHEN MATCHED THEN
                UPDATE SET
                    tgt.DATE_KEY = src.DATE_KEY,
                    tgt.EXHIBITOR_KEY = src.EXHIBITOR_KEY,
                    tgt.ARTIST_KEY = src.ARTIST_KEY,
                    tgt.COLLECTION_KEY = src.COLLECTION_KEY,
                    tgt.LOCATION_KEY = src.LOCATION_KEY,
                    tgt.POLICY_KEY = src.POLICY_KEY,
                    tgt.ESTIMATED_VALUE = src.ESTIMATED_VALUE,
                    tgt.INSURED_AMOUNT = src.INSURED_AMOUNT,
                    tgt.LOAN_FLAG = src.LOAN_FLAG,
                    tgt.RESTORATION_COUNT = src.RESTORATION_COUNT,
                    tgt.REVIEW_COUNT = src.REVIEW_COUNT,
                    tgt.AVG_RATING = src.AVG_RATING
                WHERE NVL(tgt.ESTIMATED_VALUE, -1) != NVL(src.ESTIMATED_VALUE, -1)
                   OR NVL(tgt.INSURED_AMOUNT, -1) != NVL(src.INSURED_AMOUNT, -1)
                   OR NVL(tgt.LOAN_FLAG, -1) != NVL(src.LOAN_FLAG, -1)
                   OR NVL(tgt.RESTORATION_COUNT, -1) != NVL(src.RESTORATION_COUNT, -1)
                   OR NVL(tgt.REVIEW_COUNT, -1) != NVL(src.REVIEW_COUNT, -1)
                   OR NVL(tgt.AVG_RATING, -1) != NVL(src.AVG_RATING, -1)
                   OR NVL(tgt.POLICY_KEY, -1) != NVL(src.POLICY_KEY, -1)
            WHEN NOT MATCHED THEN
                INSERT (
                    FACT_KEY, DATE_KEY, EXHIBITION_KEY, EXHIBITOR_KEY, ARTWORK_KEY,
                    ARTIST_KEY, COLLECTION_KEY, LOCATION_KEY, POLICY_KEY,
                    ESTIMATED_VALUE, INSURED_AMOUNT, LOAN_FLAG,
                    RESTORATION_COUNT, REVIEW_COUNT, AVG_RATING
                )
                VALUES (
                    (SELECT NVL(MAX(FACT_KEY), 0) + 1 FROM FACT_EXHIBITION_ACTIVITY),
                    src.DATE_KEY, src.EXHIBITION_KEY, src.EXHIBITOR_KEY, src.ARTWORK_KEY,
                    src.ARTIST_KEY, src.COLLECTION_KEY, src.LOCATION_KEY, src.POLICY_KEY,
                    src.ESTIMATED_VALUE, src.INSURED_AMOUNT, src.LOAN_FLAG,
                    src.RESTORATION_COUNT, src.REVIEW_COUNT, src.AVG_RATING
                )";

        return await _dwContext.Database.ExecuteSqlRawAsync(sql, ct);
    }

    #endregion

    public async Task<int> ExecuteProcedureAsync(
        string procedureName,
        Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Executing procedure: {Procedure}", procedureName);

        var connection = _dwContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedureName;
            command.CommandTimeout = _configuration.GetValue<int>("Oracle:CommandTimeout", 60);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = param.Key;
                    dbParam.Value = param.Value ?? DBNull.Value;
                    command.Parameters.Add(dbParam);
                }
            }

            // Add output parameter for records processed
            var outputParam = command.CreateParameter();
            outputParam.ParameterName = "p_result";
            outputParam.Direction = ParameterDirection.ReturnValue;
            outputParam.DbType = DbType.Int32;
            command.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync(cancellationToken);

            return outputParam.Value != DBNull.Value ? Convert.ToInt32(outputParam.Value) : 0;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    public async Task<DataIntegrityResult> ValidateReferentialIntegrityAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new DataIntegrityResult
        {
            CheckedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Starting referential integrity validation");

        try
        {
            var checks = new List<(string Name, string Sql)>
            {
                ("Orphan Artworks in DW", @"
                    SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_ARTWORK da
                    WHERE da.IS_CURRENT = 1
                    AND NOT EXISTS (
                        SELECT 1 FROM ART_GALLERY_OLTP.ARTWORKS a WHERE a.ID = da.ARTWORK_NK
                    )"),
                ("Missing Artist References", @"
                    SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_ARTWORK da
                    WHERE da.ARTIST_KEY IS NOT NULL
                    AND NOT EXISTS (
                        SELECT 1 FROM ART_GALLERY_DW.DIM_ARTIST ar WHERE ar.ARTIST_KEY = da.ARTIST_KEY
                    )"),
                ("Orphan Fact Records", @"
                    SELECT COUNT(*) FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
                    WHERE NOT EXISTS (
                        SELECT 1 FROM ART_GALLERY_DW.DIM_EXHIBITION e WHERE e.EXHIBITION_KEY = f.EXHIBITION_KEY
                    )"),
                ("Invalid Date Keys", @"
                    SELECT COUNT(*) FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY f
                    WHERE NOT EXISTS (
                        SELECT 1 FROM ART_GALLERY_DW.DIM_DATE d WHERE d.DATE_KEY = f.DATE_KEY
                    )")
            };

            foreach (var (name, sql) in checks)
            {
                result.TotalChecks++;
                
                try
                {
                    var count = await _dwContext.Database
                        .SqlQueryRaw<int>(sql)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (count > 0)
                    {
                        result.FailedChecks++;
                        result.Issues.Add(new IntegrityIssue
                        {
                            TableName = name.Split(' ')[0],
                            IssueType = "ReferentialIntegrity",
                            Description = name,
                            AffectedRecords = count,
                            Severity = count > 100 ? "Error" : "Warning"
                        });
                    }
                    else
                    {
                        result.PassedChecks++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Integrity check failed: {Check}", name);
                    result.Issues.Add(new IntegrityIssue
                    {
                        TableName = "N/A",
                        IssueType = "CheckError",
                        Description = $"Failed to execute check '{name}': {ex.Message}",
                        Severity = "Error"
                    });
                }
            }

            result.IsValid = result.FailedChecks == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Referential integrity validation failed");
            result.Issues.Add(new IntegrityIssue
            {
                IssueType = "SystemError",
                Description = ex.Message,
                Severity = "Critical"
            });
        }

        return result;
    }

    public async Task<string> GetExplainPlanAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting explain plan for query");

        var planId = Guid.NewGuid().ToString("N")[..10];
        var explainSql = $"EXPLAIN PLAN SET STATEMENT_ID = '{planId}' FOR {sql}";

        try
        {
            await _dwContext.Database.ExecuteSqlRawAsync(explainSql, cancellationToken);

            var planQuery = $@"
                SELECT PLAN_TABLE_OUTPUT 
                FROM TABLE(DBMS_XPLAN.DISPLAY('PLAN_TABLE', '{planId}', 'ALL'))";

            var planLines = await _dwContext.Database
                .SqlQueryRaw<string>(planQuery)
                .ToListAsync(cancellationToken);

            return string.Join(Environment.NewLine, planLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get explain plan");
            return $"Error getting explain plan: {ex.Message}";
        }
    }
}
