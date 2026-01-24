using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Restoration;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RestorationController : ControllerBase
{
    private readonly IRestorationService _restorationService;

    public RestorationController(IRestorationService restorationService)
    {
        _restorationService = restorationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<RestorationResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _restorationService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<RestorationResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<RestorationResponseDto>>> GetById(int id)
    {
        var result = await _restorationService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<RestorationResponseDto>.FailureResponse($"Restoration with ID {id} not found"));
        
        return Ok(ApiResponse<RestorationResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RestorationResponseDto>>> Create([FromBody] CreateRestorationDto dto)
    {
        var result = await _restorationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<RestorationResponseDto>.SuccessResponse(result, "Restoration record created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<RestorationResponseDto>>> Update(int id, [FromBody] UpdateRestorationDto dto)
    {
        var result = await _restorationService.UpdateAsync(id, dto);
        return Ok(ApiResponse<RestorationResponseDto>.SuccessResponse(result, "Restoration record updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _restorationService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("in-progress")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RestorationResponseDto>>>> GetInProgress()
    {
        var result = await _restorationService.GetInProgressAsync();
        return Ok(ApiResponse<IEnumerable<RestorationResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("completed")]
    public async Task<ActionResult<ApiResponse<IEnumerable<RestorationResponseDto>>>> GetCompleted()
    {
        var result = await _restorationService.GetCompletedAsync();
        return Ok(ApiResponse<IEnumerable<RestorationResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<RestorationStatisticsDto>>> GetStatistics()
    {
        var result = await _restorationService.GetStatisticsAsync();
        return Ok(ApiResponse<RestorationStatisticsDto>.SuccessResponse(result));
    }
}
