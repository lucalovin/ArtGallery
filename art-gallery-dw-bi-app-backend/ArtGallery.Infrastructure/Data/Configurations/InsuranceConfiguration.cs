﻿using Microsoft.EntityFrameworkCore;
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
        builder.ToTable("INSURANCE");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("INSURANCE_ID");

        builder.Property(i => i.ArtworkId)
            .IsRequired()
            .HasColumnName("ARTWORK_ID");

        builder.Property(i => i.PolicyId)
            .IsRequired()
            .HasColumnName("POLICY_ID");

        builder.Property(i => i.InsuredAmount)
            .IsRequired()
            .HasColumnType("NUMBER(14,2)")
            .HasColumnName("INSURED_AMOUNT");

        // Foreign key relationships
        builder.HasOne(i => i.Artwork)
            .WithMany(a => a.Insurances)
            .HasForeignKey(i => i.ArtworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Policy)
            .WithMany(p => p.Insurances)
            .HasForeignKey(i => i.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ArtworkId);
        builder.HasIndex(i => i.PolicyId);
    }
}
