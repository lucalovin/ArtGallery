﻿namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Fact table for Exhibition Activity in the Data Warehouse.
/// Maps to FACT_EXHIBITION_ACTIVITY table in Oracle DW schema.
/// </summary>
public class FactExhibitionActivity
{
    /// <summary>
    /// Surrogate key. Maps to FACT_KEY.
    /// </summary>
    public long FactKey { get; set; }

    /// <summary>
    /// Foreign key to DimDate. Maps to DATE_KEY.
    /// </summary>
    public int DateKey { get; set; }

    /// <summary>
    /// Foreign key to DimExhibition. Maps to EXHIBITION_KEY.
    /// </summary>
    public int ExhibitionKey { get; set; }

    /// <summary>
    /// Foreign key to DimExhibitor. Maps to EXHIBITOR_KEY.
    /// </summary>
    public int ExhibitorKey { get; set; }

    /// <summary>
    /// Foreign key to DimArtwork. Maps to ARTWORK_KEY.
    /// </summary>
    public int ArtworkKey { get; set; }

    /// <summary>
    /// Foreign key to DimArtist. Maps to ARTIST_KEY.
    /// </summary>
    public int ArtistKey { get; set; }

    /// <summary>
    /// Foreign key to DimCollection. Maps to COLLECTION_KEY.
    /// </summary>
    public int? CollectionKey { get; set; }

    /// <summary>
    /// Foreign key to DimLocation. Maps to LOCATION_KEY.
    /// </summary>
    public int? LocationKey { get; set; }

    /// <summary>
    /// Foreign key to DimPolicy. Maps to POLICY_KEY.
    /// </summary>
    public int? PolicyKey { get; set; }

    // Measures

    /// <summary>
    /// Estimated value of the artwork. Maps to ESTIMATED_VALUE.
    /// </summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Insured amount. Maps to INSURED_AMOUNT.
    /// </summary>
    public decimal? InsuredAmount { get; set; }

    /// <summary>
    /// Loan flag (1=on loan, 0=not on loan). Maps to LOAN_FLAG.
    /// </summary>
    public int? LoanFlag { get; set; }

    /// <summary>
    /// Count of restorations. Maps to RESTORATION_COUNT.
    /// </summary>
    public int? RestorationCount { get; set; }

    /// <summary>
    /// Count of reviews. Maps to REVIEW_COUNT.
    /// </summary>
    public int? ReviewCount { get; set; }

    /// <summary>
    /// Average rating. Maps to AVG_RATING.
    /// </summary>
    public decimal? AvgRating { get; set; }

    // Navigation properties
    public virtual DimDate? Date { get; set; }
    public virtual DimExhibition? Exhibition { get; set; }
    public virtual DimArtwork? Artwork { get; set; }
    public virtual DimArtist? Artist { get; set; }
    public virtual DimLocation? Location { get; set; }
}
