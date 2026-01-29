﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Staff entity.
/// </summary>
public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("STAFF");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("STAFF_ID");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("NAME");

        builder.Property(s => s.Role)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnName("ROLE");

        builder.Property(s => s.HireDate)
            .IsRequired()
            .HasColumnName("HIRE_DATE");

        builder.Property(s => s.CertificationLevel)
            .HasMaxLength(32)
            .HasColumnName("CERTIFICATION_LEVEL");

        builder.HasIndex(s => s.Role);
    }
}
