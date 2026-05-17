using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Helper extensions used by LookupsController so endpoints can
/// gracefully return an empty list when the requested entity is not mapped
/// to a table on the active data source.
/// </summary>
internal static class LookupsContextExtensions
{
    public static bool IsMapped<TEntity>(this AppDbContext ctx) where TEntity : class
        => ctx.Model.FindEntityType(typeof(TEntity))?.GetTableName() is { Length: > 0 };
}

/// <summary>
/// Controller for lookup data used by dropdowns.
/// Handles local AM/EU lookups plus GLOBAL lookups through DB links.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LookupsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LookupsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDataSourceContext _ds;

    public LookupsController(
        AppDbContext context,
        ILogger<LookupsController> logger,
        IConfiguration configuration,
        IDataSourceContext ds)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _ds = ds;
    }

    private bool IsGlobalSource()
    {
        return _ds.Source.ToString().Equals("GLOBAL", StringComparison.OrdinalIgnoreCase);
    }

    private string? GetGlobalConnectionString()
    {
        return _configuration.GetConnectionString("BddGlobalConnection");
    }

    private async Task<List<LookupDto>> GetGlobalLocationsAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("BddGlobalConnection is not configured.");
            return new List<LookupDto>();
        }

        var locations = new List<LookupDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT LOCATION_ID, NAME, GALLERY_ROOM, TYPE
            FROM LOCATION
            ORDER BY NAME";

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt32(reader["LOCATION_ID"]);
            var name = reader["NAME"] == DBNull.Value
                ? $"Location #{id}"
                : reader["NAME"]?.ToString() ?? $"Location #{id}";

            var galleryRoom = reader["GALLERY_ROOM"] == DBNull.Value
                ? null
                : reader["GALLERY_ROOM"]?.ToString();

            var type = reader["TYPE"] == DBNull.Value
                ? null
                : reader["TYPE"]?.ToString();

            var descriptionParts = new[] { galleryRoom, type }
                .Where(x => !string.IsNullOrWhiteSpace(x));

            locations.Add(new LookupDto
            {
                Id = id,
                Name = name,
                Description = string.Join(" - ", descriptionParts)
            });
        }

        return locations;
    }

    private async Task<List<LookupDto>> GetGlobalExhibitorsAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("BddGlobalConnection is not configured.");
            return new List<LookupDto>();
        }

        var exhibitors = new List<LookupDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
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

            ORDER BY NAME";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt32(reader["EXHIBITOR_ID"]);

            var name = reader["NAME"] == DBNull.Value
                ? $"Exhibitor #{id}"
                : reader["NAME"]?.ToString() ?? $"Exhibitor #{id}";

            var city = reader["CITY"] == DBNull.Value
                ? null
                : reader["CITY"]?.ToString();

            var region = reader["SOURCE_REGION"] == DBNull.Value
                ? null
                : reader["SOURCE_REGION"]?.ToString();

            exhibitors.Add(new LookupDto
            {
                Id = id,
                Name = name,
                Description = string.IsNullOrWhiteSpace(city)
                    ? region
                    : $"{city} ({region})"
            });
        }

        return exhibitors;
    }

    private async Task<List<LookupDto>> GetGlobalArtistsAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return new List<LookupDto>();

        var artists = new List<LookupDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ARTIST_ID, NAME, NATIONALITY
            FROM ARTIST_EU@link_eu
            ORDER BY NAME";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt32(reader["ARTIST_ID"]);

            artists.Add(new LookupDto
            {
                Id = id,
                Name = reader["NAME"] == DBNull.Value
                    ? $"Artist #{id}"
                    : reader["NAME"]?.ToString() ?? $"Artist #{id}",
                Description = reader["NATIONALITY"] == DBNull.Value
                    ? null
                    : reader["NATIONALITY"]?.ToString()
            });
        }

        return artists;
    }

    private async Task<List<LookupDto>> GetGlobalCollectionsAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return new List<LookupDto>();

        var collections = new List<LookupDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COLLECTION_ID, NAME, DESCRIPTION
            FROM COLLECTION_EU@link_eu
            ORDER BY NAME";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt32(reader["COLLECTION_ID"]);

            collections.Add(new LookupDto
            {
                Id = id,
                Name = reader["NAME"] == DBNull.Value
                    ? $"Collection #{id}"
                    : reader["NAME"]?.ToString() ?? $"Collection #{id}",
                Description = reader["DESCRIPTION"] == DBNull.Value
                    ? null
                    : reader["DESCRIPTION"]?.ToString()
            });
        }

        return collections;
    }

    private async Task<List<ArtworkLookupDto>> GetGlobalArtworksAsync()
    {
        var connectionString = GetGlobalConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
            return new List<ArtworkLookupDto>();

        var artworks = new List<ArtworkLookupDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                c.ARTWORK_ID,
                c.TITLE,
                c.YEAR_CREATED,
                a.NAME AS ARTIST_NAME
            FROM ARTWORK_CORE@link_eu c
            JOIN ARTWORK_DETAILS@link_am d
              ON d.ARTWORK_ID = c.ARTWORK_ID
            LEFT JOIN ARTIST_EU@link_eu a
              ON a.ARTIST_ID = c.ARTIST_ID
            ORDER BY c.TITLE";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = Convert.ToInt32(reader["ARTWORK_ID"]);

            artworks.Add(new ArtworkLookupDto
            {
                Id = id,
                Title = reader["TITLE"] == DBNull.Value
                    ? $"Artwork #{id}"
                    : reader["TITLE"]?.ToString() ?? $"Artwork #{id}",
                ArtistName = reader["ARTIST_NAME"] == DBNull.Value
                    ? null
                    : reader["ARTIST_NAME"]?.ToString(),
                YearCreated = reader["YEAR_CREATED"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["YEAR_CREATED"])
            });
        }

        return artworks;
    }

    [HttpGet("artists")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetArtists()
    {
        if (IsGlobalSource())
        {
            var globalArtists = await GetGlobalArtistsAsync();
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(globalArtists));
        }

        if (!_context.IsMapped<ArtGallery.Domain.Entities.Artist>())
        {
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(
                Array.Empty<LookupDto>()));
        }

        var artists = await _context.Artists
            .OrderBy(a => a.Name)
            .Select(a => new LookupDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Nationality
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(artists));
    }

    [HttpGet("collections")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetCollections()
    {
        if (IsGlobalSource())
        {
            var globalCollections = await GetGlobalCollectionsAsync();
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(globalCollections));
        }

        if (!_context.IsMapped<ArtGallery.Domain.Entities.Collection>())
        {
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(
                Array.Empty<LookupDto>()));
        }

        var collections = await _context.Collections
            .OrderBy(c => c.Name)
            .Select(c => new LookupDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(collections));
    }

    [HttpGet("locations")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetLocations()
    {
        var locations = await GetGlobalLocationsAsync();
        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(locations));
    }

    [HttpGet("exhibitors")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetExhibitors()
    {
        if (IsGlobalSource())
        {
            var globalExhibitors = await GetGlobalExhibitorsAsync();
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(globalExhibitors));
        }

        if (!_context.IsMapped<ArtGallery.Domain.Entities.Exhibitor>())
        {
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(
                Array.Empty<LookupDto>()));
        }

        var exhibitorEntities = await _context.Exhibitors
            .OrderBy(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.City
            })
            .ToListAsync();

        var exhibitors = exhibitorEntities
            .Select(e => new LookupDto
            {
                Id = e.Id,
                Name = !string.IsNullOrWhiteSpace(e.Name)
                    ? e.Name
                    : $"Exhibitor #{e.Id}",
                Description = e.City
            })
            .ToList();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(exhibitors));
    }

    [HttpGet("staff")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetStaff()
    {
        if (!_context.IsMapped<ArtGallery.Domain.Entities.Staff>())
        {
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(
                Array.Empty<LookupDto>()));
        }

        var staff = await _context.Staff
            .OrderBy(s => s.Name)
            .Select(s => new LookupDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Role
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(staff));
    }

    [HttpGet("artworks")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkLookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkLookupDto>>>> GetArtworks()
    {
        if (IsGlobalSource())
        {
            var globalArtworks = await GetGlobalArtworksAsync();
            return Ok(ApiResponse<IEnumerable<ArtworkLookupDto>>.SuccessResponse(globalArtworks));
        }

        if (!_context.IsMapped<ArtGallery.Domain.Entities.Artwork>())
        {
            return Ok(ApiResponse<IEnumerable<ArtworkLookupDto>>.SuccessResponse(
                Array.Empty<ArtworkLookupDto>()));
        }

        var artworks = await _context.Artworks
            .OrderBy(a => a.Id)
            .Select(a => new ArtworkLookupDto
            {
                Id = a.Id,
                Title = !string.IsNullOrWhiteSpace(a.Title)
                    ? a.Title
                    : $"Artwork #{a.Id}",
                ArtistName = null,
                YearCreated = a.YearCreated
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<ArtworkLookupDto>>.SuccessResponse(artworks));
    }

    [HttpGet("policies")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetPolicies()
    {
        if (!_context.IsMapped<ArtGallery.Domain.Entities.InsurancePolicy>())
        {
            return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(
                Array.Empty<LookupDto>()));
        }

        var policies = await _context.InsurancePolicies
            .OrderBy(p => p.Provider)
            .Select(p => new LookupDto
            {
                Id = p.Id,
                Name = $"Policy #{p.Id}",
                Description = p.Provider
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(policies));
    }

    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<AllLookupsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AllLookupsDto>>> GetAllLookups()
    {
        if (IsGlobalSource())
        {
            var globalResult = new AllLookupsDto
            {
                Artists = await GetGlobalArtistsAsync(),
                Collections = await GetGlobalCollectionsAsync(),
                Locations = await GetGlobalLocationsAsync(),
                Exhibitors = await GetGlobalExhibitorsAsync(),
                Staff = _context.IsMapped<ArtGallery.Domain.Entities.Staff>()
                    ? await _context.Staff
                        .OrderBy(s => s.Name)
                        .Select(s => new LookupDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Role
                        })
                        .ToListAsync()
                    : new List<LookupDto>()
            };

            return Ok(ApiResponse<AllLookupsDto>.SuccessResponse(globalResult));
        }

        var result = new AllLookupsDto
        {
            Artists = _context.IsMapped<ArtGallery.Domain.Entities.Artist>()
                ? await _context.Artists
                    .OrderBy(a => a.Name)
                    .Select(a => new LookupDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Nationality
                    })
                    .ToListAsync()
                : new List<LookupDto>(),

            Collections = _context.IsMapped<ArtGallery.Domain.Entities.Collection>()
                ? await _context.Collections
                    .OrderBy(c => c.Name)
                    .Select(c => new LookupDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync()
                : new List<LookupDto>(),

            Locations = await GetGlobalLocationsAsync(),

            Exhibitors = _context.IsMapped<ArtGallery.Domain.Entities.Exhibitor>()
                ? await _context.Exhibitors
                    .OrderBy(e => e.Id)
                    .Select(e => new LookupDto
                    {
                        Id = e.Id,
                        Name = !string.IsNullOrWhiteSpace(e.Name)
                            ? e.Name
                            : $"Exhibitor #{e.Id}",
                        Description = e.City
                    })
                    .ToListAsync()
                : new List<LookupDto>(),

            Staff = _context.IsMapped<ArtGallery.Domain.Entities.Staff>()
                ? await _context.Staff
                    .OrderBy(s => s.Name)
                    .Select(s => new LookupDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Role
                    })
                    .ToListAsync()
                : new List<LookupDto>()
        };

        return Ok(ApiResponse<AllLookupsDto>.SuccessResponse(result));
    }
}

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ArtworkLookupDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ArtistName { get; set; }
    public int? YearCreated { get; set; }
}

public class AllLookupsDto
{
    public List<LookupDto> Artists { get; set; } = new();
    public List<LookupDto> Collections { get; set; } = new();
    public List<LookupDto> Locations { get; set; } = new();
    public List<LookupDto> Exhibitors { get; set; } = new();
    public List<LookupDto> Staff { get; set; } = new();
}