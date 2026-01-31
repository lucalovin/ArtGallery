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
    public string Status { get; set; } = "idle";
    public EtlSyncResponseDto? LastSync { get; set; }
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public double SuccessRate { get; set; }
    
    /// <summary>
    /// List of data sources with their record counts
    /// </summary>
    public List<EtlDataSourceDto> DataSources { get; set; } = new();
    
    /// <summary>
    /// Statistics summary
    /// </summary>
    public EtlStatsDto? Stats { get; set; }
}

/// <summary>
/// DTO for data source info in ETL status.
/// </summary>
public class EtlDataSourceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "connected";
    public int RecordCount { get; set; }
}

/// <summary>
/// DTO for ETL stats summary.
/// </summary>
public class EtlStatsDto
{
    public int TotalRecords { get; set; }
    public string Duration { get; set; } = "N/A";
    public double SuccessRate { get; set; }
    public int FailedRecords { get; set; }
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
