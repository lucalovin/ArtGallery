namespace ArtGallery.Application.DTOs.Etl;

/// <summary>
/// Result of an ETL propagation operation.
/// </summary>
public class EtlPropagationResult
{
    /// <summary>
    /// Status of the operation (Success, Error, Running).
    /// </summary>
    public string Status { get; set; } = "Success";

    /// <summary>
    /// Total records loaded.
    /// </summary>
    public int LoadedRecords { get; set; }

    /// <summary>
    /// Records inserted.
    /// </summary>
    public int InsertedRecords { get; set; }

    /// <summary>
    /// Records updated (for SCD).
    /// </summary>
    public int UpdatedRecords { get; set; }

    /// <summary>
    /// Records deleted/archived.
    /// </summary>
    public int DeletedRecords { get; set; }

    /// <summary>
    /// Duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Start timestamp.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End timestamp.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// List of errors encountered.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Breakdown by table.
    /// </summary>
    public List<TableSyncResult> TableResults { get; set; } = new();
}

/// <summary>
/// Sync result for individual table.
/// </summary>
public class TableSyncResult
{
    public string TableName { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsDeleted { get; set; }
    public long DurationMs { get; set; }
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of data integrity validation.
/// </summary>
public class DataIntegrityResult
{
    public bool IsValid { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public List<IntegrityIssue> Issues { get; set; } = new();
    public int TotalChecks { get; set; }
    public int PassedChecks { get; set; }
    public int FailedChecks { get; set; }
}

/// <summary>
/// Individual integrity issue.
/// </summary>
public class IntegrityIssue
{
    public string TableName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty; // OrphanRecord, MissingReference, DuplicateKey
    public string Description { get; set; } = string.Empty;
    public int AffectedRecords { get; set; }
    public string Severity { get; set; } = "Warning"; // Info, Warning, Error, Critical
}

/// <summary>
/// Request to trigger ETL propagation.
/// </summary>
public class EtlPropagationRequest
{
    /// <summary>
    /// Source system (OLTP).
    /// </summary>
    public string Source { get; set; } = "OLTP";

    /// <summary>
    /// Target system (DW).
    /// </summary>
    public string Target { get; set; } = "DW";

    /// <summary>
    /// Operation mode (full_load or incremental).
    /// </summary>
    public string Operation { get; set; } = "incremental";

    /// <summary>
    /// Optional timestamp for incremental sync.
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Specific tables to sync (null for all).
    /// </summary>
    public List<string>? Tables { get; set; }
}
