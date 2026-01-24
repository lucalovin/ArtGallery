using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Restoration entity.
/// </summary>
public class RestorationConfiguration : IEntityTypeConfiguration<Restoration>
{
    public void Configure(EntityTypeBuilder<Restoration> builder)
    {
        builder.ToTable("Restorations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Scheduled");

        builder.Property(r => r.Conservator)
            .HasMaxLength(255);

        builder.Property(r => r.EstimatedCost)
            .HasPrecision(18, 2);

        builder.Property(r => r.ActualCost)
            .HasPrecision(18, 2);

        builder.Property(r => r.ConditionBefore)
            .HasMaxLength(500);

        builder.Property(r => r.ConditionAfter)
            .HasMaxLength(500);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        builder.HasOne(r => r.Artwork)
            .WithMany(a => a.Restorations)
            .HasForeignKey(r => r.ArtworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Status);
    }
}

/// <summary>
/// EF Core configuration for EtlSync entity.
/// </summary>
public class EtlSyncConfiguration : IEntityTypeConfiguration<EtlSync>
{
    public void Configure(EntityTypeBuilder<EtlSync> builder)
    {
        builder.ToTable("EtlSyncs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(e => e.SourceSystem)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TargetSystem)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.SyncType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.Details)
            .HasMaxLength(4000);

        builder.HasIndex(e => e.SyncDate);
        builder.HasIndex(e => e.Status);
    }
}
