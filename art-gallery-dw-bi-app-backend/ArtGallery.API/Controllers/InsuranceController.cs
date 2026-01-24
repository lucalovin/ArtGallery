using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Insurance;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InsuranceController : ControllerBase
{
    private readonly IInsuranceService _insuranceService;

    public InsuranceController(IInsuranceService insuranceService)
    {
        _insuranceService = insuranceService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<InsuranceResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _insuranceService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<InsuranceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<InsuranceResponseDto>>> GetById(int id)
    {
        var result = await _insuranceService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<InsuranceResponseDto>.FailureResponse($"Insurance policy with ID {id} not found"));
        
        return Ok(ApiResponse<InsuranceResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InsuranceResponseDto>>> Create([FromBody] CreateInsuranceDto dto)
    {
        var result = await _insuranceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<InsuranceResponseDto>.SuccessResponse(result, "Insurance policy created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<InsuranceResponseDto>>> Update(int id, [FromBody] UpdateInsuranceDto dto)
    {
        var result = await _insuranceService.UpdateAsync(id, dto);
        return Ok(ApiResponse<InsuranceResponseDto>.SuccessResponse(result, "Insurance policy updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _insuranceService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InsuranceResponseDto>>>> GetActive()
    {
        var result = await _insuranceService.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<InsuranceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InsuranceResponseDto>>>> GetExpiring([FromQuery] int days = 30)
    {
        var result = await _insuranceService.GetExpiringAsync(days);
        return Ok(ApiResponse<IEnumerable<InsuranceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<InsuranceStatisticsDto>>> GetStatistics()
    {
        var result = await _insuranceService.GetStatisticsAsync();
        return Ok(ApiResponse<InsuranceStatisticsDto>.SuccessResponse(result));
    }
}
