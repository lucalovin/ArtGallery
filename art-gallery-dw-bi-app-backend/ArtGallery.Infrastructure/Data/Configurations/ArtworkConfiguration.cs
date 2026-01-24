using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Artwork entity.
/// </summary>
public class ArtworkConfiguration : IEntityTypeConfiguration<Artwork>
{
    public void Configure(EntityTypeBuilder<Artwork> builder)
    {
        builder.ToTable("Artworks");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Artist)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Medium)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Dimensions)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.ImageUrl)
            .HasMaxLength(500);

        builder.Property(a => a.Collection)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Available");

        builder.Property(a => a.EstimatedValue)
            .HasPrecision(18, 2);

        builder.Property(a => a.Location)
            .HasMaxLength(100);

        builder.Property(a => a.AcquisitionMethod)
            .HasMaxLength(100);

        builder.Property(a => a.Provenance)
            .HasMaxLength(2000);

        builder.Property(a => a.Condition)
            .HasMaxLength(100);

        // Store Tags as JSON
        builder.Property(a => a.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        builder.HasIndex(a => a.Artist);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Collection);
    }
}
