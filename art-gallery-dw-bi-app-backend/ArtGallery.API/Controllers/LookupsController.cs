using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Controller for lookup data (dropdowns) to support FK relationships in forms.
/// Provides endpoints for Artists, Collections, Locations, Exhibitors, etc.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LookupsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LookupsController> _logger;

    public LookupsController(AppDbContext context, ILogger<LookupsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all artists for dropdown selection.
    /// </summary>
    [HttpGet("artists")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetArtists()
    {
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

    /// <summary>
    /// Gets all collections for dropdown selection.
    /// </summary>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetCollections()
    {
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

    /// <summary>
    /// Gets all locations for dropdown selection.
    /// </summary>
    [HttpGet("locations")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetLocations()
    {
        var locations = await _context.Locations
            .OrderBy(l => l.Name)
            .Select(l => new LookupDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = $"{l.GalleryRoom} - {l.Type}"
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(locations));
    }

    /// <summary>
    /// Gets all exhibitors for dropdown selection.
    /// </summary>
    [HttpGet("exhibitors")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetExhibitors()
    {
        var exhibitors = await _context.Exhibitors
            .OrderBy(e => e.Name)
            .Select(e => new LookupDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.City
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<LookupDto>>.SuccessResponse(exhibitors));
    }

    /// <summary>
    /// Gets all staff members for dropdown selection.
    /// </summary>
    [HttpGet("staff")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetStaff()
    {
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

    /// <summary>
    /// Gets all artworks for dropdown selection (for loans, exhibitions, etc.).
    /// </summary>
    [HttpGet("artworks")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkLookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkLookupDto>>>> GetArtworks()
    {
        var artworks = await _context.Artworks
            .Include(a => a.Artist)
            .OrderBy(a => a.Title)
            .Select(a => new ArtworkLookupDto
            {
                Id = a.Id,
                Title = a.Title,
                ArtistName = a.Artist != null ? a.Artist.Name : "Unknown",
                YearCreated = a.YearCreated
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<ArtworkLookupDto>>.SuccessResponse(artworks));
    }

    /// <summary>
    /// Gets all insurance policies for dropdown selection.
    /// </summary>
    [HttpGet("policies")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LookupDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupDto>>>> GetPolicies()
    {
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

    /// <summary>
    /// Gets all lookup data in a single request (for form initialization).
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<AllLookupsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AllLookupsDto>>> GetAllLookups()
    {
        var result = new AllLookupsDto
        {
            Artists = await _context.Artists
                .OrderBy(a => a.Name)
                .Select(a => new LookupDto { Id = a.Id, Name = a.Name, Description = a.Nationality })
                .ToListAsync(),

            Collections = await _context.Collections
                .OrderBy(c => c.Name)
                .Select(c => new LookupDto { Id = c.Id, Name = c.Name, Description = c.Description })
                .ToListAsync(),

            Locations = await _context.Locations
                .OrderBy(l => l.Name)
                .Select(l => new LookupDto { Id = l.Id, Name = l.Name, Description = $"{l.GalleryRoom} - {l.Type}" })
                .ToListAsync(),

            Exhibitors = await _context.Exhibitors
                .OrderBy(e => e.Name)
                .Select(e => new LookupDto { Id = e.Id, Name = e.Name, Description = e.City })
                .ToListAsync(),

            Staff = await _context.Staff
                .OrderBy(s => s.Name)
                .Select(s => new LookupDto { Id = s.Id, Name = s.Name, Description = s.Role })
                .ToListAsync()
        };

        return Ok(ApiResponse<AllLookupsDto>.SuccessResponse(result));
    }
}

/// <summary>
/// Generic lookup DTO for dropdowns.
/// </summary>
public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Artwork-specific lookup DTO with additional fields.
/// </summary>
public class ArtworkLookupDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ArtistName { get; set; }
    public int? YearCreated { get; set; }
}

/// <summary>
/// Container for all lookup data.
/// </summary>
public class AllLookupsDto
{
    public List<LookupDto> Artists { get; set; } = new();
    public List<LookupDto> Collections { get; set; } = new();
    public List<LookupDto> Locations { get; set; } = new();
    public List<LookupDto> Exhibitors { get; set; } = new();
    public List<LookupDto> Staff { get; set; } = new();
}
