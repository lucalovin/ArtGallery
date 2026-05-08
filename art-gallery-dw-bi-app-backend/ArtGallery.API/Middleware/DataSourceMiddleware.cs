using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;

namespace ArtGallery.API.Middleware;

/// <summary>
/// Reads the <c>X-Data-Source</c> request header (or <c>?dataSource=</c> query
/// string) and stores the parsed <see cref="DataSource"/> in the request-scoped
/// <see cref="IDataSourceContext"/>. Defaults to <see cref="DataSource.OLTP"/>.
///
/// Allows the frontend to switch the OLTP-style controllers to operate on
/// ARTGALLERY_AM, ARTGALLERY_EU or ARTGALLERY_GLOBAL without code changes.
/// </summary>
public sealed class DataSourceMiddleware
{
    public const string HeaderName = "X-Data-Source";
    public const string QueryName  = "dataSource";

    private readonly RequestDelegate _next;

    public DataSourceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDataSourceContext dataSource)
    {
        string? raw = null;

        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            raw = headerValues.ToString();
        }
        else if (context.Request.Query.TryGetValue(QueryName, out var queryValues))
        {
            raw = queryValues.ToString();
        }

        dataSource.Source = DataSourceContext.Parse(raw);

        // Echo the resolved source on the response so the UI can confirm it.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = dataSource.Source.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public static class DataSourceMiddlewareExtensions
{
    public static IApplicationBuilder UseDataSourceContext(this IApplicationBuilder app)
        => app.UseMiddleware<DataSourceMiddleware>();
}
