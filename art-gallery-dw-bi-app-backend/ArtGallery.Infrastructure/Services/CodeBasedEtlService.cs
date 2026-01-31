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
    /// Uses USER_TABLES for owned tables or tries direct query as fallback.
    /// </summary>
    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            // First try USER_TABLES (tables owned by current user)
            var sql = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :tableName";
            var count = await ExecuteScalarCountAsync(sql,
                new[] { new OracleParameter("tableName", tableName.ToUpper()) },
                cancellationToken);
            
            if (count > 0)
            {
                return true;
            }
            
            // Fallback: try ALL_TABLES with schema prefix
            sql = "SELECT COUNT(*) FROM ALL_TABLES WHERE OWNER = 'ART_GALLERY_DW' AND TABLE_NAME = :tableName";
            count = await ExecuteScalarCountAsync(sql,
                new[] { new OracleParameter("tableName", tableName.ToUpper()) },
                cancellationToken);
            
            if (count > 0)
            {
                return true;
            }
            
            // Last resort: try to select from the table directly (will fail if doesn't exist)
            try
            {
                sql = $"SELECT 1 FROM {tableName} WHERE ROWNUM = 1";
                await ExecuteScalarCountAsync(sql, Array.Empty<OracleParameter>(), cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
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

            // Propagate collections before artworks (artworks reference collections)
            if (await TableExistsAsync("DIM_COLLECTION", cancellationToken))
            {
                result.CollectionsProcessed = await PropagateCollectionsAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_COLLECTION table does not exist, skipping collection propagation");
            }

            // Propagate locations before artworks (artworks reference locations)
            if (await TableExistsAsync("DIM_LOCATION", cancellationToken))
            {
                result.LocationsProcessed = await PropagateLocationsAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_LOCATION table does not exist, skipping location propagation");
            }

            // Propagate exhibitors before exhibitions (exhibitions reference exhibitors)
            if (await TableExistsAsync("DIM_EXHIBITOR", cancellationToken))
            {
                result.ExhibitorsProcessed = await PropagateExhibitorsAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_EXHIBITOR table does not exist, skipping exhibitor propagation");
            }

            // Propagate policies before fact table (fact references policies)
            if (await TableExistsAsync("DIM_POLICY", cancellationToken))
            {
                result.PoliciesProcessed = await PropagatePoliciesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("DIM_POLICY table does not exist, skipping policy propagation");
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
                result.CollectionsProcessed +
                result.LocationsProcessed +
                result.ExhibitorsProcessed +
                result.PoliciesProcessed +
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
                        (ARTWORK_KEY, ARTWORK_ID_OLTP, TITLE, ARTIST_KEY, YEAR_CREATED, MEDIUM, COLLECTION_KEY, LOCATION_KEY, ESTIMATED_VALUE) 
                        VALUES (:artworkKey, :artworkIdOltp, :title, :artistKey, :yearCreated, :medium, :collectionKey, :locationKey, :estimatedValue)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("artworkKey", artwork.Id),
                        new OracleParameter("artworkIdOltp", artwork.Id),
                        new OracleParameter("title", artwork.Title ?? "Unknown"),
                        new OracleParameter("artistKey", artwork.ArtistId),
                        new OracleParameter("yearCreated", (object?)artwork.YearCreated ?? DBNull.Value),
                        new OracleParameter("medium", (object?)artwork.Medium ?? DBNull.Value),
                        new OracleParameter("collectionKey", (object?)artwork.CollectionId ?? DBNull.Value),
                        new OracleParameter("locationKey", (object?)artwork.LocationId ?? DBNull.Value),
                        new OracleParameter("estimatedValue", (object?)artwork.EstimatedValue ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_ARTWORK 
                        SET ARTWORK_ID_OLTP = :artworkIdOltp, TITLE = :title, ARTIST_KEY = :artistKey, YEAR_CREATED = :yearCreated, 
                            MEDIUM = :medium, COLLECTION_KEY = :collectionKey, LOCATION_KEY = :locationKey, ESTIMATED_VALUE = :estimatedValue 
                        WHERE ARTWORK_KEY = :artworkKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("artworkIdOltp", artwork.Id),
                        new OracleParameter("title", artwork.Title ?? "Unknown"),
                        new OracleParameter("artistKey", artwork.ArtistId),
                        new OracleParameter("yearCreated", (object?)artwork.YearCreated ?? DBNull.Value),
                        new OracleParameter("medium", (object?)artwork.Medium ?? DBNull.Value),
                        new OracleParameter("collectionKey", (object?)artwork.CollectionId ?? DBNull.Value),
                        new OracleParameter("locationKey", (object?)artwork.LocationId ?? DBNull.Value),
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

    public async Task<int> PropagateCollectionsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating collections from OLTP to DW");
        var count = 0;

        try
        {
            var oltpCollections = await _oltpContext.Collections.AsNoTracking().ToListAsync(cancellationToken);

            foreach (var collection in oltpCollections)
            {
                // Convert date to date key (format: YYYYMMDD as integer)
                int? createdDateKey = null;
                if (collection.CreatedDate.HasValue)
                {
                    var date = collection.CreatedDate.Value;
                    createdDateKey = date.Year * 10000 + date.Month * 100 + date.Day;
                }

                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_COLLECTION WHERE COLLECTION_KEY = :collectionKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("collectionKey", collection.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_COLLECTION 
                        (COLLECTION_KEY, COLLECTION_ID_OLTP, NAME, DESCRIPTION, CREATED_DATE_KEY) 
                        VALUES (:collectionKey, :collectionIdOltp, :name, :description, :createdDateKey)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("collectionKey", collection.Id),
                        new OracleParameter("collectionIdOltp", collection.Id),
                        new OracleParameter("name", collection.Name ?? "Unknown"),
                        new OracleParameter("description", (object?)collection.Description ?? DBNull.Value),
                        new OracleParameter("createdDateKey", (object?)createdDateKey ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_COLLECTION 
                        SET COLLECTION_ID_OLTP = :collectionIdOltp, NAME = :name, DESCRIPTION = :description, CREATED_DATE_KEY = :createdDateKey 
                        WHERE COLLECTION_KEY = :collectionKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("collectionIdOltp", collection.Id),
                        new OracleParameter("name", collection.Name ?? "Unknown"),
                        new OracleParameter("description", (object?)collection.Description ?? DBNull.Value),
                        new OracleParameter("createdDateKey", (object?)createdDateKey ?? DBNull.Value),
                        new OracleParameter("collectionKey", collection.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} collections", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating collections");
            throw;
        }

        return count;
    }

    public async Task<int> PropagateLocationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating locations from OLTP to DW");
        var count = 0;

        try
        {
            var oltpLocations = await _oltpContext.Locations.AsNoTracking().ToListAsync(cancellationToken);

            foreach (var location in oltpLocations)
            {
                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_LOCATION WHERE LOCATION_KEY = :locationKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("locationKey", location.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_LOCATION 
                        (LOCATION_KEY, LOCATION_ID_OLTP, NAME, GALLERY_ROOM, TYPE, CAPACITY) 
                        VALUES (:locationKey, :locationIdOltp, :name, :galleryRoom, :locationType, :capacity)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("locationKey", location.Id),
                        new OracleParameter("locationIdOltp", location.Id),
                        new OracleParameter("name", location.Name ?? "Unknown"),
                        new OracleParameter("galleryRoom", (object?)location.GalleryRoom ?? DBNull.Value),
                        new OracleParameter("locationType", (object?)location.Type ?? DBNull.Value),
                        new OracleParameter("capacity", (object?)location.Capacity ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_LOCATION 
                        SET LOCATION_ID_OLTP = :locationIdOltp, NAME = :name, GALLERY_ROOM = :galleryRoom, TYPE = :locationType, CAPACITY = :capacity 
                        WHERE LOCATION_KEY = :locationKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("locationIdOltp", location.Id),
                        new OracleParameter("name", location.Name ?? "Unknown"),
                        new OracleParameter("galleryRoom", (object?)location.GalleryRoom ?? DBNull.Value),
                        new OracleParameter("locationType", (object?)location.Type ?? DBNull.Value),
                        new OracleParameter("capacity", (object?)location.Capacity ?? DBNull.Value),
                        new OracleParameter("locationKey", location.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} locations", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating locations");
            throw;
        }

        return count;
    }

    public async Task<int> PropagateExhibitorsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating exhibitors from OLTP to DW");
        var count = 0;

        try
        {
            var oltpExhibitors = await _oltpContext.Exhibitors.AsNoTracking().ToListAsync(cancellationToken);

            foreach (var exhibitor in oltpExhibitors)
            {
                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_EXHIBITOR WHERE EXHIBITOR_KEY = :exhibitorKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("exhibitorKey", exhibitor.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_EXHIBITOR 
                        (EXHIBITOR_KEY, EXHIBITOR_ID_OLTP, NAME, ADDRESS, CITY, CONTACT_INFO) 
                        VALUES (:exhibitorKey, :exhibitorIdOltp, :name, :address, :city, :contactInfo)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("exhibitorKey", exhibitor.Id),
                        new OracleParameter("exhibitorIdOltp", exhibitor.Id),
                        new OracleParameter("name", exhibitor.Name ?? "Unknown"),
                        new OracleParameter("address", (object?)exhibitor.Address ?? DBNull.Value),
                        new OracleParameter("city", (object?)exhibitor.City ?? DBNull.Value),
                        new OracleParameter("contactInfo", (object?)exhibitor.ContactInfo ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_EXHIBITOR 
                        SET EXHIBITOR_ID_OLTP = :exhibitorIdOltp, NAME = :name, ADDRESS = :address, CITY = :city, CONTACT_INFO = :contactInfo 
                        WHERE EXHIBITOR_KEY = :exhibitorKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("exhibitorIdOltp", exhibitor.Id),
                        new OracleParameter("name", exhibitor.Name ?? "Unknown"),
                        new OracleParameter("address", (object?)exhibitor.Address ?? DBNull.Value),
                        new OracleParameter("city", (object?)exhibitor.City ?? DBNull.Value),
                        new OracleParameter("contactInfo", (object?)exhibitor.ContactInfo ?? DBNull.Value),
                        new OracleParameter("exhibitorKey", exhibitor.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} exhibitors", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating exhibitors");
            throw;
        }

        return count;
    }

    public async Task<int> PropagatePoliciesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Propagating insurance policies from OLTP to DW");
        var count = 0;

        try
        {
            var oltpPolicies = await _oltpContext.InsurancePolicies.AsNoTracking().ToListAsync(cancellationToken);

            foreach (var policy in oltpPolicies)
            {
                // Convert dates to date keys (format: YYYYMMDD as integer)
                var startDateKey = policy.StartDate.Year * 10000 + policy.StartDate.Month * 100 + policy.StartDate.Day;
                var endDateKey = policy.EndDate.Year * 10000 + policy.EndDate.Month * 100 + policy.EndDate.Day;

                var existsSql = "SELECT COUNT(*) FROM ART_GALLERY_DW.DIM_POLICY WHERE POLICY_KEY = :policyKey";
                var existsCount = await ExecuteScalarCountAsync(existsSql,
                    new[] { new OracleParameter("policyKey", policy.Id) },
                    cancellationToken);

                if (existsCount == 0)
                {
                    var insertSql = @"INSERT INTO ART_GALLERY_DW.DIM_POLICY 
                        (POLICY_KEY, POLICY_ID_OLTP, PROVIDER, START_DATE_KEY, END_DATE_KEY, TOTAL_COVERAGE_AMT) 
                        VALUES (:policyKey, :policyIdOltp, :provider, :startDateKey, :endDateKey, :totalCoverageAmt)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("policyKey", policy.Id),
                        new OracleParameter("policyIdOltp", policy.Id),
                        new OracleParameter("provider", policy.Provider ?? "Unknown"),
                        new OracleParameter("startDateKey", startDateKey),
                        new OracleParameter("endDateKey", endDateKey),
                        new OracleParameter("totalCoverageAmt", (object?)policy.TotalCoverageAmount ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_POLICY 
                        SET POLICY_ID_OLTP = :policyIdOltp, PROVIDER = :provider, START_DATE_KEY = :startDateKey, 
                            END_DATE_KEY = :endDateKey, TOTAL_COVERAGE_AMT = :totalCoverageAmt 
                        WHERE POLICY_KEY = :policyKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("policyIdOltp", policy.Id),
                        new OracleParameter("provider", policy.Provider ?? "Unknown"),
                        new OracleParameter("startDateKey", startDateKey),
                        new OracleParameter("endDateKey", endDateKey),
                        new OracleParameter("totalCoverageAmt", (object?)policy.TotalCoverageAmount ?? DBNull.Value),
                        new OracleParameter("policyKey", policy.Id));
                }
            }

            _logger.LogDebug("Propagated {Count} insurance policies", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error propagating insurance policies");
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
                        (EXHIBITION_KEY, EXHIBITION_ID_OLTP, TITLE, START_DATE_KEY, END_DATE_KEY, EXHIBITOR_KEY, DESCRIPTION) 
                        VALUES (:exhibitionKey, :exhibitionIdOltp, :title, :startDateKey, :endDateKey, :exhibitorKey, :description)";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                        new OracleParameter("exhibitionKey", exhibition.Id),
                        new OracleParameter("exhibitionIdOltp", exhibition.Id),
                        new OracleParameter("title", exhibition.Title ?? "Unknown"),
                        new OracleParameter("startDateKey", startDateKey),
                        new OracleParameter("endDateKey", endDateKey),
                        new OracleParameter("exhibitorKey", exhibition.ExhibitorId),
                        new OracleParameter("description", (object?)exhibition.Description ?? DBNull.Value));
                    count++;
                }
                else
                {
                    var updateSql = @"UPDATE ART_GALLERY_DW.DIM_EXHIBITION 
                        SET EXHIBITION_ID_OLTP = :exhibitionIdOltp, TITLE = :title, START_DATE_KEY = :startDateKey, 
                            END_DATE_KEY = :endDateKey, EXHIBITOR_KEY = :exhibitorKey, DESCRIPTION = :description 
                        WHERE EXHIBITION_KEY = :exhibitionKey";
                    
                    await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                        new OracleParameter("exhibitionIdOltp", exhibition.Id),
                        new OracleParameter("title", exhibition.Title ?? "Unknown"),
                        new OracleParameter("startDateKey", startDateKey),
                        new OracleParameter("endDateKey", endDateKey),
                        new OracleParameter("exhibitorKey", exhibition.ExhibitorId),
                        new OracleParameter("description", (object?)exhibition.Description ?? DBNull.Value),
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
                .Include(a => a.Insurances)
                .Include(a => a.Loans)
                .Include(a => a.Restorations)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Get current max FACT_KEY to generate new keys
            var maxFactKeySql = "SELECT NVL(MAX(FACT_KEY), 0) FROM ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY";
            var maxFactKey = await ExecuteScalarCountAsync(maxFactKeySql, Array.Empty<OracleParameter>(), cancellationToken);

            foreach (var artwork in oltpArtworks)
            {
                foreach (var ea in artwork.ExhibitionArtworks)
                {
                    var exhibition = ea.Exhibition;
                    if (exhibition == null) continue;
                    
                    var exhibitionId = ea.ExhibitionId;

                    // Calculate date key from exhibition start date
                    var dateKey = exhibition.StartDate.Year * 10000 + exhibition.StartDate.Month * 100 + exhibition.StartDate.Day;

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
                        
                        // Get exhibitor_key from the exhibition
                        var exhibitorKey = exhibition.ExhibitorId;
                        
                        // Calculate measures
                        var totalInsured = artwork.Insurances?.Sum(i => i.InsuredAmount) ?? 0;
                        var loanFlag = artwork.Loans?.Any() == true ? 1 : 0;
                        var restorationCount = artwork.Restorations?.Count ?? 0;
                        
                        // Get policy key (first insurance policy if exists)
                        int? policyKey = artwork.Insurances?.FirstOrDefault()?.PolicyId;
                        
                        var insertSql = @"INSERT INTO ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY 
                            (FACT_KEY, DATE_KEY, ARTWORK_KEY, ARTIST_KEY, EXHIBITION_KEY, EXHIBITOR_KEY, 
                             COLLECTION_KEY, LOCATION_KEY, POLICY_KEY,
                             ESTIMATED_VALUE, INSURED_AMOUNT, LOAN_FLAG, RESTORATION_COUNT, REVIEW_COUNT, AVG_RATING) 
                            VALUES (:factKey, :dateKey, :artworkKey, :artistKey, :exhibitionKey, :exhibitorKey, 
                                    :collectionKey, :locationKey, :policyKey,
                                    :estimatedValue, :insuredAmount, :loanFlag, :restorationCount, 0, 0)";
                        
                        await _dwContext.Database.ExecuteSqlRawAsync(insertSql,
                            new OracleParameter("factKey", maxFactKey),
                            new OracleParameter("dateKey", dateKey),
                            new OracleParameter("artworkKey", artwork.Id),
                            new OracleParameter("artistKey", artwork.ArtistId),
                            new OracleParameter("exhibitionKey", exhibitionId),
                            new OracleParameter("exhibitorKey", exhibitorKey),
                            new OracleParameter("collectionKey", (object?)artwork.CollectionId ?? DBNull.Value),
                            new OracleParameter("locationKey", (object?)artwork.LocationId ?? DBNull.Value),
                            new OracleParameter("policyKey", (object?)policyKey ?? DBNull.Value),
                            new OracleParameter("estimatedValue", (object?)artwork.EstimatedValue ?? DBNull.Value),
                            new OracleParameter("insuredAmount", totalInsured),
                            new OracleParameter("loanFlag", loanFlag),
                            new OracleParameter("restorationCount", restorationCount));
                        count++;
                    }
                    else
                    {
                        // Update existing record with new measures
                        var totalInsured = artwork.Insurances?.Sum(i => i.InsuredAmount) ?? 0;
                        var loanFlag = artwork.Loans?.Any() == true ? 1 : 0;
                        var restorationCount = artwork.Restorations?.Count ?? 0;
                        int? policyKey = artwork.Insurances?.FirstOrDefault()?.PolicyId;
                        
                        var updateSql = @"UPDATE ART_GALLERY_DW.FACT_EXHIBITION_ACTIVITY 
                            SET DATE_KEY = :dateKey, EXHIBITOR_KEY = :exhibitorKey,
                                COLLECTION_KEY = :collectionKey, LOCATION_KEY = :locationKey, POLICY_KEY = :policyKey,
                                ESTIMATED_VALUE = :estimatedValue, INSURED_AMOUNT = :insuredAmount, 
                                LOAN_FLAG = :loanFlag, RESTORATION_COUNT = :restorationCount
                            WHERE ARTWORK_KEY = :artworkKey AND EXHIBITION_KEY = :exhibitionKey";
                        
                        await _dwContext.Database.ExecuteSqlRawAsync(updateSql,
                            new OracleParameter("dateKey", dateKey),
                            new OracleParameter("exhibitorKey", exhibition.ExhibitorId),
                            new OracleParameter("collectionKey", (object?)artwork.CollectionId ?? DBNull.Value),
                            new OracleParameter("locationKey", (object?)artwork.LocationId ?? DBNull.Value),
                            new OracleParameter("policyKey", (object?)policyKey ?? DBNull.Value),
                            new OracleParameter("estimatedValue", (object?)artwork.EstimatedValue ?? DBNull.Value),
                            new OracleParameter("insuredAmount", totalInsured),
                            new OracleParameter("loanFlag", loanFlag),
                            new OracleParameter("restorationCount", restorationCount),
                            new OracleParameter("artworkKey", artwork.Id),
                            new OracleParameter("exhibitionKey", exhibitionId));
                    }
                }
            }

            _logger.LogDebug("Created/Updated {Count} fact records", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating fact table");
            throw;
        }

        return count;
    }
}
