using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Staff;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<StaffResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _staffService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<StaffResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StaffResponseDto>>> GetById(int id)
    {
        var result = await _staffService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<StaffResponseDto>.FailureResponse($"Staff with ID {id} not found"));
        
        return Ok(ApiResponse<StaffResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StaffResponseDto>>> Create([FromBody] CreateStaffDto dto)
    {
        var result = await _staffService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<StaffResponseDto>.SuccessResponse(result, "Staff member created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<StaffResponseDto>>> Update(int id, [FromBody] UpdateStaffDto dto)
    {
        var result = await _staffService.UpdateAsync(id, dto);
        return Ok(ApiResponse<StaffResponseDto>.SuccessResponse(result, "Staff member updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _staffService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("by-department/{department}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StaffResponseDto>>>> GetByDepartment(string department)
    {
        var result = await _staffService.GetByDepartmentAsync(department);
        return Ok(ApiResponse<IEnumerable<StaffResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<StaffStatisticsDto>>> GetStatistics()
    {
        var result = await _staffService.GetStatisticsAsync();
        return Ok(ApiResponse<StaffStatisticsDto>.SuccessResponse(result));
    }
}
