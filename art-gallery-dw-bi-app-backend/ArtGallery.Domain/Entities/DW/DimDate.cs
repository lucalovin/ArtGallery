﻿namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Date dimension table for the Data Warehouse.
/// Maps to DIM_DATE table in Oracle DW schema.
/// </summary>
public class DimDate
{
    /// <summary>
    /// Surrogate key (YYYYMMDD format). Maps to DATE_KEY.
    /// </summary>
    public int DateKey { get; set; }

    /// <summary>
    /// Full calendar date. Maps to CALENDAR_DATE.
    /// </summary>
    public DateTime CalendarDate { get; set; }

    /// <summary>
    /// Calendar year. Maps to CALENDAR_YEAR.
    /// </summary>
    public int CalendarYear { get; set; }

    /// <summary>
    /// Calendar month (1-12). Maps to CALENDAR_MONTH.
    /// </summary>
    public int CalendarMonth { get; set; }

    /// <summary>
    /// Calendar day (1-31). Maps to CALENDAR_DAY.
    /// </summary>
    public int CalendarDay { get; set; }

    /// <summary>
    /// Month name (e.g., January). Maps to MONTH_NAME.
    /// </summary>
    public string? MonthName { get; set; }

    /// <summary>
    /// Quarter (1-4). Maps to QUARTER.
    /// </summary>
    public int? Quarter { get; set; }

    /// <summary>
    /// Is weekend flag ('Y' or 'N'). Maps to IS_WEEKEND.
    /// </summary>
    public string? IsWeekend { get; set; }
}
