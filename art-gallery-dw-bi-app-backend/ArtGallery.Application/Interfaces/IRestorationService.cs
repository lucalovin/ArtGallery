using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Restoration;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for restoration operations.
/// </summary>
public interface IRestorationService
{
    Task<PaginatedResponse<RestorationResponseDto>> GetAllAsync(PagedRequest request);
    Task<RestorationResponseDto?> GetByIdAsync(int id);
    Task<RestorationResponseDto> CreateAsync(CreateRestorationDto dto);
    Task<RestorationResponseDto> UpdateAsync(int id, UpdateRestorationDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<RestorationResponseDto>> GetInProgressAsync();
    Task<IEnumerable<RestorationResponseDto>> GetCompletedAsync();
    Task<RestorationStatisticsDto> GetStatisticsAsync();
}
