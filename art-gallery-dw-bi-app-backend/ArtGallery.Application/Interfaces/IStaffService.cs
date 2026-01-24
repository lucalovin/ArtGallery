using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Staff;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for staff operations.
/// </summary>
public interface IStaffService
{
    Task<PaginatedResponse<StaffResponseDto>> GetAllAsync(PagedRequest request);
    Task<StaffResponseDto?> GetByIdAsync(int id);
    Task<StaffResponseDto> CreateAsync(CreateStaffDto dto);
    Task<StaffResponseDto> UpdateAsync(int id, UpdateStaffDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<StaffResponseDto>> GetByDepartmentAsync(string department);
    Task<StaffStatisticsDto> GetStatisticsAsync();
}
