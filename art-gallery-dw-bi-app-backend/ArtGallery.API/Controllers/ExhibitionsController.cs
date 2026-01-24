using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Exhibition;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExhibitionsController : ControllerBase
{
    private readonly IExhibitionService _exhibitionService;

    public ExhibitionsController(IExhibitionService exhibitionService)
    {
        _exhibitionService = exhibitionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ExhibitionResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _exhibitionService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ExhibitionResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExhibitionDetailDto>>> GetById(int id)
    {
        var result = await _exhibitionService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<ExhibitionDetailDto>.FailureResponse($"Exhibition with ID {id} not found"));
        
        return Ok(ApiResponse<ExhibitionDetailDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExhibitionResponseDto>>> Create([FromBody] CreateExhibitionDto dto)
    {
        var result = await _exhibitionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<ExhibitionResponseDto>.SuccessResponse(result, "Exhibition created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ExhibitionResponseDto>>> Update(int id, [FromBody] UpdateExhibitionDto dto)
    {
        var result = await _exhibitionService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ExhibitionResponseDto>.SuccessResponse(result, "Exhibition updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _exhibitionService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExhibitionResponseDto>>>> GetUpcoming([FromQuery] int limit = 5)
    {
        var result = await _exhibitionService.GetUpcomingAsync(limit);
        return Ok(ApiResponse<IEnumerable<ExhibitionResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExhibitionResponseDto>>>> GetActive()
    {
        var result = await _exhibitionService.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<ExhibitionResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("past")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExhibitionResponseDto>>>> GetPast()
    {
        var result = await _exhibitionService.GetPastAsync();
        return Ok(ApiResponse<IEnumerable<ExhibitionResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("{id:int}/artworks/{artworkId:int}")]
    public async Task<IActionResult> AddArtwork(int id, int artworkId, [FromQuery] int? displayOrder = null)
    {
        await _exhibitionService.AddArtworkAsync(id, artworkId, displayOrder);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Artwork added to exhibition"));
    }

    [HttpDelete("{id:int}/artworks/{artworkId:int}")]
    public async Task<IActionResult> RemoveArtwork(int id, int artworkId)
    {
        await _exhibitionService.RemoveArtworkAsync(id, artworkId);
        return NoContent();
    }

    [HttpGet("{id:int}/artworks")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExhibitionArtworkDto>>>> GetArtworks(int id)
    {
        var result = await _exhibitionService.GetArtworksAsync(id);
        return Ok(ApiResponse<IEnumerable<ExhibitionArtworkDto>>.SuccessResponse(result));
    }
}
