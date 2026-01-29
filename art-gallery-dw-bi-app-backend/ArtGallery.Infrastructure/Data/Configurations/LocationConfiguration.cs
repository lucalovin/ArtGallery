﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Location entity.
/// </summary>
public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("LOCATION");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("LOCATION_ID");

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("NAME");

        builder.Property(l => l.GalleryRoom)
            .HasMaxLength(32)
            .HasColumnName("GALLERY_ROOM");

        builder.Property(l => l.Type)
            .HasMaxLength(32)
            .HasColumnName("TYPE");

        builder.Property(l => l.Capacity)
            .HasColumnName("CAPACITY");

        builder.HasIndex(l => l.Type);
    }
}
