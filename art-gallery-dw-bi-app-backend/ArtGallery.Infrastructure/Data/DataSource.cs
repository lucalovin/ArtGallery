using ArtGallery.Application.Interfaces;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// Default <see cref="IDataSourceContext"/> implementation plus
/// schema/connection-string helpers for the four Oracle sources.
/// </summary>
public sealed class DataSourceContext : IDataSourceContext
{
    public DataSource Source { get; set; } = DataSource.OLTP;

    /// <summary>Parse a header / query value to a <see cref="DataSource"/>.</summary>
    public static DataSource Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DataSource.OLTP;

        return value.Trim().ToUpperInvariant() switch
        {
            "OLTP"    => DataSource.OLTP,
            "AM"      => DataSource.AM,
            "EU"      => DataSource.EU,
            "GLOBAL"  => DataSource.GLOBAL,
            _         => DataSource.OLTP
        };
    }

    /// <summary>Map the data source to the configured connection-string key.</summary>
    public static string ConnectionStringKey(DataSource source) => source switch
    {
        DataSource.AM     => "BddAmConnection",
        DataSource.EU     => "BddEuConnection",
        DataSource.GLOBAL => "BddGlobalConnection",
        _                 => "OltpConnection"
    };

    /// <summary>Default Oracle schema used by EF for the given source.</summary>
    public static string DefaultSchema(DataSource source) => source switch
    {
        DataSource.AM     => "ARTGALLERY_AM",
        DataSource.EU     => "ARTGALLERY_EU",
        DataSource.GLOBAL => "ARTGALLERY_GLOBAL",
        _                 => "ART_GALLERY_OLTP"
    };
}
