﻿using Microsoft.EntityFrameworkCore;
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
        builder.ToTable("RESTORATION");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("RESTORATION_ID");

        builder.Property(r => r.ArtworkId)
            .IsRequired()
            .HasColumnName("ARTWORK_ID");

        builder.Property(r => r.StaffId)
            .IsRequired()
            .HasColumnName("STAFF_ID");

        builder.Property(r => r.StartDate)
            .IsRequired()
            .HasColumnName("START_DATE");

        builder.Property(r => r.EndDate)
            .HasColumnName("END_DATE");

        builder.Property(r => r.Description)
            .HasMaxLength(512)
            .HasColumnName("DESCRIPTION");

        // Foreign key relationships
        builder.HasOne(r => r.Artwork)
            .WithMany(a => a.Restorations)
            .HasForeignKey(r => r.ArtworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Staff)
            .WithMany(s => s.Restorations)
            .HasForeignKey(r => r.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ArtworkId);
        builder.HasIndex(r => r.StaffId);
        builder.HasIndex(r => r.StartDate);
    }
}

/// <summary>
/// EF Core configuration for EtlSync entity.
/// </summary>
public class EtlSyncConfiguration : IEntityTypeConfiguration<EtlSync>
{
    public void Configure(EntityTypeBuilder<EtlSync> builder)
    {
        builder.ToTable("ETL_SYNC");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("ID");

        builder.Property(e => e.SyncDate)
            .IsRequired()
            .HasColumnName("SYNC_DATE");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending")
            .HasColumnName("STATUS");

        builder.Property(e => e.RecordsProcessed)
            .HasColumnName("RECORDS_PROCESSED");

        builder.Property(e => e.RecordsFailed)
            .HasColumnName("RECORDS_FAILED");

        builder.Property(e => e.Duration)
            .HasColumnName("DURATION");

        builder.Property(e => e.SourceSystem)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("SOURCE_SYSTEM");

        builder.Property(e => e.TargetSystem)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("TARGET_SYSTEM");

        builder.Property(e => e.SyncType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("SYNC_TYPE");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000)
            .HasColumnName("ERROR_MESSAGE");

        builder.Property(e => e.Details)
            .HasMaxLength(4000)
            .HasColumnName("DETAILS");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CREATED_AT");

        builder.HasIndex(e => e.SyncDate);
        builder.HasIndex(e => e.Status);
    }
}
