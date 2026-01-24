using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Exhibition entity.
/// </summary>
public class ExhibitionConfiguration : IEntityTypeConfiguration<Exhibition>
{
    public void Configure(EntityTypeBuilder<Exhibition> builder)
    {
        builder.ToTable("Exhibitions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Planning");

        builder.Property(e => e.Location)
            .HasMaxLength(100);

        builder.Property(e => e.Curator)
            .HasMaxLength(100);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Budget)
            .HasPrecision(18, 2);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.EndDate);
    }
}

/// <summary>
/// EF Core configuration for ExhibitionArtwork join entity.
/// </summary>
public class ExhibitionArtworkConfiguration : IEntityTypeConfiguration<ExhibitionArtwork>
{
    public void Configure(EntityTypeBuilder<ExhibitionArtwork> builder)
    {
        builder.ToTable("ExhibitionArtworks");

        builder.HasKey(ea => new { ea.ExhibitionId, ea.ArtworkId });

        builder.HasOne(ea => ea.Exhibition)
            .WithMany(e => e.ExhibitionArtworks)
            .HasForeignKey(ea => ea.ExhibitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ea => ea.Artwork)
            .WithMany(a => a.ExhibitionArtworks)
            .HasForeignKey(ea => ea.ArtworkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
