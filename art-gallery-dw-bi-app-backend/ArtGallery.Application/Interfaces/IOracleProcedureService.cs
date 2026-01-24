using ArtGallery.Application.DTOs.Etl;

namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Service interface for executing Oracle PL/SQL procedures.
/// </summary>
public interface IOracleProcedureService
{
    /// <summary>
    /// Executes the OLTP to DW propagation procedure.
    /// </summary>
    /// <param name="mode">Sync mode (Full or Incremental)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ETL execution result</returns>
    Task<EtlPropagationResult> PropagateOltpToDwAsync(
        EtlMode mode = EtlMode.Incremental, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a named PL/SQL procedure.
    /// </summary>
    /// <param name="procedureName">Name of the procedure (with schema)</param>
    /// <param name="parameters">Dictionary of parameter names and values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected or output value</returns>
    Task<int> ExecuteProcedureAsync(
        string procedureName, 
        Dictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates referential integrity between OLTP and DW.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with any integrity issues</returns>
    Task<DataIntegrityResult> ValidateReferentialIntegrityAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the execution plan for a query (for optimization).
    /// </summary>
    /// <param name="sql">SQL query to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution plan as string</returns>
    Task<string> GetExplainPlanAsync(
        string sql, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ETL synchronization mode.
/// </summary>
public enum EtlMode
{
    /// <summary>
    /// Full load - truncate and reload all data.
    /// </summary>
    Full,
    
    /// <summary>
    /// Incremental load - only sync changes since last sync.
    /// </summary>
    Incremental
}
