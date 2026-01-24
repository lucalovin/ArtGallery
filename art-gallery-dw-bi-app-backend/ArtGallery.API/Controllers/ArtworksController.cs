using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Artwork;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Controller for artwork operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ArtworksController : ControllerBase
{
    private readonly IArtworkService _artworkService;
    private readonly ILogger<ArtworksController> _logger;

    public ArtworksController(IArtworkService artworkService, ILogger<ArtworksController> logger)
    {
        _artworkService = artworkService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all artworks with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ArtworkListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ArtworkListDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _artworkService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ArtworkListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets an artwork by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ArtworkResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArtworkResponseDto>>> GetById(int id)
    {
        var result = await _artworkService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<ArtworkResponseDto>.FailureResponse($"Artwork with ID {id} not found"));
        
        return Ok(ApiResponse<ArtworkResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Creates a new artwork.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ArtworkResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ArtworkResponseDto>>> Create([FromBody] CreateArtworkDto dto)
    {
        var result = await _artworkService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<ArtworkResponseDto>.SuccessResponse(result, "Artwork created successfully"));
    }

    /// <summary>
    /// Updates an existing artwork.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ArtworkResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArtworkResponseDto>>> Update(int id, [FromBody] UpdateArtworkDto dto)
    {
        var result = await _artworkService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ArtworkResponseDto>.SuccessResponse(result, "Artwork updated successfully"));
    }

    /// <summary>
    /// Partially updates an artwork.
    /// </summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ArtworkResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArtworkResponseDto>>> Patch(int id, [FromBody] UpdateArtworkDto dto)
    {
        var result = await _artworkService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ArtworkResponseDto>.SuccessResponse(result, "Artwork updated successfully"));
    }

    /// <summary>
    /// Deletes an artwork.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _artworkService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Searches artworks by title or artist.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkListDto>>>> Search([FromQuery] string q)
    {
        var result = await _artworkService.SearchAsync(q);
        return Ok(ApiResponse<IEnumerable<ArtworkListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets artwork statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<ArtworkStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ArtworkStatisticsDto>>> GetStatistics()
    {
        var result = await _artworkService.GetStatisticsAsync();
        return Ok(ApiResponse<ArtworkStatisticsDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets artworks by artist.
    /// </summary>
    [HttpGet("by-artist/{artist}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkListDto>>>> GetByArtist(string artist)
    {
        var result = await _artworkService.GetByArtistAsync(artist);
        return Ok(ApiResponse<IEnumerable<ArtworkListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets artworks by collection.
    /// </summary>
    [HttpGet("by-collection/{collection}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkListDto>>>> GetByCollection(string collection)
    {
        var result = await _artworkService.GetByCollectionAsync(collection);
        return Ok(ApiResponse<IEnumerable<ArtworkListDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets artworks by status.
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArtworkListDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtworkListDto>>>> GetByStatus(string status)
    {
        var result = await _artworkService.GetByStatusAsync(status);
        return Ok(ApiResponse<IEnumerable<ArtworkListDto>>.SuccessResponse(result));
    }
}
