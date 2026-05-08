namespace ArtGallery.Application.Interfaces;

/// <summary>
/// Identifies which physical Oracle schema the application's
/// "OLTP" services should target for the current request.
/// </summary>
public enum DataSource
{
    OLTP = 0,
    AM = 1,
    EU = 2,
    GLOBAL = 3
}

/// <summary>
/// Per-request scoped service exposing the active <see cref="DataSource"/>.
/// Populated by <c>DataSourceMiddleware</c> from the <c>X-Data-Source</c> header.
/// </summary>
public interface IDataSourceContext
{
    DataSource Source { get; set; }
}

/// <summary>
/// Helpers describing which entities/operations are physically available
/// on a given source. Used by services to short-circuit gracefully instead
/// of letting EF blow up on unmapped tables.
/// </summary>
public static class DataSourceCapabilities
{
    public static bool HasArtist(DataSource s)        => s is DataSource.OLTP or DataSource.AM or DataSource.EU or DataSource.GLOBAL;
    public static bool HasCollection(DataSource s)    => s is DataSource.OLTP or DataSource.AM or DataSource.EU or DataSource.GLOBAL;
    public static bool HasLocation(DataSource s)      => s is DataSource.OLTP or DataSource.GLOBAL;
    public static bool HasVisitor(DataSource s)       => s is DataSource.OLTP or DataSource.GLOBAL;
    public static bool HasStaff(DataSource s)         => s is DataSource.OLTP or DataSource.GLOBAL;
    public static bool HasInsurance(DataSource s)     => s is DataSource.OLTP or DataSource.GLOBAL;
    public static bool HasRestoration(DataSource s)   => s is DataSource.OLTP or DataSource.GLOBAL;
    public static bool HasExhibition(DataSource s)    => s is DataSource.OLTP or DataSource.AM or DataSource.EU or DataSource.GLOBAL;
    public static bool HasLoan(DataSource s)          => s is DataSource.OLTP or DataSource.AM or DataSource.EU or DataSource.GLOBAL;
    public static bool HasGalleryReview(DataSource s) => s is DataSource.OLTP or DataSource.AM or DataSource.EU or DataSource.GLOBAL;
    public static bool HasEtl(DataSource s)           => s is DataSource.OLTP;

    /// <summary>True when Artwork carries the OLTP-style scalar columns (title, artist_id, year_created, medium, collection_id).</summary>
    public static bool ArtworkHasCore(DataSource s)    => s is DataSource.OLTP or DataSource.EU or DataSource.GLOBAL;
    /// <summary>True when Artwork carries the OLTP-style fragment columns (location_id, estimated_value).</summary>
    public static bool ArtworkHasDetails(DataSource s) => s is DataSource.OLTP or DataSource.AM or DataSource.GLOBAL;
}
