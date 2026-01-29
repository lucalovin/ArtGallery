using Microsoft.EntityFrameworkCore;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Art Gallery OLTP operations.
/// Configured to work with Oracle Database.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
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
    public DbSet<EtlSync> EtlSyncs => Set<EtlSync>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for Oracle
        modelBuilder.HasDefaultSchema("ART_GALLERY_OLTP");

        // Apply configurations from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
