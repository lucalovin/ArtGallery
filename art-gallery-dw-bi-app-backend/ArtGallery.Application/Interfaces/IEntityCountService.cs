namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for getting entity counts.
/// Used by ETL status to report record counts per data source.
/// </summary>
public interface IEntityCountService
{
    Task<int> GetArtworkCountAsync();
    Task<int> GetExhibitionCountAsync();
    Task<int> GetVisitorCountAsync();
    Task<int> GetStaffCountAsync();
    Task<int> GetLoanCountAsync();
}
