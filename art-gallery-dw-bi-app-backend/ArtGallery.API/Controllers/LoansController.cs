using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Loan;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<LoanResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _loanService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<LoanResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> GetById(int id)
    {
        var result = await _loanService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<LoanResponseDto>.FailureResponse($"Loan with ID {id} not found"));
        
        return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> Create([FromBody] CreateLoanDto dto)
    {
        var result = await _loanService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<LoanResponseDto>.SuccessResponse(result, "Loan created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<LoanResponseDto>>> Update(int id, [FromBody] UpdateLoanDto dto)
    {
        var result = await _loanService.UpdateAsync(id, dto);
        return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(result, "Loan updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _loanService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LoanResponseDto>>>> GetActive()
    {
        var result = await _loanService.GetActiveAsync();
        return Ok(ApiResponse<IEnumerable<LoanResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LoanResponseDto>>>> GetOverdue()
    {
        var result = await _loanService.GetOverdueAsync();
        return Ok(ApiResponse<IEnumerable<LoanResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<LoanStatisticsDto>>> GetStatistics()
    {
        var result = await _loanService.GetStatisticsAsync();
        return Ok(ApiResponse<LoanStatisticsDto>.SuccessResponse(result));
    }
}
