using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Loan;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for loan operations.
/// </summary>
public interface ILoanService
{
    Task<PaginatedResponse<LoanResponseDto>> GetAllAsync(PagedRequest request);
    Task<LoanResponseDto?> GetByIdAsync(int id);
    Task<LoanResponseDto> CreateAsync(CreateLoanDto dto);
    Task<LoanResponseDto> UpdateAsync(int id, UpdateLoanDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<LoanResponseDto>> GetActiveAsync();
    Task<IEnumerable<LoanResponseDto>> GetOverdueAsync();
    Task<LoanStatisticsDto> GetStatisticsAsync();
}
