﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Collection entity.
/// </summary>
public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("COLLECTION");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("COLLECTION_ID");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("NAME");

        builder.Property(c => c.Description)
            .HasMaxLength(512)
            .HasColumnName("DESCRIPTION");

        builder.Property(c => c.CreatedDate)
            .HasColumnName("CREATED_DATE");

        builder.HasIndex(c => c.Name);
    }
}
