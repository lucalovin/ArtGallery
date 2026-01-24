using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Insurance;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for insurance operations.
/// </summary>
public interface IInsuranceService
{
    Task<PaginatedResponse<InsuranceResponseDto>> GetAllAsync(PagedRequest request);
    Task<InsuranceResponseDto?> GetByIdAsync(int id);
    Task<InsuranceResponseDto> CreateAsync(CreateInsuranceDto dto);
    Task<InsuranceResponseDto> UpdateAsync(int id, UpdateInsuranceDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<InsuranceResponseDto>> GetActiveAsync();
    Task<IEnumerable<InsuranceResponseDto>> GetExpiringAsync(int daysThreshold = 30);
    Task<InsuranceStatisticsDto> GetStatisticsAsync();
}
