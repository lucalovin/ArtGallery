using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Review;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for gallery review operations.
/// </summary>
public interface IReviewService
{
    Task<PaginatedResponse<ReviewResponseDto>> GetAllAsync(PagedRequest request);
    Task<ReviewResponseDto?> GetByIdAsync(int id);
    Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto);
    Task<ReviewResponseDto> UpdateAsync(int id, UpdateReviewDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ReviewResponseDto>> GetByVisitorAsync(int visitorId);
    Task<IEnumerable<ReviewResponseDto>> GetByArtworkAsync(int artworkId);
    Task<IEnumerable<ReviewResponseDto>> GetByExhibitionAsync(int exhibitionId);
    Task<ReviewStatisticsDto> GetStatisticsAsync();
}
