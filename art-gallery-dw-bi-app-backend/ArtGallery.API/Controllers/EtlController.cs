using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Etl;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

/// <summary>
/// ETL operations controller for OLTP to DW data propagation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EtlController : ControllerBase
{
    private readonly IEtlService _etlService;
    private readonly IOracleProcedureService _oracleProcedureService;
    private readonly ILogger<EtlController> _logger;

    public EtlController(
        IEtlService etlService,
        IOracleProcedureService oracleProcedureService,
        ILogger<EtlController> logger)
    {
        _etlService = etlService;
        _oracleProcedureService = oracleProcedureService;
        _logger = logger;
    }

    /// <summary>
    /// Gets paginated list of ETL sync records.
    /// </summary>
    [HttpGet("syncs")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<EtlSyncResponseDto>>>> GetSyncs([FromQuery] PagedRequest request)
    {
        var result = await _etlService.GetSyncsAsync(request);
        return Ok(ApiResponse<PaginatedResponse<EtlSyncResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets a specific ETL sync record by ID.
    /// </summary>
    [HttpGet("syncs/{id:int}")]
    public async Task<ActionResult<ApiResponse<EtlSyncResponseDto>>> GetSyncById(int id)
    {
        var result = await _etlService.GetSyncByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<EtlSyncResponseDto>.FailureResponse($"Sync record with ID {id} not found"));
        
        return Ok(ApiResponse<EtlSyncResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Triggers an ETL sync operation.
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<ApiResponse<EtlSyncResponseDto>>> TriggerSync([FromBody] TriggerSyncDto dto)
    {
        var result = await _etlService.TriggerSyncAsync(dto);
        return Ok(ApiResponse<EtlSyncResponseDto>.SuccessResponse(result, "ETL sync triggered successfully"));
    }

    /// <summary>
    /// Runs OLTP to DW propagation using Oracle PL/SQL procedures.
    /// </summary>
    /// <param name="request">Propagation request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("run-propagation")]
    [ProducesResponseType(typeof(ApiResponse<EtlPropagationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EtlPropagationResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EtlPropagationResult>>> RunPropagation(
        [FromBody] EtlPropagationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ETL propagation. Source: {Source}, Target: {Target}, Operation: {Operation}",
            request.Source, request.Target, request.Operation);

        var mode = request.Operation?.ToLower() == "full_load" 
            ? EtlMode.Full 
            : EtlMode.Incremental;

        var result = await _oracleProcedureService.PropagateOltpToDwAsync(mode, cancellationToken);

        if (result.Status == "Error")
        {
            var errorMessage = result.Errors.Count > 0 
                ? string.Join("; ", result.Errors) 
                : "ETL propagation failed";
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<EtlPropagationResult>.FailureResponse(errorMessage));
        }

        return Ok(ApiResponse<EtlPropagationResult>.SuccessResponse(result, "ETL propagation completed successfully"));
    }

    /// <summary>
    /// Gets current ETL status.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<EtlStatusDto>>> GetStatus()
    {
        var result = await _etlService.GetStatusAsync();
        return Ok(ApiResponse<EtlStatusDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets ETL field mappings configuration.
    /// </summary>
    [HttpGet("mappings")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EtlMappingDto>>>> GetMappings()
    {
        var result = await _etlService.GetMappingsAsync();
        return Ok(ApiResponse<IEnumerable<EtlMappingDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Updates ETL field mappings configuration.
    /// </summary>
    [HttpPut("mappings")]
    public async Task<IActionResult> UpdateMappings([FromBody] IEnumerable<EtlMappingDto> mappings)
    {
        await _etlService.UpdateMappingsAsync(mappings);
        return Ok(ApiResponse<object>.SuccessResponse(null!, "Mappings updated successfully"));
    }

    /// <summary>
    /// Gets ETL execution statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<EtlStatisticsDto>>> GetStatistics()
    {
        var result = await _etlService.GetStatisticsAsync();
        return Ok(ApiResponse<EtlStatisticsDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Validates data consistency between OLTP and DW.
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateDataConsistency()
    {
        var result = await _etlService.ValidateDataConsistencyAsync();
        return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Data is consistent" : "Data inconsistencies found"));
    }

    /// <summary>
    /// Validates referential integrity in the Data Warehouse.
    /// </summary>
    [HttpPost("validate-integrity")]
    [ProducesResponseType(typeof(ApiResponse<DataIntegrityResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DataIntegrityResult>>> ValidateReferentialIntegrity(
        CancellationToken cancellationToken = default)
    {
        var result = await _oracleProcedureService.ValidateReferentialIntegrityAsync(cancellationToken);
        
        var message = result.IsValid 
            ? "All referential integrity checks passed" 
            : $"Found {result.FailedChecks} integrity issues";
            
        return Ok(ApiResponse<DataIntegrityResult>.SuccessResponse(result, message));
    }

    /// <summary>
    /// Gets execution plan for a DW query (for optimization analysis).
    /// </summary>
    [HttpPost("explain-plan")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetExplainPlan(
        [FromBody] ExplainPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return BadRequest(ApiResponse<string>.FailureResponse("SQL query is required"));
        }

        var plan = await _oracleProcedureService.GetExplainPlanAsync(request.Sql, cancellationToken);
        return Ok(ApiResponse<string>.SuccessResponse(plan));
    }
}

/// <summary>
/// Request for explain plan endpoint.
/// </summary>
public class ExplainPlanRequest
{
    /// <summary>
    /// SQL query to analyze.
    /// </summary>
    public string Sql { get; set; } = string.Empty;
}
