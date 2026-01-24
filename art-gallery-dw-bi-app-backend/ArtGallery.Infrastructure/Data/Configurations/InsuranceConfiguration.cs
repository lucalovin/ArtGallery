using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Insurance entity.
/// </summary>
public class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
{
    public void Configure(EntityTypeBuilder<Insurance> builder)
    {
        builder.ToTable("Insurances");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Provider)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.PolicyNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.CoverageAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.Premium)
            .HasPrecision(18, 2);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(i => i.CoverageType)
            .HasMaxLength(100);

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.HasOne(i => i.Artwork)
            .WithMany(a => a.Insurances)
            .HasForeignKey(i => i.ArtworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.EndDate);
        builder.HasIndex(i => i.PolicyNumber).IsUnique();
    }
}
