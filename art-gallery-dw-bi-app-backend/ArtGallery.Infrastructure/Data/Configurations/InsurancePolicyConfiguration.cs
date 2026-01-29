﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for InsurancePolicy entity.
/// </summary>
public class InsurancePolicyConfiguration : IEntityTypeConfiguration<InsurancePolicy>
{
    public void Configure(EntityTypeBuilder<InsurancePolicy> builder)
    {
        builder.ToTable("INSURANCE_POLICY");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("POLICY_ID");

        builder.Property(p => p.Provider)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("PROVIDER");

        builder.Property(p => p.StartDate)
            .IsRequired()
            .HasColumnName("START_DATE");

        builder.Property(p => p.EndDate)
            .IsRequired()
            .HasColumnName("END_DATE");

        builder.Property(p => p.TotalCoverageAmount)
            .HasColumnType("NUMBER(14,2)")
            .HasColumnName("TOTAL_COVERAGE_AMOUNT");

        builder.HasIndex(p => p.Provider);
        builder.HasIndex(p => p.EndDate);
    }
}
