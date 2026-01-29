﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Loan entity.
/// </summary>
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("LOAN");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("LOAN_ID");

        builder.Property(l => l.ArtworkId)
            .IsRequired()
            .HasColumnName("ARTWORK_ID");

        builder.Property(l => l.ExhibitorId)
            .IsRequired()
            .HasColumnName("EXHIBITOR_ID");

        builder.Property(l => l.StartDate)
            .IsRequired()
            .HasColumnName("START_DATE");

        builder.Property(l => l.EndDate)
            .HasColumnName("END_DATE");

        builder.Property(l => l.Conditions)
            .HasMaxLength(512)
            .HasColumnName("CONDITIONS");

        // Foreign key relationships
        builder.HasOne(l => l.Artwork)
            .WithMany(a => a.Loans)
            .HasForeignKey(l => l.ArtworkId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Exhibitor)
            .WithMany(e => e.Loans)
            .HasForeignKey(l => l.ExhibitorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.ArtworkId);
        builder.HasIndex(l => l.ExhibitorId);
        builder.HasIndex(l => l.StartDate);
    }
}
