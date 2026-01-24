﻿using Microsoft.EntityFrameworkCore;
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
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<Exhibition> Exhibitions => Set<Exhibition>();
    public DbSet<ExhibitionArtwork> ExhibitionArtworks => Set<ExhibitionArtwork>();
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Loan> Loans => Set<Loan>();
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

        // Configure Oracle-specific settings
        ConfigureOracleConventions(modelBuilder);
    }

    private void ConfigureOracleConventions(ModelBuilder modelBuilder)
    {
        // Configure all string properties to use VARCHAR2 by default
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Map decimal to NUMBER
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetColumnType("NUMBER(18,2)");
                }

                // Map DateTime to TIMESTAMP
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("TIMESTAMP");
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
            if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime))
            {
                updatedAtProperty.SetValue(entry.Entity, DateTime.UtcNow);
            }

            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Entity.GetType().GetProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
                {
                    createdAtProperty.SetValue(entry.Entity, DateTime.UtcNow);
                }
            }
        }
    }
}
