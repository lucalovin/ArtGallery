namespace ArtGallery.Domain.Entities.DW;

/// <summary>
/// Date dimension table for the Data Warehouse.
/// </summary>
public class DimDate
{
    /// <summary>
    /// Surrogate key (YYYYMMDD format).
    /// </summary>
    public int DateKey { get; set; }

    /// <summary>
    /// Full date.
    /// </summary>
    public DateTime FullDate { get; set; }

    /// <summary>
    /// Day of week (1-7).
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Day name (e.g., Monday).
    /// </summary>
    public string DayName { get; set; } = string.Empty;

    /// <summary>
    /// Day of month (1-31).
    /// </summary>
    public int DayOfMonth { get; set; }

    /// <summary>
    /// Day of year (1-366).
    /// </summary>
    public int DayOfYear { get; set; }

    /// <summary>
    /// Week of year.
    /// </summary>
    public int WeekOfYear { get; set; }

    /// <summary>
    /// Month number (1-12).
    /// </summary>
    public int MonthNumber { get; set; }

    /// <summary>
    /// Month name (e.g., January).
    /// </summary>
    public string MonthName { get; set; } = string.Empty;

    /// <summary>
    /// Quarter (1-4).
    /// </summary>
    public int Quarter { get; set; }

    /// <summary>
    /// Year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Fiscal year.
    /// </summary>
    public int FiscalYear { get; set; }

    /// <summary>
    /// Fiscal quarter.
    /// </summary>
    public int FiscalQuarter { get; set; }

    /// <summary>
    /// Is weekend flag.
    /// </summary>
    public bool IsWeekend { get; set; }

    /// <summary>
    /// Is holiday flag.
    /// </summary>
    public bool IsHoliday { get; set; }

    /// <summary>
    /// Holiday name if applicable.
    /// </summary>
    public string? HolidayName { get; set; }
}
