using ArtGallery.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// EF Core caches the compiled model per <see cref="DbContext"/> type.
/// Because <see cref="AppDbContext"/> remaps tables / schema based on
/// <see cref="DataSource"/>, we need a separate cached model per source.
///
/// This factory adds the current <see cref="AppDbContext.CurrentSource"/>
/// to the cache key so each source gets its own compiled model.
/// </summary>
public sealed class DataSourceModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        if (context is AppDbContext app)
        {
            return (context.GetType(), app.CurrentSource, designTime);
        }
        return (context.GetType(), designTime);
    }
}
