﻿using Microsoft.EntityFrameworkCore;
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
        builder.ToTable("EXHIBITION");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("EXHIBITION_ID");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("TITLE");

        builder.Property(e => e.StartDate)
            .IsRequired()
            .HasColumnName("START_DATE");

        builder.Property(e => e.EndDate)
            .IsRequired()
            .HasColumnName("END_DATE");

        builder.Property(e => e.ExhibitorId)
            .IsRequired()
            .HasColumnName("EXHIBITOR_ID");

        builder.Property(e => e.Description)
            .HasMaxLength(512)
            .HasColumnName("DESCRIPTION");

        // Foreign key relationship
        builder.HasOne(e => e.Exhibitor)
            .WithMany(ex => ex.Exhibitions)
            .HasForeignKey(e => e.ExhibitorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ExhibitorId);
        builder.HasIndex(e => e.StartDate);
    }
}

/// <summary>
/// EF Core configuration for ExhibitionArtwork join entity.
/// </summary>
public class ExhibitionArtworkConfiguration : IEntityTypeConfiguration<ExhibitionArtwork>
{
    public void Configure(EntityTypeBuilder<ExhibitionArtwork> builder)
    {
        builder.ToTable("ARTWORK_EXHIBITION");

        // Composite primary key
        builder.HasKey(ea => new { ea.ArtworkId, ea.ExhibitionId });

        builder.Property(ea => ea.ArtworkId)
            .HasColumnName("ARTWORK_ID");

        builder.Property(ea => ea.ExhibitionId)
            .HasColumnName("EXHIBITION_ID");

        builder.Property(ea => ea.PositionInGallery)
            .HasMaxLength(64)
            .HasColumnName("POSITION_IN_GALLERY");

        builder.Property(ea => ea.FeaturedStatus)
            .HasMaxLength(16)
            .HasColumnName("FEATURED_STATUS");

        // Foreign key relationships
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
