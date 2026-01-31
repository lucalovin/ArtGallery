using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;

namespace ArtGallery.Infrastructure.Services;

/// <summary>
/// Code-based ETL service that propagates data from OLTP to DW using raw SQL.
/// Uses only columns that actually exist in the Oracle DW schema.
/// </summary>
public class CodeBasedEtlService : ICodeBasedEtlService
{
    private readonly AppDbContext _oltpContext;
    private readonly DwDbContext _dwContext;
    private readonly ILogger<CodeBasedEtlService> _logger;

    public CodeBasedEtlService(
        AppDbContext oltpContext,
        DwDbContext dwContext,
        ILogger<CodeBasedEtlService> logger)
    {
        _oltpContext = oltpContext;
        _dwContext = dwContext;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a table exists in the DW schema.
    /// </summary>
    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM ALL_TABLES WHERE OWNER = 'ART_GALLERY_DW' AND TABLE_NAME = :tableName";
            var count = await ExecuteScalarCountAsync(sql,
                new[] { new OracleParameter("tableName", tableName) },
                cancellationToken);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking if table {TableName} exists", tableName);
            return false;
        }
    }

    /// <summary>
    /// Executes a scalar count query using ADO.NET directly.
    /// </summary>
    private async Task<int> ExecuteScalarCountAsync(string sql, OracleParameter[] parameters, CancellationToken cancellationToken)
    {
        var connection = _dwContext.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        
        foreach (var param in parameters)
        {
            var dbParam = command.CreateParameter();
            dbParam.ParameterName = param.ParameterName;
            dbParam.Value = param.Value ?? DBNull.Value;
            command.Parameters.Add(dbParam);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public async Task<EtlResult> PropagateAllAsync(bool fullLoad = false, CancellationToken cancellationToken = default)
    {
        var result = new EtlResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting code-based ETL propagation. Full load: {FullLoad}", fullLoad);

            // Propagate dimension tables in order (respecting dependencies)
            // Only propagate if the table exists in the DW schema
            if (await TableExistsAsync("DIM_ARTIST", cancellationToken))
            {
                result.ArtistsProcessed = await PropagateArtistsAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_ARTIST table does not exist, skipping artist propagation");
            }

            if (await TableExistsAsync("DIM_ARTWORK", cancellationToken))
            {
                result.ArtworksProcessed = await PropagateArtworksAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_ARTWORK table does not exist, skipping artwork propagation");
            }

            if (await TableExistsAsync("DIM_EXHIBITION", cancellationToken))
            {
                result.ExhibitionsProcessed = await PropagateExhibitionsAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_EXHIBITION table does not exist, skipping exhibition propagation");
            }

            if (await TableExistsAsync("DIM_VISITOR", cancellationToken))
            {
                result.VisitorsProcessed = await PropagateVisitorsAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_VISITOR table does not exist, skipping visitor propagation");
            }

            if (await TableExistsAsync("DIM_STAFF", cancellationToken))
            {
                result.StaffProcessed = await PropagateStaffAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_STAFF table does not exist, skipping staff propagation");
            }
            
            // Populate fact table
            if (await TableExistsAsync("FACT_EXHIBITION_ACTIVITY", cancellationToken))
            {
                result.FactRecordsProcessed = await PopulateFactTableAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("FACT_EXHIBITION_ACTIVITY table does not exist, skipping fact table population");
            }

            result.TotalRecordsProcessed = 
                result.ArtistsProcessed + 
                result.ArtworksProcessed + 
                result.ExhibitionsProcessed + 
                result.VisitorsProcessed + 
                result.StaffProcessed + 
                result.FactRecordsProcessed;

            result.Success = true;
            
            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "ETL propagation completed. Total records: {Total}, Duration: {Duration}ms",
                result.TotalRecordsProcessed, result.DurationMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "ETL propagation failed");
        }

        return result;
    }

    public async Task<int> PropagateArtistsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating artists from OLTP to DW");
        var count = 0;

        try
        {
            var oltpArtists = await _oltpContext.Artists.AsNoTracking().ToListAsync(cancellationToken);

            foreach (var artist in oltpArtists)
            {
                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_ARTIST WHERE ARTIST_KEY = :artistKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql, 
                    new[] { new OracleParameter("artistKey", artist.Id) }, 
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = "INSERT INTO ART_GALLERY_DW.DIM_ARTIST (ARTIST_KEY, ARTIST_ID_OLTP, NAME) VALUES (:artistKey, :artistIdOltp, :name)";
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("artistKey", artist.Id),
                        new OracleParameter("artistIdOltp", artist.Id),
                        new OracleParameter("name", artist.Name ?? "Unknown"));
                    count++;
                }
                else
                {
                    var updateSql = "UPDATE ART_GALLERY_DW.DIM_ARTIST SET ARTIST_ID_OLTP = :artistIdOltp, NAME = :name WHERE ARTIST_KEY = :artistKey";
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("artistIdOltp", artist.Id),
                        new OracleParameter("name", artist.Name ?? "Unknown"),
                        new OracleParameter("artistKey", artist.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} artists", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating artists");
            throw;
        }

        return count;
    }

    public async Task<int> PropagateArtworksAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating artworks from OLTP to DW");
        var count = 0;

        try
        {
            var oltpArtworks = await _oltpContext.Artworks
                .Include(a => a.Artist)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var artwork in oltpArtworks)
            {
                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_ARTWORK WHERE ARTWORK_KEY = :artworkKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("artworkKey", artwork.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_ARTWORK 
                        (ARTWORK_KEY, ARTWORK_ID_OLTP, TITLE, ARTIST_KEY, YEAR_CREATED, MEDIUM, ESTIMATED_VALUE) 
                        VALUES (:artworkKey, :artworkIdOltp, :title, :artistKey, :yearCreated, :medium, :estimatedValue)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("artworkKey", artwork.Id),
                        new OracleParameter("artworkIdOltp", artwork.Id),
                        new OracleParameter("title", artwork.Title ?? "Unknown"),
                        new OracleParameter("artistKey", artwork.ArtistId),
                        new OracleParameter("yearCreated", (object?)artwork.YearCreated ?? DBNull.Value),
                        new OracleParameter("medium", (object?)artwork.Medium ?? DBNull.Value),
                        new OracleParameter("estimatedValue", (object?)artwork.EstimatedValue ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_ARTWORK 
                        SET ARTWORK_ID_OLTP = :artworkIdOltp, TITLE = :title, ARTIST_KEY = :artistKey, YEAR_CREATED = :yearCreated, 
                            MEDIUM = :medium, ESTIMATED_VALUE = :estimatedValue 
                        WHERE ARTWORK_KEY = :artworkKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("artworkIdOltp", artwork.Id),
                        new OracleParameter("title", artwork.Title ?? "Unknown"),
                        new OracleParameter("artistKey", artwork.ArtistId),
                        new OracleParameter("yearCreated", (object?)artwork.YearCreated ?? DBNull.Value),
                        new OracleParameter("medium", (object?)artwork.Medium ?? DBNull.Value),
                        new OracleParameter("estimatedValue", (object?)artwork.EstimatedValue ?? DBNull.Value),
                        new OracleParameter("artworkKey", artwork.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} artworks", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating artworks");
            throw;
        }

        return count;
    }

    public async Task<int> PropagateExhibitionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating exhibitions from OLTP to DW");
        var count = 0;

        try
        {
            var oltpExhibitions = await _oltpContext.Exhibitions
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var exhibition in oltpExhibitions)
            {
                // Convert dates to date keys (format: YYYYMMDD as integer)
                var startDateKey = exhibition.StartDate.Year * 10000 + exhibition.StartDate.Month * 100 + exhibition.StartDate.Day;
                var endDateKey = exhibition.EndDate.Year * 10000 + exhibition.EndDate.Month * 100 + exhibition.EndDate.Day;

                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_EXHIBITION WHERE EXHIBITION_KEY = :exhibitionKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("exhibitionKey", exhibition.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_EXHIBITION 
                        (EXHIBITION_KEY, EXHIBITION_ID_OLTP, TITLE, START_DATE_KEY, END_DATE_KEY) 
                        VALUES (:exhibitionKey, :exhibitionIdOltp, :title, :startDateKey, :endDateKey)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("exhibitionKey", exhibition.Id),
                        new OracleParameter("exhibitionIdOltp", exhibition.Id),
                        new OracleParameter("title", exhibition.Title ?? "Unknown"),
                        new OracleParameter("startDateKey", startDateKey),
                        new OracleParameter("endDateKey", endDateKey));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_EXHIBITION 
                        SET EXHIBITION_ID_OLTP = :exhibitionIdOltp, TITLE = :title, START_DATE_KEY = :startDateKey, END_DATE_KEY = :endDateKey 
                        WHERE EXHIBITION_KEY = :exhibitionKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("exhibitionIdOltp", exhibition.Id),
                        new OracleParameter("title", exhibition.Title ?? "Unknown"),
                        new OracleParameter("startDateKey", startDateKey),
                        new OracleParameter("endDateKey", endDateKey),
                        new OracleParameter("exhibitionKey", exhibition.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} exhibitions", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating exhibitions");
            throw;
        }

        return count;
    }

    public async Task<int> PropagateVisitorsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating visitors from OLTP to DW");
        var count = 0;

        try
        {
            var oltpVisitors = await _oltpContext.Visitors
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var visitor in oltpVisitors)
            {
                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_VISITOR WHERE VISITOR_KEY = :visitorKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("visitorKey", visitor.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_VISITOR 
                        (VISITOR_KEY, FULL_NAME, EMAIL, MEMBERSHIP_TYPE) 
                        VALUES (:visitorKey, :fullName, :email, :membershipType)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("visitorKey", visitor.Id),
                        new OracleParameter("fullName", visitor.Name ?? "Unknown"),
                        new OracleParameter("email", (object?)visitor.Email ?? DBNull.Value),
                        new OracleParameter("membershipType", (object?)visitor.MembershipType ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_VISITOR 
                        SET FULL_NAME = :fullName, EMAIL = :email, MEMBERSHIP_TYPE = :membershipType 
                        WHERE VISITOR_KEY = :visitorKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("fullName", visitor.Name ?? "Unknown"),
                        new OracleParameter("email", (object?)visitor.Email ?? DBNull.Value),
                        new OracleParameter("membershipType", (object?)visitor.MembershipType ?? DBNull.Value),
                        new OracleParameter("visitorKey", visitor.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} visitors", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating visitors");
            throw;
        }

        return count;
    }

    public async Task<int> PropagateStaffAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating staff from OLTP to DW");
        var count = 0;

        try
        {
            var oltpStaff = await _oltpContext.Staff
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var staff in oltpStaff)
            {
                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_STAFF WHERE STAFF_KEY = :staffKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("staffKey", staff.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_STAFF 
                        (STAFF_KEY, FULL_NAME, JOB_TITLE, HIRE_DATE) 
                        VALUES (:staffKey, :fullName, :jobTitle, :hireDate)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("staffKey", staff.Id),
                        new OracleParameter("fullName", staff.Name ?? "Unknown"),
                        new OracleParameter("jobTitle", (object?)staff.Role ?? DBNull.Value),
                        new OracleParameter("hireDate", staff.HireDate));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_STAFF 
                        SET FULL_NAME = :fullName, JOB_TITLE = :jobTitle, HIRE_DATE = :hireDate 
                        WHERE STAFF_KEY = :staffKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("fullName", staff.Name ?? "Unknown"),
                        new OracleParameter("jobTitle", (object?)staff.Role ?? DBNull.Value),
                        new OracleParameter("hireDate", staff.HireDate),
                        new OracleParameter("staffKey", staff.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} staff", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating staff");
            throw;
        }

        return count;
    }

    public async Task<int> PopulateFactTableAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Populating fact table");
        var count = 0;

        try
        {
            var oltpArtworks = await _oltpContext.Artworks
                .Include(a => a.ExhibitionArtworks)
                .ThenInclude(ea => ea.Exhibition)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Get current date key
            var today = DateTime.Today;
            var dateKey = today.Year * 10000 + today.Month * 100 + today.Day;

            // Get current max FACT_KEY to generate new keys
            var maxFactKeySql = "SELECT NVL(MAX(FACT_KEY), 0) FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY";
            var maxFactKey = await ExecuteScalarCountAsync(maxFactKeySql, Array.Empty<OracleParameter>(), cancellationToken);

            foreach (var artwork in oltpArtworks)
            {
                foreach (var ea in artwork.ExhibitionArtworks)
                {
                    var exhibitionId = ea.ExhibitionId;

                    var existsSql = @"SELECT COUNT(*) FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY 
                        WHERE ARTWORK_KEY = :artworkKey AND EXHIBITION_KEY = :exhibitionKey";
                    
                    var existsCount = await ExecuteScalarCountAsync(existsSql,
                        new[] { 
                            new OracleParameter("artworkKey", artwork.Id),
                            new OracleParameter("exhibitionKey", exhibitionId)
                        },
                        cancellationToken);

                    if (existsCount == 0)
                    {
                        maxFactKey++;
                        
                        // Get exhibitor_key from the exhibition (default to 1 if not set)
                        var exhibitorKey = ea.Exhibition?.ExhibitorId ?? 1;
                        
                        var insertSql = @"INSERT INTO ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY 
                            (FACT_KEY, DATE_KEY, ARTWORK_KEY, ARTIST_KEY, EXHIBITION_KEY, EXHIBITOR_KEY, ESTIMATED_VALUE, INSURED_AMOUNT) 
                            VALUES (:factKey, :dateKey, :artworkKey, :artistKey, :exhibitionKey, :exhibitorKey, :estimatedValue, :insuredAmount)";
                        
                        await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                            new OracleParameter("factKey", maxFactKey),
                            new OracleParameter("dateKey", dateKey),
                            new OracleParameter("artworkKey", artwork.Id),
                            new OracleParameter("artistKey", artwork.ArtistId),
                            new OracleParameter("exhibitionKey", exhibitionId),
                            new OracleParameter("exhibitorKey", exhibitorKey),
                            new OracleParameter("estimatedValue", (object?)artwork.EstimatedValue ?? DBNull.Value),
                            new OracleParameter("insuredAmount", (object?)artwork.EstimatedValue ?? DBNull.Value));
                        count++;
                    }
                }
            }

            _logger.LogDebug("Created {Count} fact records", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating fact table");
            throw;
        }

        return count;
    }
}
