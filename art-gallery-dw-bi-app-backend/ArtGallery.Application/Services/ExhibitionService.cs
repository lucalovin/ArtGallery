using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Exhibition;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

/// <summary>
/// Service implementation for exhibition operations.
/// Handles local AM/EU exhibitions and GLOBAL reconstruction via database links.
/// Create uses raw Oracle SQL to avoid EF Core RETURNING clause issues.
/// </summary>
public class ExhibitionService : IExhibitionService
{
    private readonly IRepository<Exhibition> _repository;
    private readonly IRepository<ExhibitionArtwork> _exhibitionArtworkRepository;
    private readonly IRepository<Artwork> _artworkRepository;
    private readonly IMapper _mapper;
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public ExhibitionService(
        IRepository<Exhibition> repository,
        IRepository<ExhibitionArtwork> exhibitionArtworkRepository,
        IRepository<Artwork> artworkRepository,
        IMapper mapper,
        IDataSourceContext ds,
        IConfiguration configuration)
    {
        _repository = repository;
        _exhibitionArtworkRepository = exhibitionArtworkRepository;
        _artworkRepository = artworkRepository;
        _mapper = mapper;
        _ds = ds;
        _configuration = configuration;
    }

    private string CurrentSource()
    {
        return _ds.Source.ToString().ToUpperInvariant();
    }

    private bool IsGlobalSource()
    {
        return CurrentSource().Equals("GLOBAL", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<List<Exhibition>> GetGlobalExhibitionsRawAsync()
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new List<Exhibition>();
        }

        var exhibitions = new List<Exhibition>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT
                x.EXHIBITION_ID,
                x.TITLE,
                x.START_DATE,
                x.END_DATE,
                x.EXHIBITOR_ID,
                x.DESCRIPTION,
                x.SOURCE_REGION,
                ex.NAME AS EXHIBITOR_NAME,
                ex.CITY AS EXHIBITOR_CITY,
                NVL(cnt.ARTWORK_COUNT, 0) AS ARTWORK_COUNT
            FROM (
                SELECT
                    e.EXHIBITION_ID,
                    e.TITLE,
                    e.START_DATE,
                    e.END_DATE,
                    e.EXHIBITOR_ID,
                    e.DESCRIPTION,
                    'AM' AS SOURCE_REGION
                FROM EXHIBITION_AM@link_am e

                UNION ALL

                SELECT
                    e.EXHIBITION_ID,
                    e.TITLE,
                    e.START_DATE,
                    e.END_DATE,
                    e.EXHIBITOR_ID,
                    e.DESCRIPTION,
                    'EU' AS SOURCE_REGION
                FROM EXHIBITION_EU@link_eu e
            ) x
            LEFT JOIN (
                SELECT
                    EXHIBITOR_ID,
                    NAME,
                    CITY,
                    'AM' AS SOURCE_REGION
                FROM EXHIBITOR_AM@link_am

                UNION ALL

                SELECT
                    EXHIBITOR_ID,
                    NAME,
                    CITY,
                    'EU' AS SOURCE_REGION
                FROM EXHIBITOR_EU@link_eu
            ) ex
              ON ex.EXHIBITOR_ID = x.EXHIBITOR_ID
             AND ex.SOURCE_REGION = x.SOURCE_REGION
            LEFT JOIN (
                SELECT EXHIBITION_ID, 'AM' AS SOURCE_REGION, COUNT(*) AS ARTWORK_COUNT
                FROM ARTWORK_EXHIBITION_AM@link_am
                GROUP BY EXHIBITION_ID

                UNION ALL

                SELECT EXHIBITION_ID, 'EU' AS SOURCE_REGION, COUNT(*) AS ARTWORK_COUNT
                FROM ARTWORK_EXHIBITION_EU@link_eu
                GROUP BY EXHIBITION_ID
            ) cnt
              ON cnt.EXHIBITION_ID = x.EXHIBITION_ID
             AND cnt.SOURCE_REGION = x.SOURCE_REGION
            ORDER BY x.START_DATE DESC, x.EXHIBITION_ID";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var exhibitionId = Convert.ToInt32(reader["EXHIBITION_ID"]);

            var exhibitorId = reader["EXHIBITOR_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["EXHIBITOR_ID"]);

            var artworkCount = reader["ARTWORK_COUNT"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["ARTWORK_COUNT"]);

            var exhibition = new Exhibition
            {
                Id = exhibitionId,

                Title = reader["TITLE"] == DBNull.Value
                    ? $"Exhibition #{exhibitionId}"
                    : reader["TITLE"]?.ToString() ?? $"Exhibition #{exhibitionId}",

                StartDate = reader["START_DATE"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(reader["START_DATE"]),

                EndDate = reader["END_DATE"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(reader["END_DATE"]),

                ExhibitorId = exhibitorId,

                Description = reader["DESCRIPTION"] == DBNull.Value
                    ? null
                    : reader["DESCRIPTION"]?.ToString(),

                Exhibitor = exhibitorId == 0
                    ? null
                    : new Exhibitor
                    {
                        Id = exhibitorId,
                        Name = reader["EXHIBITOR_NAME"] == DBNull.Value
                            ? $"Exhibitor #{exhibitorId}"
                            : reader["EXHIBITOR_NAME"]?.ToString() ?? $"Exhibitor #{exhibitorId}",
                        City = reader["EXHIBITOR_CITY"] == DBNull.Value
                            ? null
                            : reader["EXHIBITOR_CITY"]?.ToString()
                    },

                ExhibitionArtworks = Enumerable.Range(1, artworkCount)
                    .Select(_ => new ExhibitionArtwork())
                    .ToList()
            };

            exhibitions.Add(exhibition);
        }

        return exhibitions;
    }

    private async Task<PaginatedResponse<ExhibitionResponseDto>> GetAllGlobalAsync(PagedRequest request)
    {
        var exhibitions = await GetGlobalExhibitionsRawAsync();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLowerInvariant();

            exhibitions = exhibitions
                .Where(e =>
                    !string.IsNullOrWhiteSpace(e.Title) &&
                    e.Title.ToLowerInvariant().Contains(searchTerm))
                .ToList();
        }

        exhibitions = request.SortBy?.ToLowerInvariant() switch
        {
            "title" => request.IsDescending
                ? exhibitions.OrderByDescending(e => e.Title).ToList()
                : exhibitions.OrderBy(e => e.Title).ToList(),

            "startdate" => request.IsDescending
                ? exhibitions.OrderByDescending(e => e.StartDate).ToList()
                : exhibitions.OrderBy(e => e.StartDate).ToList(),

            "enddate" => request.IsDescending
                ? exhibitions.OrderByDescending(e => e.EndDate).ToList()
                : exhibitions.OrderBy(e => e.EndDate).ToList(),

            _ => exhibitions.OrderByDescending(e => e.StartDate).ToList()
        };

        var totalCount = exhibitions.Count;

        var pagedItems = exhibitions
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ExhibitionResponseDto>>(pagedItems);

        return PaginatedResponse<ExhibitionResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<PaginatedResponse<ExhibitionResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (IsGlobalSource())
        {
            return await GetAllGlobalAsync(request);
        }

        if (!DataSourceCapabilities.HasExhibition(_ds.Source))
        {
            return PaginatedResponse<ExhibitionResponseDto>.Create(
                new List<ExhibitionResponseDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        IQueryable<Exhibition> query = _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Include(e => e.Exhibitor);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();

            query = query.Where(e =>
                e.Title.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "title" => request.IsDescending
                ? query.OrderByDescending(e => e.Title)
                : query.OrderBy(e => e.Title),

            "startdate" => request.IsDescending
                ? query.OrderByDescending(e => e.StartDate)
                : query.OrderBy(e => e.StartDate),

            "enddate" => request.IsDescending
                ? query.OrderByDescending(e => e.EndDate)
                : query.OrderBy(e => e.EndDate),

            _ => query.OrderByDescending(e => e.StartDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ExhibitionResponseDto>>(items);

        return PaginatedResponse<ExhibitionResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    public async Task<ExhibitionDetailDto?> GetByIdAsync(int id)
    {
        if (IsGlobalSource())
        {
            var exhibitions = await GetGlobalExhibitionsRawAsync();
            var exhibition = exhibitions.FirstOrDefault(e => e.Id == id);

            return exhibition == null
                ? null
                : _mapper.Map<ExhibitionDetailDto>(exhibition);
        }

        if (!DataSourceCapabilities.HasExhibition(_ds.Source))
            return null;

        var localExhibition = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Include(e => e.Exhibitor)
            .FirstOrDefaultAsync(e => e.Id == id);

        return localExhibition == null
            ? null
            : _mapper.Map<ExhibitionDetailDto>(localExhibition);
    }

    public async Task<ExhibitionResponseDto> CreateAsync(CreateExhibitionDto dto)
    {
        return await CreateExhibitionWithRawSqlAsync(dto);
    }

    private async Task<ExhibitionResponseDto> CreateExhibitionWithRawSqlAsync(CreateExhibitionDto dto)
    {
        var source = CurrentSource();

        string? connectionString;
        string tableName;

        if (source == "AM")
        {
            connectionString = _configuration.GetConnectionString("BddAmConnection");
            tableName = "EXHIBITION_AM";
        }
        else if (source == "EU")
        {
            connectionString = _configuration.GetConnectionString("BddEuConnection");
            tableName = "EXHIBITION_EU";
        }
        else if (source == "GLOBAL")
        {
            connectionString = _configuration.GetConnectionString("BddGlobalConnection");
            tableName = "GLOBAL_EXHIBITION";
        }
        else
        {
            var exhibition = _mapper.Map<Exhibition>(dto);

            await _repository.AddAsync(exhibition);
            await _repository.SaveChangesAsync();

            return _mapper.Map<ExhibitionResponseDto>(exhibition);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string for source {source} is not configured.");
        }

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var insertCommand = connection.CreateCommand();

        insertCommand.CommandText = $@"
            INSERT INTO {tableName} (
                TITLE,
                START_DATE,
                END_DATE,
                EXHIBITOR_ID,
                DESCRIPTION
            )
            VALUES (
                :title,
                :startDate,
                :endDate,
                :exhibitorId,
                :description
            )";

        insertCommand.Parameters.Add(new OracleParameter("title", dto.Title));
        insertCommand.Parameters.Add(new OracleParameter("startDate", dto.StartDate));
        insertCommand.Parameters.Add(new OracleParameter("endDate", dto.EndDate));
        insertCommand.Parameters.Add(new OracleParameter("exhibitorId", dto.ExhibitorId));
        insertCommand.Parameters.Add(new OracleParameter("description", (object?)dto.Description ?? DBNull.Value));

        await insertCommand.ExecuteNonQueryAsync();

        await using var selectCommand = connection.CreateCommand();

        selectCommand.CommandText = $@"
            SELECT MAX(EXHIBITION_ID)
            FROM {tableName}
            WHERE TITLE = :title
              AND START_DATE = :startDate
              AND END_DATE = :endDate
              AND EXHIBITOR_ID = :exhibitorId";

        selectCommand.Parameters.Add(new OracleParameter("title", dto.Title));
        selectCommand.Parameters.Add(new OracleParameter("startDate", dto.StartDate));
        selectCommand.Parameters.Add(new OracleParameter("endDate", dto.EndDate));
        selectCommand.Parameters.Add(new OracleParameter("exhibitorId", dto.ExhibitorId));

        var idResult = await selectCommand.ExecuteScalarAsync();

        var createdId = idResult == null || idResult == DBNull.Value
            ? 0
            : Convert.ToInt32(idResult);

        var created = new Exhibition
        {
            Id = createdId,
            Title = dto.Title,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ExhibitorId = dto.ExhibitorId,
            Description = dto.Description
        };

        return _mapper.Map<ExhibitionResponseDto>(created);
    }

    public async Task<ExhibitionResponseDto> UpdateAsync(int id, UpdateExhibitionDto dto)
    {
        var exhibition = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Exhibition), id);

        if (dto.Title != null) exhibition.Title = dto.Title;
        if (dto.Description != null) exhibition.Description = dto.Description;
        if (dto.StartDate.HasValue) exhibition.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) exhibition.EndDate = dto.EndDate.Value;
        if (dto.ExhibitorId.HasValue) exhibition.ExhibitorId = dto.ExhibitorId.Value;

        _repository.Update(exhibition);
        await _repository.SaveChangesAsync();

        return _mapper.Map<ExhibitionResponseDto>(exhibition);
    }

    public async Task DeleteAsync(int id)
    {
        var exhibition = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Exhibition), id);

        _repository.Delete(exhibition);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExhibitionResponseDto>> GetUpcomingAsync(int limit = 5)
    {
        if (IsGlobalSource())
        {
            var exhibitions = await GetGlobalExhibitionsRawAsync();

            var upcoming = exhibitions
                .Where(e => e.StartDate > DateTime.UtcNow)
                .OrderBy(e => e.StartDate)
                .Take(limit)
                .ToList();

            return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(upcoming);
        }

        if (!DataSourceCapabilities.HasExhibition(_ds.Source))
            return Enumerable.Empty<ExhibitionResponseDto>();

        var localExhibitions = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Include(e => e.Exhibitor)
            .Where(e => e.StartDate > DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Take(limit)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(localExhibitions);
    }

    public async Task<IEnumerable<ExhibitionResponseDto>> GetActiveAsync()
    {
        var today = DateTime.UtcNow.Date;

        if (IsGlobalSource())
        {
            var exhibitions = await GetGlobalExhibitionsRawAsync();

            var active = exhibitions
                .Where(e => e.StartDate <= today && e.EndDate >= today)
                .OrderBy(e => e.StartDate)
                .ToList();

            return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(active);
        }

        if (!DataSourceCapabilities.HasExhibition(_ds.Source))
            return Enumerable.Empty<ExhibitionResponseDto>();

        var localExhibitions = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Include(e => e.Exhibitor)
            .Where(e => e.StartDate <= today && e.EndDate >= today)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(localExhibitions);
    }

    public async Task<IEnumerable<ExhibitionResponseDto>> GetPastAsync()
    {
        if (IsGlobalSource())
        {
            var exhibitions = await GetGlobalExhibitionsRawAsync();

            var past = exhibitions
                .Where(e => e.EndDate < DateTime.UtcNow)
                .OrderByDescending(e => e.EndDate)
                .ToList();

            return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(past);
        }

        if (!DataSourceCapabilities.HasExhibition(_ds.Source))
            return Enumerable.Empty<ExhibitionResponseDto>();

        var localExhibitions = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Include(e => e.Exhibitor)
            .Where(e => e.EndDate < DateTime.UtcNow)
            .OrderByDescending(e => e.EndDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(localExhibitions);
    }

    public async Task AddArtworkAsync(int exhibitionId, int artworkId, int? displayOrder = null)
    {
        var exhibition = await _repository.GetByIdAsync(exhibitionId)
            ?? throw new NotFoundException(nameof(Exhibition), exhibitionId);

        var artwork = await _artworkRepository.GetByIdAsync(artworkId)
            ?? throw new NotFoundException(nameof(Artwork), artworkId);

        var existsCount = await _exhibitionArtworkRepository.CountAsync(
            ea => ea.ExhibitionId == exhibitionId &&
                  ea.ArtworkId == artworkId);

        if (existsCount > 0)
            throw new ConflictException($"Artwork {artworkId} is already in exhibition {exhibitionId}");

        var exhibitionArtwork = new ExhibitionArtwork
        {
            ExhibitionId = exhibitionId,
            ArtworkId = artworkId,
            PositionInGallery = displayOrder?.ToString()
        };

        await _exhibitionArtworkRepository.AddAsync(exhibitionArtwork);
        await _exhibitionArtworkRepository.SaveChangesAsync();
    }

    public async Task RemoveArtworkAsync(int exhibitionId, int artworkId)
    {
        var exhibitionArtwork = await _exhibitionArtworkRepository.Query()
            .FirstOrDefaultAsync(ea =>
                ea.ExhibitionId == exhibitionId &&
                ea.ArtworkId == artworkId)
            ?? throw new NotFoundException("ExhibitionArtwork", $"{exhibitionId}-{artworkId}");

        _exhibitionArtworkRepository.Delete(exhibitionArtwork);
        await _exhibitionArtworkRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExhibitionArtworkDto>> GetArtworksAsync(int exhibitionId)
    {
        if (IsGlobalSource())
        {
            return await GetGlobalExhibitionArtworksAsync(exhibitionId);
        }

        IQueryable<ExhibitionArtwork> query = _exhibitionArtworkRepository.Query();

        if (DataSourceCapabilities.ArtworkHasCore(_ds.Source))
        {
            query = query.Include(ea => ea.Artwork);
        }

        var exhibitionArtworks = await query
            .Where(ea => ea.ExhibitionId == exhibitionId)
            .OrderBy(ea => ea.PositionInGallery)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionArtworkDto>>(exhibitionArtworks);
    }

    private async Task<IEnumerable<ExhibitionArtworkDto>> GetGlobalExhibitionArtworksAsync(int exhibitionId)
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Enumerable.Empty<ExhibitionArtworkDto>();
        }

        var items = new List<ExhibitionArtwork>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT
                ae.ARTWORK_ID,
                ae.EXHIBITION_ID,
                ae.POSITION_IN_GALLERY,
                ae.FEATURED_STATUS,
                c.TITLE,
                c.ARTIST_ID,
                c.YEAR_CREATED,
                c.MEDIUM,
                c.COLLECTION_ID,
                d.LOCATION_ID,
                d.ESTIMATED_VALUE,
                a.NAME AS ARTIST_NAME
            FROM (
                SELECT
                    ARTWORK_ID,
                    EXHIBITION_ID,
                    POSITION_IN_GALLERY,
                    FEATURED_STATUS
                FROM ARTWORK_EXHIBITION_AM@link_am
                WHERE EXHIBITION_ID = :exhibitionId

                UNION ALL

                SELECT
                    ARTWORK_ID,
                    EXHIBITION_ID,
                    POSITION_IN_GALLERY,
                    FEATURED_STATUS
                FROM ARTWORK_EXHIBITION_EU@link_eu
                WHERE EXHIBITION_ID = :exhibitionId
            ) ae
            JOIN ARTWORK_CORE@link_eu c
              ON c.ARTWORK_ID = ae.ARTWORK_ID
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = ae.ARTWORK_ID
            LEFT JOIN ARTIST_EU@link_eu a
              ON a.ARTIST_ID = c.ARTIST_ID
            ORDER BY ae.POSITION_IN_GALLERY, ae.ARTWORK_ID";

        command.Parameters.Add(new OracleParameter("exhibitionId", exhibitionId));

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

            var exhibitionArtwork = new ExhibitionArtwork
            {
                ExhibitionId = Convert.ToInt32(reader["EXHIBITION_ID"]),
                ArtworkId = artworkId,
                PositionInGallery = reader["POSITION_IN_GALLERY"] == DBNull.Value
                    ? null
                    : reader["POSITION_IN_GALLERY"]?.ToString(),
                Artwork = new Artwork
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
                        }
                }
            };

            items.Add(exhibitionArtwork);
        }

        return _mapper.Map<IEnumerable<ExhibitionArtworkDto>>(items);
    }
}