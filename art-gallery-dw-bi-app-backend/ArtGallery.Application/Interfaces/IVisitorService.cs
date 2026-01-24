using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Visitor;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for visitor operations.
/// </summary>
public interface IVisitorService
{
    Task<PaginatedResponse<VisitorResponseDto>> GetAllAsync(PagedRequest request);
    Task<VisitorResponseDto?> GetByIdAsync(int id);
    Task<VisitorResponseDto> CreateAsync(CreateVisitorDto dto);
    Task<VisitorResponseDto> UpdateAsync(int id, UpdateVisitorDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<VisitorResponseDto>> GetMembersAsync();
    Task<VisitorStatisticsDto> GetStatisticsAsync();
    Task<IEnumerable<VisitorResponseDto>> SearchAsync(string query);
}
