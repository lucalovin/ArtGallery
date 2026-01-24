namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Fact table for Exhibition Activity in the Data Warehouse.
/// This is a partitioned fact table tracking exhibition activities.
/// </summary>
public class FactExhibitionActivity
{
    /// <summary>
    /// Surrogate key.
    /// </summary>
    public long FactKey { get; set; }

    /// <summary>
    /// Foreign key to DimDate.
    /// </summary>
    public int DateKey { get; set; }

    /// <summary>
    /// Foreign key to DimExhibition.
    /// </summary>
    public int ExhibitionKey { get; set; }

    /// <summary>
    /// Foreign key to DimArtwork.
    /// </summary>
    public int ArtworkKey { get; set; }

    /// <summary>
    /// Foreign key to DimArtist.
    /// </summary>
    public int ArtistKey { get; set; }

    /// <summary>
    /// Foreign key to DimVisitor.
    /// </summary>
    public int? VisitorKey { get; set; }

    /// <summary>
    /// Foreign key to DimStaff (curator).
    /// </summary>
    public int? StaffKey { get; set; }

    /// <summary>
    /// Foreign key to DimLocation.
    /// </summary>
    public int? LocationKey { get; set; }

    /// <summary>
    /// Foreign key to DimInsurance.
    /// </summary>
    public int? InsuranceKey { get; set; }

    // Measures

    /// <summary>
    /// Number of visitors for this activity.
    /// </summary>
    public int VisitorCount { get; set; }

    /// <summary>
    /// Ticket revenue.
    /// </summary>
    public decimal TicketRevenue { get; set; }

    /// <summary>
    /// Merchandise revenue.
    /// </summary>
    public decimal MerchandiseRevenue { get; set; }

    /// <summary>
    /// Total revenue.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Duration of visit in minutes.
    /// </summary>
    public int? VisitDurationMinutes { get; set; }

    /// <summary>
    /// Insurance value at time of exhibition.
    /// </summary>
    public decimal? InsuranceValue { get; set; }

    /// <summary>
    /// Operating cost for the day.
    /// </summary>
    public decimal? OperatingCost { get; set; }

    /// <summary>
    /// Number of artworks on display.
    /// </summary>
    public int ArtworkCount { get; set; }

    /// <summary>
    /// Partition key for range partitioning (YYYY format).
    /// </summary>
    public int PartitionYear { get; set; }

    /// <summary>
    /// ETL timestamp.
    /// </summary>
    public DateTime EtlLoadDate { get; set; }

    // Navigation properties
    public virtual DimDate? Date { get; set; }
    public virtual DimExhibition? Exhibition { get; set; }
    public virtual DimArtwork? Artwork { get; set; }
    public virtual DimArtist? Artist { get; set; }
    public virtual DimVisitor? Visitor { get; set; }
    public virtual DimStaff? Staff { get; set; }
    public virtual DimLocation? Location { get; set; }
    public virtual DimInsurance? Insurance { get; set; }
}
