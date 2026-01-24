using ArtGallery.Application.DTOs.Artwork;
using ArtGallery.Application.DTOs.Common;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for artwork operations.
/// </summary>
public interface IArtworkService
{
    Task<PaginatedResponse<ArtworkListDto>> GetAllAsync(PagedRequest request);
    Task<ArtworkResponseDto?> GetByIdAsync(int id);
    Task<ArtworkResponseDto> CreateAsync(CreateArtworkDto dto);
    Task<ArtworkResponseDto> UpdateAsync(int id, UpdateArtworkDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ArtworkListDto>> SearchAsync(string query);
    Task<ArtworkStatisticsDto> GetStatisticsAsync();
    Task<IEnumerable<ArtworkListDto>> GetByArtistAsync(string artist);
    Task<IEnumerable<ArtworkListDto>> GetByCollectionAsync(string collection);
    Task<IEnumerable<ArtworkListDto>> GetByStatusAsync(string status);
}
