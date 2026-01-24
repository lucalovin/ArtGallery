using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Visitor;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VisitorsController : ControllerBase
{
    private readonly IVisitorService _visitorService;

    public VisitorsController(IVisitorService visitorService)
    {
        _visitorService = visitorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<VisitorResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _visitorService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<VisitorResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VisitorResponseDto>>> GetById(int id)
    {
        var result = await _visitorService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<VisitorResponseDto>.FailureResponse($"Visitor with ID {id} not found"));
        
        return Ok(ApiResponse<VisitorResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VisitorResponseDto>>> Create([FromBody] CreateVisitorDto dto)
    {
        var result = await _visitorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<VisitorResponseDto>.SuccessResponse(result, "Visitor created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<VisitorResponseDto>>> Update(int id, [FromBody] UpdateVisitorDto dto)
    {
        var result = await _visitorService.UpdateAsync(id, dto);
        return Ok(ApiResponse<VisitorResponseDto>.SuccessResponse(result, "Visitor updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _visitorService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("members")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VisitorResponseDto>>>> GetMembers()
    {
        var result = await _visitorService.GetMembersAsync();
        return Ok(ApiResponse<IEnumerable<VisitorResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<VisitorStatisticsDto>>> GetStatistics()
    {
        var result = await _visitorService.GetStatisticsAsync();
        return Ok(ApiResponse<VisitorStatisticsDto>.SuccessResponse(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VisitorResponseDto>>>> Search([FromQuery] string q)
    {
        var result = await _visitorService.SearchAsync(q);
        return Ok(ApiResponse<IEnumerable<VisitorResponseDto>>.SuccessResponse(result));
    }
}
