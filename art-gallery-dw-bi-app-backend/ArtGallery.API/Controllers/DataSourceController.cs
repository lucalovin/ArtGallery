using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Exposes which OLTP-style data source is currently being used and which are
/// available. The frontend uses this to render the "schema switcher".
/// </summary>
[ApiController]
[Route("api/datasource")]
[Produces("application/json")]
public class DataSourceController : ControllerBase
{
    private readonly IDataSourceContext _dataSourceContext;
    private readonly IConfiguration _configuration;

    public DataSourceController(IDataSourceContext dataSourceContext, IConfiguration configuration)
    {
        _dataSourceContext = dataSourceContext;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var sources = new[] { DataSource.OLTP, DataSource.AM, DataSource.EU, DataSource.GLOBAL };
        return Ok(new
        {
            current = _dataSourceContext.Source.ToString(),
            available = sources.Select(s => new
            {
                code = s.ToString(),
                schema = DataSourceContext.DefaultSchema(s),
                connectionConfigured = !string.IsNullOrWhiteSpace(
                    _configuration.GetConnectionString(DataSourceContext.ConnectionStringKey(s))),
                supports = SupportMatrix(s)
            })
        });
    }

    /// <summary>
    /// Lists which logical entities are usable on the given data source.
    /// The frontend uses this to disable / hide modules that don't apply.
    /// </summary>
    private static object SupportMatrix(DataSource source) => source switch
    {
        DataSource.OLTP => new
        {
            artworks = true,  exhibitions = true, exhibitors = true,
            artists = true,   collections = true, locations = true,
            visitors = true,  staff = true,       loans = true,
            insurance = true, restorations = true, reviews = true,
            etl = true,       analytics = true
        },
        DataSource.AM => new
        {
            // AM holds only the AM-side fragments + replicated dim copies.
            // Artwork is the vertical "details" half (no title/artist/year/medium).
            artworks = true,  exhibitions = true, exhibitors = true,
            artists = true,   collections = true, locations = false,
            visitors = false, staff = false,      loans = true,
            insurance = false, restorations = false, reviews = true,
            etl = false,      analytics = false
        },
        DataSource.EU => new
        {
            // EU holds the EU-side fragments + replicated dim copies.
            // Artwork is the vertical "core" half (no location/value).
            artworks = true,  exhibitions = true, exhibitors = true,
            artists = true,   collections = true, locations = false,
            visitors = false, staff = false,      loans = true,
            insurance = false, restorations = false, reviews = true,
            etl = false,      analytics = false
        },
        DataSource.GLOBAL => new
        {
            // GLOBAL has physical Location/Visitor/Staff/Insurance/Restoration
            // and reconstructs Artwork / Exhibitor through views.
            artworks = true,  exhibitions = false, exhibitors = true,
            artists = false,  collections = false, locations = true,
            visitors = true,  staff = true,        loans = false,
            insurance = true, restorations = true, reviews = false,
            etl = false,      analytics = false
        },
        _ => new { }
    };
}
