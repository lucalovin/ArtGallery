namespace ArtGallery.Application.DTOs.Etl;

/// <summary>
/// DTO for ETL sync response.
/// </summary>
public class EtlSyncResponseDto
{
    public int Id { get; set; }
    public DateTime SyncDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsFailed { get; set; }
    public long Duration { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string SyncType { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for triggering an ETL sync.
/// </summary>
public class TriggerSyncDto
{
    public string SyncType { get; set; } = "Full";
    public string SourceSystem { get; set; } = "OLTP";
    public string TargetSystem { get; set; } = "DW";
}

/// <summary>
/// DTO for ETL status.
/// </summary>
public class EtlStatusDto
{
    public bool IsRunning { get; set; }
    public EtlSyncResponseDto? LastSync { get; set; }
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// DTO for ETL field mapping.
/// </summary>
public class EtlMappingDto
{
    public string SourceTable { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public List<FieldMappingDto> FieldMappings { get; set; } = new();
}

/// <summary>
/// DTO for field mapping details.
/// </summary>
public class FieldMappingDto
{
    public string SourceField { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public string? Transformation { get; set; }
}

/// <summary>
/// DTO for ETL statistics.
/// </summary>
public class EtlStatisticsDto
{
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public double SuccessRate { get; set; }
    public long AverageDuration { get; set; }
    public int TotalRecordsProcessed { get; set; }
    public int TotalRecordsFailed { get; set; }
    public Dictionary<string, int> BySyncType { get; set; } = new();
}
