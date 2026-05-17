using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Artwork;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

/// <summary>
/// Service implementation for artwork operations.
/// Handles normal OLTP persistence and BDD vertical fragmentation for ARTWORK.
/// </summary>
public class ArtworkService : IArtworkService
{
    private readonly IRepository<Artwork> _repository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public ArtworkService(
        IRepository<Artwork> repository,
        IMapper mapper,
        IDataSourceContext ds,
        IConfiguration configuration)
    {
        _repository = repository;
        _mapper = mapper;
        _ds = ds;
        _configuration = configuration;
    }

    private bool IsGlobalSource()
    {
        return _ds.Source.ToString().Equals("GLOBAL", StringComparison.OrdinalIgnoreCase);
    }

    private IQueryable<Artwork> WithIncludes(IQueryable<Artwork> q)
    {
        var s = _ds.Source;

        if (DataSourceCapabilities.HasArtist(s) && DataSourceCapabilities.ArtworkHasCore(s))
            q = q.Include(a => a.Artist);

        if (DataSourceCapabilities.HasCollection(s) && DataSourceCapabilities.ArtworkHasCore(s))
            q = q.Include(a => a.Collection);

        if (DataSourceCapabilities.HasLocation(s) && DataSourceCapabilities.ArtworkHasDetails(s))
            q = q.Include(a => a.Location);

        return q;
    }

    public async Task<PaginatedResponse<ArtworkListDto>> GetAllAsync(PagedRequest request)
    {
        if (IsGlobalSource())
        {
            return await GetAllGlobalAsync(request);
        }

        var hasCore = DataSourceCapabilities.ArtworkHasCore(_ds.Source);
        IQueryable<Artwork> query = WithIncludes(_repository.Query());

        if (!string.IsNullOrWhiteSpace(request.Search) && hasCore)
        {
            var searchTerm = request.Search.ToLower();

            query = query.Where(a =>
                a.Title.ToLower().Contains(searchTerm) ||
                (a.Artist != null && a.Artist.Name.ToLower().Contains(searchTerm)));
        }

        var sortKey = request.SortBy?.ToLower();

        if (!hasCore && sortKey is "title" or "artist" or "year")
            sortKey = null;

        query = sortKey switch
        {
            "title" => request.IsDescending
                ? query.OrderByDescending(a => a.Title)
                : query.OrderBy(a => a.Title),

            "artist" => request.IsDescending
                ? query.OrderByDescending(a => a.Artist!.Name)
                : query.OrderBy(a => a.Artist!.Name),

            "year" => request.IsDescending
                ? query.OrderByDescending(a => a.YearCreated)
                : query.OrderBy(a => a.YearCreated),

            "estimatedvalue" => request.IsDescending
                ? query.OrderByDescending(a => a.EstimatedValue)
                : query.OrderBy(a => a.EstimatedValue),

            _ => query.OrderBy(a => a.Id)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ArtworkListDto>>(items);

        return PaginatedResponse<ArtworkListDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<ArtworkListDto>> GetAllGlobalAsync(PagedRequest request)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return PaginatedResponse<ArtworkListDto>.Create(
                new List<ArtworkListDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        var artworks = new List<Artwork>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                c.ARTWORK_ID,
                c.TITLE,
                c.ARTIST_ID,
                c.YEAR_CREATED,
                c.MEDIUM,
                c.COLLECTION_ID,
                d.LOCATION_ID,
                d.ESTIMATED_VALUE,
                a.NAME AS ARTIST_NAME,
                col.NAME AS COLLECTION_NAME,
                loc.NAME AS LOCATION_NAME
            FROM ARTWORK_CORE@link_eu c
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = c.ARTWORK_ID
            LEFT JOIN ARTIST_EU@link_eu a
              ON a.ARTIST_ID = c.ARTIST_ID
            LEFT JOIN COLLECTION_EU@link_eu col
              ON col.COLLECTION_ID = c.COLLECTION_ID
            LEFT JOIN LOCATION loc
              ON loc.LOCATION_ID = d.LOCATION_ID
            ORDER BY c.ARTWORK_ID";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var artworkId = Convert.ToInt32(reader["ARTWORK_ID"]);

            var artistId = reader["ARTIST_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTIST_ID"]);

            int? collectionId = reader["COLLECTION_ID"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["COLLECTION_ID"]);

            int? locationId = reader["LOCATION_ID"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["LOCATION_ID"]);

            var artwork = new Artwork
            {
                Id = artworkId,
                Title = reader["TITLE"] == DBNull.Value
                    ? $"Artwork #{artworkId}"
                    : reader["TITLE"]?.ToString() ?? $"Artwork #{artworkId}",

                ArtistId = artistId,

                YearCreated = reader["YEAR_CREATED"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["YEAR_CREATED"]),

                Medium = reader["MEDIUM"] == DBNull.Value
                    ? null
                    : reader["MEDIUM"]?.ToString(),

                CollectionId = collectionId,
                LocationId = locationId,

                EstimatedValue = reader["ESTIMATED_VALUE"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["ESTIMATED_VALUE"]),

                Artist = artistId == 0
                    ? null
                    : new Artist
                    {
                        Id = artistId,
                        Name = reader["ARTIST_NAME"] == DBNull.Value
                            ? $"Artist #{artistId}"
                            : reader["ARTIST_NAME"]?.ToString() ?? $"Artist #{artistId}"
                    },

                Collection = collectionId == null
                    ? null
                    : new Collection
                    {
                        Id = collectionId.Value,
                        Name = reader["COLLECTION_NAME"] == DBNull.Value
                            ? $"Collection #{collectionId.Value}"
                            : reader["COLLECTION_NAME"]?.ToString() ?? $"Collection #{collectionId.Value}"
                    },

                Location = locationId == null
                    ? null
                    : new Location
                    {
                        Id = locationId.Value,
                        Name = reader["LOCATION_NAME"] == DBNull.Value
                            ? $"Location #{locationId.Value}"
                            : reader["LOCATION_NAME"]?.ToString() ?? $"Location #{locationId.Value}"
                    }
            };

            artworks.Add(artwork);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLowerInvariant();

            artworks = artworks
                .Where(a =>
                    (!string.IsNullOrWhiteSpace(a.Title) &&
                     a.Title.ToLowerInvariant().Contains(searchTerm)) ||
                    (a.Artist != null &&
                     !string.IsNullOrWhiteSpace(a.Artist.Name) &&
                     a.Artist.Name.ToLowerInvariant().Contains(searchTerm)))
                .ToList();
        }

        artworks = request.SortBy?.ToLowerInvariant() switch
        {
            "title" => request.IsDescending
                ? artworks.OrderByDescending(a => a.Title).ToList()
                : artworks.OrderBy(a => a.Title).ToList(),

            "artist" => request.IsDescending
                ? artworks.OrderByDescending(a => a.Artist?.Name).ToList()
                : artworks.OrderBy(a => a.Artist?.Name).ToList(),

            "year" => request.IsDescending
                ? artworks.OrderByDescending(a => a.YearCreated).ToList()
                : artworks.OrderBy(a => a.YearCreated).ToList(),

            "estimatedvalue" => request.IsDescending
                ? artworks.OrderByDescending(a => a.EstimatedValue).ToList()
                : artworks.OrderBy(a => a.EstimatedValue).ToList(),

            _ => artworks.OrderBy(a => a.Id).ToList()
        };

        var totalCount = artworks.Count;

        var pagedItems = artworks
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ArtworkListDto>>(pagedItems);

        return PaginatedResponse<ArtworkListDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<ArtworkResponseDto?> GetByIdAsync(int id)
    {
        if (IsGlobalSource())
        {
            return await GetByIdGlobalAsync(id);
        }

        var artwork = await WithIncludes(_repository.Query())
            .FirstOrDefaultAsync(a => a.Id == id);

        return artwork == null ? null : _mapper.Map<ArtworkResponseDto>(artwork);
    }

    private async Task<ArtworkResponseDto?> GetByIdGlobalAsync(int id)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                c.ARTWORK_ID,
                c.TITLE,
                c.ARTIST_ID,
                c.YEAR_CREATED,
                c.MEDIUM,
                c.COLLECTION_ID,
                d.LOCATION_ID,
                d.ESTIMATED_VALUE,
                a.NAME AS ARTIST_NAME,
                col.NAME AS COLLECTION_NAME,
                loc.NAME AS LOCATION_NAME
            FROM ARTWORK_CORE@link_eu c
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = c.ARTWORK_ID
            LEFT JOIN ARTIST_EU@link_eu a
              ON a.ARTIST_ID = c.ARTIST_ID
            LEFT JOIN COLLECTION_EU@link_eu col
              ON col.COLLECTION_ID = c.COLLECTION_ID
            LEFT JOIN LOCATION loc
              ON loc.LOCATION_ID = d.LOCATION_ID
            WHERE c.ARTWORK_ID = :artworkId";

        command.Parameters.Add(new OracleParameter("artworkId", id));

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var artistId = reader["ARTIST_ID"] == DBNull.Value
            ? 0
            : Convert.ToInt32(reader["ARTIST_ID"]);

        int? collectionId = reader["COLLECTION_ID"] == DBNull.Value
            ? null
            : Convert.ToInt32(reader["COLLECTION_ID"]);

        int? locationId = reader["LOCATION_ID"] == DBNull.Value
            ? null
            : Convert.ToInt32(reader["LOCATION_ID"]);

        var artwork = new Artwork
        {
            Id = Convert.ToInt32(reader["ARTWORK_ID"]),

            Title = reader["TITLE"] == DBNull.Value
                ? $"Artwork #{id}"
                : reader["TITLE"]?.ToString() ?? $"Artwork #{id}",

            ArtistId = artistId,

            YearCreated = reader["YEAR_CREATED"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["YEAR_CREATED"]),

            Medium = reader["MEDIUM"] == DBNull.Value
                ? null
                : reader["MEDIUM"]?.ToString(),

            CollectionId = collectionId,
            LocationId = locationId,

            EstimatedValue = reader["ESTIMATED_VALUE"] == DBNull.Value
                ? null
                : Convert.ToDecimal(reader["ESTIMATED_VALUE"]),

            Artist = artistId == 0
                ? null
                : new Artist
                {
                    Id = artistId,
                    Name = reader["ARTIST_NAME"] == DBNull.Value
                        ? $"Artist #{artistId}"
                        : reader["ARTIST_NAME"]?.ToString() ?? $"Artist #{artistId}"
                },

            Collection = collectionId == null
                ? null
                : new Collection
                {
                    Id = collectionId.Value,
                    Name = reader["COLLECTION_NAME"] == DBNull.Value
                        ? $"Collection #{collectionId.Value}"
                        : reader["COLLECTION_NAME"]?.ToString() ?? $"Collection #{collectionId.Value}"
                },

            Location = locationId == null
                ? null
                : new Location
                {
                    Id = locationId.Value,
                    Name = reader["LOCATION_NAME"] == DBNull.Value
                        ? $"Location #{locationId.Value}"
                        : reader["LOCATION_NAME"]?.ToString() ?? $"Location #{locationId.Value}"
                }
        };

        return _mapper.Map<ArtworkResponseDto>(artwork);
    }

    public async Task<ArtworkResponseDto> CreateAsync(CreateArtworkDto dto)
    {
        var source = _ds.Source.ToString().ToUpperInvariant();

        if (source is "AM" or "EU" or "GLOBAL")
        {
            return await CreateDistributedArtworkAsync(dto);
        }

        var artwork = _mapper.Map<Artwork>(dto);

        await _repository.AddAsync(artwork);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ArtworkResponseDto>(artwork);
    }

    private async Task<ArtworkResponseDto> CreateDistributedArtworkAsync(CreateArtworkDto dto)
    {
        var amConnectionString = _configuration.GetConnectionString("BddAmConnection");
        var euConnectionString = _configuration.GetConnectionString("BddEuConnection");

        if (string.IsNullOrWhiteSpace(amConnectionString))
            throw new InvalidOperationException("BddAmConnection is not configured.");

        if (string.IsNullOrWhiteSpace(euConnectionString))
            throw new InvalidOperationException("BddEuConnection is not configured.");

        await using var amConnection = new OracleConnection(amConnectionString);
        await using var euConnection = new OracleConnection(euConnectionString);

        await amConnection.OpenAsync();
        await euConnection.OpenAsync();

        var artworkId = await GetNextDistributedArtworkIdAsync(amConnection, euConnection);

        await using var euTransaction = euConnection.BeginTransaction();
        await using var amTransaction = amConnection.BeginTransaction();

        try
        {
            await InsertArtworkCoreAsync(euConnection, euTransaction, artworkId, dto);
            await InsertArtworkDetailsAsync(amConnection, amTransaction, artworkId, dto);

            await euTransaction.CommitAsync();
            await amTransaction.CommitAsync();

            var artwork = new Artwork
            {
                Id = artworkId,
                Title = dto.Title,
                ArtistId = dto.ArtistId,
                YearCreated = dto.YearCreated,
                Medium = dto.Medium,
                CollectionId = dto.CollectionId,
                LocationId = dto.LocationId,
                EstimatedValue = dto.EstimatedValue
            };

            return _mapper.Map<ArtworkResponseDto>(artwork);
        }
        catch
        {
            await SafeRollbackAsync(euTransaction);
            await SafeRollbackAsync(amTransaction);
            throw;
        }
    }

    private static async Task SafeRollbackAsync(OracleTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch
        {
            // Ignore rollback errors so the original exception is preserved.
        }
    }

    private static async Task<int> GetNextDistributedArtworkIdAsync(
        OracleConnection amConnection,
        OracleConnection euConnection)
    {
        var maxEu = await ExecuteScalarIntAsync(
            euConnection,
            "SELECT NVL(MAX(ARTWORK_ID), 0) FROM ARTWORK_CORE");

        var maxAm = await ExecuteScalarIntAsync(
            amConnection,
            "SELECT NVL(MAX(ARTWORK_ID), 0) FROM ARTWORK_DETAILS");

        return Math.Max(maxEu, maxAm) + 1;
    }

    private static async Task<int> ExecuteScalarIntAsync(
        OracleConnection connection,
        string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    private static async Task InsertArtworkCoreAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        int artworkId,
        CreateArtworkDto dto)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = @"
            INSERT INTO ARTWORK_CORE (
                ARTWORK_ID,
                TITLE,
                ARTIST_ID,
                YEAR_CREATED,
                MEDIUM,
                COLLECTION_ID
            )
            VALUES (
                :artworkId,
                :title,
                :artistId,
                :yearCreated,
                :medium,
                :collectionId
            )";

        command.Parameters.Add(new OracleParameter("artworkId", artworkId));
        command.Parameters.Add(new OracleParameter("title", dto.Title));
        command.Parameters.Add(new OracleParameter("artistId", dto.ArtistId));
        command.Parameters.Add(new OracleParameter("yearCreated", dto.YearCreated));
        command.Parameters.Add(new OracleParameter("medium", (object?)dto.Medium ?? DBNull.Value));
        command.Parameters.Add(new OracleParameter("collectionId", (object?)dto.CollectionId ?? DBNull.Value));

        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertArtworkDetailsAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        int artworkId,
        CreateArtworkDto dto)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = @"
            INSERT INTO ARTWORK_DETAILS (
                ARTWORK_ID,
                LOCATION_ID,
                ESTIMATED_VALUE
            )
            VALUES (
                :artworkId,
                :locationId,
                :estimatedValue
            )";

        command.Parameters.Add(new OracleParameter("artworkId", artworkId));
        command.Parameters.Add(new OracleParameter("locationId", (object?)dto.LocationId ?? DBNull.Value));
        command.Parameters.Add(new OracleParameter("estimatedValue", (object?)dto.EstimatedValue ?? DBNull.Value));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<ArtworkResponseDto> UpdateAsync(int id, UpdateArtworkDto dto)
    {
        var artwork = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Artwork), id);

        if (dto.Title != null) artwork.Title = dto.Title;
        if (dto.ArtistId.HasValue) artwork.ArtistId = dto.ArtistId.Value;
        if (dto.YearCreated.HasValue) artwork.YearCreated = dto.YearCreated;
        if (dto.Medium != null) artwork.Medium = dto.Medium;
        if (dto.CollectionId.HasValue) artwork.CollectionId = dto.CollectionId;
        if (dto.LocationId.HasValue) artwork.LocationId = dto.LocationId;
        if (dto.EstimatedValue.HasValue) artwork.EstimatedValue = dto.EstimatedValue;

        _repository.Update(artwork);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ArtworkResponseDto>(artwork);
    }

    public async Task DeleteAsync(int id)
    {
        var artwork = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Artwork), id);

        _repository.Delete(artwork);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ArtworkListDto>> SearchAsync(string query)
    {
        if (IsGlobalSource())
        {
            var result = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue,
                Search = query
            });

            return result.Items;
        }

        if (!DataSourceCapabilities.ArtworkHasCore(_ds.Source))
            return Enumerable.Empty<ArtworkListDto>();

        var searchTerm = query.ToLower();

        var artworks = await WithIncludes(_repository.Query())
            .Where(a =>
                a.Title.ToLower().Contains(searchTerm) ||
                (a.Artist != null && a.Artist.Name.ToLower().Contains(searchTerm)))
            .ToListAsync();

        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<ArtworkStatisticsDto> GetStatisticsAsync()
    {
        if (IsGlobalSource())
        {
            var result = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            var artworks = result.Items.ToList();

            return new ArtworkStatisticsDto
            {
                TotalArtworks = artworks.Count,
                TotalEstimatedValue = artworks.Sum(a => a.EstimatedValue ?? 0),
                ByCollection = new Dictionary<string, int>(),
                ByArtist = artworks
                    .Where(a => !string.IsNullOrWhiteSpace(a.ArtistName))
                    .GroupBy(a => a.ArtistName!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByLocation = new Dictionary<string, int>()
            };
        }

        var localArtworks = await WithIncludes(_repository.Query()).ToListAsync();

        return new ArtworkStatisticsDto
        {
            TotalArtworks = localArtworks.Count,
            TotalEstimatedValue = localArtworks.Sum(a => a.EstimatedValue ?? 0),
            ByCollection = localArtworks
                .Where(a => a.Collection != null)
                .GroupBy(a => a.Collection!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByArtist = localArtworks
                .Where(a => a.Artist != null)
                .GroupBy(a => a.Artist!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByLocation = localArtworks
                .Where(a => a.Location != null)
                .GroupBy(a => a.Location!.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByArtistAsync(string artist)
    {
        if (IsGlobalSource())
        {
            var result = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue,
                Search = artist
            });

            return result.Items;
        }

        if (!DataSourceCapabilities.ArtworkHasCore(_ds.Source) ||
            !DataSourceCapabilities.HasArtist(_ds.Source))
        {
            return Enumerable.Empty<ArtworkListDto>();
        }

        var artworks = await WithIncludes(_repository.Query())
            .Where(a =>
                a.Artist != null &&
                a.Artist.Name.ToLower().Contains(artist.ToLower()))
            .ToListAsync();

        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByCollectionAsync(string collection)
    {
        if (!DataSourceCapabilities.ArtworkHasCore(_ds.Source) ||
            !DataSourceCapabilities.HasCollection(_ds.Source))
        {
            return Enumerable.Empty<ArtworkListDto>();
        }

        var artworks = await WithIncludes(_repository.Query())
            .Where(a =>
                a.Collection != null &&
                a.Collection.Name.ToLower() == collection.ToLower())
            .ToListAsync();

        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByStatusAsync(string status)
    {
        if (IsGlobalSource())
        {
            var result = await GetAllGlobalAsync(new PagedRequest
            {
                Page = 1,
                PageSize = int.MaxValue
            });

            return result.Items;
        }

        var artworks = await WithIncludes(_repository.Query()).ToListAsync();
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }
}