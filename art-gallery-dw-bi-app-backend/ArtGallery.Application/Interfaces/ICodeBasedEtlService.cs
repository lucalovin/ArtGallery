namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for code-based ETL propagation from OLTP to DW.
/// This provides a fallback when Oracle stored procedures are not available.
/// </summary>
public interface ICodeBasedEtlService
{
    /// <summary>
    /// Propagates all data from OLTP to DW dimension and fact tables.
    /// </summary>
    /// <param name="fullLoad">If true, truncates and reloads all data. If false, incremental update.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of total records processed</returns>
    Task<EtlResult> PropagateAllAsync(bool fullLoad = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propagates artists from OLTP to DW DIM_ARTIST.
    /// </summary>
    Task<int> PropagateArtistsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propagates artworks from OLTP to DW DIM_ARTWORK.
    /// </summary>
    Task<int> PropagateArtworksAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propagates exhibitions from OLTP to DW DIM_EXHIBITION.
    /// </summary>
    Task<int> PropagateExhibitionsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propagates visitors from OLTP to DW DIM_VISITOR.
    /// </summary>
    Task<int> PropagateVisitorsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Propagates staff from OLTP to DW DIM_STAFF.
    /// </summary>
    Task<int> PropagateStaffAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Populates the fact table from dimension data.
    /// </summary>
    Task<int> PopulateFactTableAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of code-based ETL execution.
/// </summary>
public class EtlResult
{
    public int TotalRecordsProcessed { get; set; }
    public int ArtistsProcessed { get; set; }
    public int ArtworksProcessed { get; set; }
    public int ExhibitionsProcessed { get; set; }
    public int VisitorsProcessed { get; set; }
    public int StaffProcessed { get; set; }
    public int FactRecordsProcessed { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
}
