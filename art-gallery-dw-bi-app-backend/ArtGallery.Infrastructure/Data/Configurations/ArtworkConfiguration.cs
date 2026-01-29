﻿using Microsoft.EntityFrameworkCore;
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
        builder.ToTable("ARTWORK");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("ARTWORK_ID");

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("TITLE");

        builder.Property(a => a.ArtistId)
            .IsRequired()
            .HasColumnName("ARTIST_ID");

        builder.Property(a => a.YearCreated)
            .HasColumnName("YEAR_CREATED");

        builder.Property(a => a.Medium)
            .HasMaxLength(64)
            .HasColumnName("MEDIUM");

        builder.Property(a => a.CollectionId)
            .HasColumnName("COLLECTION_ID");

        builder.Property(a => a.LocationId)
            .HasColumnName("LOCATION_ID");

        builder.Property(a => a.EstimatedValue)
            .HasColumnType("NUMBER(12,2)")
            .HasColumnName("ESTIMATED_VALUE");

        // Foreign key relationships
        builder.HasOne(a => a.Artist)
            .WithMany(ar => ar.Artworks)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Collection)
            .WithMany(c => c.Artworks)
            .HasForeignKey(a => a.CollectionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Location)
            .WithMany(l => l.Artworks)
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.ArtistId);
        builder.HasIndex(a => a.CollectionId);
        builder.HasIndex(a => a.LocationId);
    }
}
