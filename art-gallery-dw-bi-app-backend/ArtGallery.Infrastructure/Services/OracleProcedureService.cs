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
            _logger.LogInformation("Starting ETL propagation. Mode: {Mode}", mode);

            // Get connection from DW context
            var connection = _dwContext.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);

            try
            {
                using var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ART_GALLERY_DW.PKG_ETL.PROPAGATE_OLTP_TO_DW";
                command.CommandTimeout = _configuration.GetValue<int>("EtlSettings:TimeoutSeconds", 300);

                // Add parameters
                var modeParam = command.CreateParameter();
                modeParam.ParameterName = "p_mode";
                modeParam.Value = mode == EtlMode.Full ? "FULL" : "INCREMENTAL";
                modeParam.Direction = ParameterDirection.Input;
                command.Parameters.Add(modeParam);

                var recordsParam = command.CreateParameter();
                recordsParam.ParameterName = "p_records_loaded";
                recordsParam.Direction = ParameterDirection.Output;
                recordsParam.DbType = DbType.Int32;
                command.Parameters.Add(recordsParam);

                var statusParam = command.CreateParameter();
                statusParam.ParameterName = "p_status";
                statusParam.Direction = ParameterDirection.Output;
                statusParam.DbType = DbType.String;
                statusParam.Size = 50;
                command.Parameters.Add(statusParam);

                await command.ExecuteNonQueryAsync(cancellationToken);

                result.LoadedRecords = recordsParam.Value != DBNull.Value 
                    ? Convert.ToInt32(recordsParam.Value) 
                    : 0;
                result.Status = statusParam.Value?.ToString() ?? "Success";
            }
            finally
            {
                await connection.CloseAsync();
            }

            // Sync individual dimension tables
            result.TableResults = await SyncDimensionTablesAsync(mode, cancellationToken);

            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.EndTime = DateTime.UtcNow;
            result.Status = "Success";

            _logger.LogInformation(
                "ETL propagation completed. Records: {Records}, Duration: {Duration}ms",
                result.LoadedRecords, result.DurationMs);
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

    private async Task<List<TableSyncResult>> SyncDimensionTablesAsync(
        EtlMode mode, 
        CancellationToken cancellationToken)
    {
        var results = new List<TableSyncResult>();
        var tables = new[] 
        { 
            "DIM_ARTIST", "DIM_ARTWORK", "DIM_EXHIBITION", 
            "DIM_VISITOR", "DIM_STAFF", "DIM_INSURANCE", 
            "DIM_LOCATION", "FACT_EXHIBITION_ACTIVITY" 
        };

        foreach (var table in tables)
        {
            var tableResult = new TableSyncResult { TableName = table };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Execute table-specific sync procedure
                var procName = $"ART_GALLERY_DW.PKG_ETL.SYNC_{table}";
                var recordsProcessed = await ExecuteProcedureAsync(
                    procName,
                    new Dictionary<string, object?> { { "p_mode", mode.ToString().ToUpper() } },
                    cancellationToken);

                stopwatch.Stop();
                tableResult.RecordsProcessed = recordsProcessed;
                tableResult.DurationMs = stopwatch.ElapsedMilliseconds;
                tableResult.Status = "Success";
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                tableResult.DurationMs = stopwatch.ElapsedMilliseconds;
                tableResult.Status = "Error";
                tableResult.ErrorMessage = ex.Message;
                
                _logger.LogWarning(ex, "Failed to sync table {Table}", table);
            }

            results.Add(tableResult);
        }

        return results;
    }

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
