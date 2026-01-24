using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Exhibition;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for exhibition operations.
/// </summary>
public interface IExhibitionService
{
    Task<PaginatedResponse<ExhibitionResponseDto>> GetAllAsync(PagedRequest request);
    Task<ExhibitionDetailDto?> GetByIdAsync(int id);
    Task<ExhibitionResponseDto> CreateAsync(CreateExhibitionDto dto);
    Task<ExhibitionResponseDto> UpdateAsync(int id, UpdateExhibitionDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ExhibitionResponseDto>> GetUpcomingAsync(int limit = 5);
    Task<IEnumerable<ExhibitionResponseDto>> GetActiveAsync();
    Task<IEnumerable<ExhibitionResponseDto>> GetPastAsync();
    Task AddArtworkAsync(int exhibitionId, int artworkId, int? displayOrder = null);
    Task RemoveArtworkAsync(int exhibitionId, int artworkId);
    Task<IEnumerable<ExhibitionArtworkDto>> GetArtworksAsync(int exhibitionId);
}
