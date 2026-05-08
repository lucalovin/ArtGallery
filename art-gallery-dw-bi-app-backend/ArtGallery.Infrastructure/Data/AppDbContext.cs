using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Art Gallery OLTP operations.
///
/// Originally bound to the <c>ART_GALLERY_OLTP</c> schema. The context now also
/// understands the three distributed schemas introduced by the MODBD module:
/// <c>ARTGALLERY_AM</c>, <c>ARTGALLERY_EU</c> and <c>ARTGALLERY_GLOBAL</c>.
///
/// The active schema is selected per-request through <see cref="IDataSourceContext"/>
/// (populated by <c>DataSourceMiddleware</c> from the <c>X-Data-Source</c> header).
/// EF caches a separate compiled model per <see cref="DataSource"/> via
/// <see cref="DataSourceModelCacheKeyFactory"/>.
///
/// Important: only entities that physically exist on the selected schema can be
/// queried successfully. AM and EU expose only a subset (Artist, Collection,
/// Exhibitor, Exhibition, Loan, GalleryReview, Artwork[Details/Core]); GLOBAL
/// exposes the unified base tables plus the GLOBAL_* views. The frontend should
/// disable / hide modules that aren't supported on the current source.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Data source resolved for this scoped instance.</summary>
    public DataSource CurrentSource { get; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        CurrentSource = DataSource.OLTP;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IDataSourceContext dataSourceContext)
        : base(options)
    {
        CurrentSource = dataSourceContext?.Source ?? DataSource.OLTP;
    }

    // OLTP Tables
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<Exhibitor> Exhibitors => Set<Exhibitor>();
    public DbSet<Exhibition> Exhibitions => Set<Exhibition>();
    public DbSet<ExhibitionArtwork> ExhibitionArtworks => Set<ExhibitionArtwork>();
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<InsurancePolicy> InsurancePolicies => Set<InsurancePolicy>();
    public DbSet<Insurance> Insurances => Set<Insurance>();
    public DbSet<Restoration> Restorations => Set<Restoration>();
    public DbSet<GalleryReview> GalleryReviews => Set<GalleryReview>();
    public DbSet<EtlSync> EtlSyncs => Set<EtlSync>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Default schema and entity configurations follow OLTP conventions.
        modelBuilder.HasDefaultSchema(DataSourceContext.DefaultSchema(CurrentSource));
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Re-map tables for distributed schemas (AM / EU / GLOBAL).
        ApplyDataSourceTableMapping(modelBuilder, CurrentSource);
    }

    /// <summary>
    /// Removes any foreign keys on <typeparamref name="TEntity"/> that point to
    /// principals listed in <paramref name="principalClrTypes"/>. This is needed
    /// when remapping an entity to a fragment table that doesn't carry those
    /// FK columns; <c>Ignore(...)</c> alone is not enough because EF conventions
    /// re-discover relationships via the inverse navigation.
    /// </summary>
    private static void RemoveRelationships<TEntity>(ModelBuilder mb, params Type[] principalClrTypes)
        where TEntity : class
    {
        var entity = mb.Entity<TEntity>().Metadata;
        foreach (var fk in entity.GetForeignKeys().ToList())
        {
            if (principalClrTypes.Contains(fk.PrincipalEntityType.ClrType))
                entity.RemoveForeignKey(fk);
        }
    }

    private static void ApplyDataSourceTableMapping(ModelBuilder mb, DataSource source)
    {
        switch (source)
        {
            case DataSource.OLTP:
                // Defaults already applied by entity configurations.
                break;

            case DataSource.AM:
                // Drop OLTP-defined relationships that don't fit AM fragment
                // BEFORE re-mapping, so EF doesn't keep shadow FK columns.
                RemoveRelationships<Artwork>(mb, typeof(Artist), typeof(Collection), typeof(Location));
                // ARTGALLERY_AM: localized fragments + AM-side replicas.
                mb.Entity<Artist>().ToTable("ARTIST_AM");
                mb.Entity<Collection>().ToTable("COLLECTION_AM");
                mb.Entity<Exhibitor>().ToTable("EXHIBITOR_AM");
                mb.Entity<Exhibition>().ToTable("EXHIBITION_AM");
                mb.Entity<ExhibitionArtwork>().ToTable("ARTWORK_EXHIBITION_AM");
                mb.Entity<Loan>().ToTable("LOAN_AM");
                mb.Entity<GalleryReview>().ToTable("GALLERY_REVIEW_AM");
                // Vertical fragment: AM holds only artwork_id + location_id +
                // estimated_value. Ignore the columns / navigations that live
                // on the EU half (ARTWORK_CORE) so EF doesn't SELECT them.
                mb.Entity<Artwork>(e =>
                {
                    e.ToTable("ARTWORK_DETAILS");
                    e.Ignore(a => a.Title);
                    e.Ignore(a => a.ArtistId);
                    e.Ignore(a => a.YearCreated);
                    e.Ignore(a => a.Medium);
                    e.Ignore(a => a.CollectionId);
                    e.Ignore(a => a.Artist);
                    e.Ignore(a => a.Collection);
                    e.Ignore(a => a.Location);
                });
                // Drop the inverse collections so EF doesn't re-discover
                // shadow ArtistId / CollectionId FKs on the Artwork entity.
                mb.Entity<Artist>().Ignore(a => a.Artworks);
                mb.Entity<Collection>().Ignore(c => c.Artworks);
                // Entities below do not exist on AM; map to null so EF won't query them.
                mb.Entity<Location>().ToTable((string?)null);
                mb.Entity<Visitor>().ToTable((string?)null);
                mb.Entity<Staff>().ToTable((string?)null);
                mb.Entity<InsurancePolicy>().ToTable((string?)null);
                mb.Entity<Insurance>().ToTable((string?)null);
                mb.Entity<Restoration>().ToTable((string?)null);
                mb.Entity<EtlSync>().ToTable((string?)null);
                break;

            case DataSource.EU:
                // EU has no Location FK on Artwork.
                RemoveRelationships<Artwork>(mb, typeof(Location));
                // ARTGALLERY_EU: localized fragments + EU-side replicas.
                mb.Entity<Artist>().ToTable("ARTIST_EU");
                mb.Entity<Collection>().ToTable("COLLECTION_EU");
                mb.Entity<Exhibitor>().ToTable("EXHIBITOR_EU");
                mb.Entity<Exhibition>().ToTable("EXHIBITION_EU");
                mb.Entity<ExhibitionArtwork>().ToTable("ARTWORK_EXHIBITION_EU");
                mb.Entity<Loan>().ToTable("LOAN_EU");
                mb.Entity<GalleryReview>().ToTable("GALLERY_REVIEW_EU");
                // Vertical fragment: EU holds artwork_id + title + artist_id +
                // year_created + medium + collection_id (no location / value).
                mb.Entity<Artwork>(e =>
                {
                    e.ToTable("ARTWORK_CORE");
                    e.Ignore(a => a.LocationId);
                    e.Ignore(a => a.EstimatedValue);
                    e.Ignore(a => a.Location);
                });
                // Inverse navigation would otherwise re-introduce a shadow
                // LocationId FK column on ARTWORK_CORE.
                mb.Entity<Location>().Ignore(l => l.Artworks);
                mb.Entity<Location>().ToTable((string?)null);
                mb.Entity<Visitor>().ToTable((string?)null);
                mb.Entity<Staff>().ToTable((string?)null);
                mb.Entity<InsurancePolicy>().ToTable((string?)null);
                mb.Entity<Insurance>().ToTable((string?)null);
                mb.Entity<Restoration>().ToTable((string?)null);
                mb.Entity<EtlSync>().ToTable((string?)null);
                break;

            case DataSource.GLOBAL:
                // ARTGALLERY_GLOBAL holds physical tables (Location/Visitor/Staff/
                // Insurance/Restoration) and exposes GLOBAL_* views that UNION the
                // regional fragments via DB links to ARTGALLERY_AM / ARTGALLERY_EU.
                mb.Entity<Artwork>().ToTable("GLOBAL_ARTWORK");
                mb.Entity<Exhibitor>().ToTable("GLOBAL_EXHIBITOR");
                mb.Entity<Exhibition>().ToTable("GLOBAL_EXHIBITION");
                mb.Entity<ExhibitionArtwork>().ToTable("GLOBAL_ARTWORK_EXHIBITION");
                mb.Entity<Loan>().ToTable("GLOBAL_LOAN");
                mb.Entity<GalleryReview>().ToTable("GLOBAL_GALLERY_REVIEW");
                mb.Entity<Artist>().ToTable("GLOBAL_ARTIST");
                mb.Entity<Collection>().ToTable("GLOBAL_COLLECTION");
                // EtlSync only exists in OLTP.
                mb.Entity<EtlSync>().ToTable((string?)null);
                break;
        }
    }
}
