using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Etl;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for ETL operations.
/// </summary>
public interface IEtlService
{
    Task<PaginatedResponse<EtlSyncResponseDto>> GetSyncsAsync(PagedRequest request);
    Task<EtlSyncResponseDto?> GetSyncByIdAsync(int id);
    Task<EtlSyncResponseDto> TriggerSyncAsync(TriggerSyncDto dto);
    Task<EtlStatusDto> GetStatusAsync();
    Task<IEnumerable<EtlMappingDto>> GetMappingsAsync();
    Task UpdateMappingsAsync(IEnumerable<EtlMappingDto> mappings);
    Task<EtlStatisticsDto> GetStatisticsAsync();
    Task<bool> ValidateDataConsistencyAsync();
}
