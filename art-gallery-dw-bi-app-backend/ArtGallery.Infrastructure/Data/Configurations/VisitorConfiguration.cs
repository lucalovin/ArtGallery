﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Visitor entity.
/// </summary>
public class VisitorConfiguration : IEntityTypeConfiguration<Visitor>
{
    public void Configure(EntityTypeBuilder<Visitor> builder)
    {
        builder.ToTable("VISITOR");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("VISITOR_ID");

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("NAME");

        builder.Property(v => v.Email)
            .HasMaxLength(128)
            .HasColumnName("EMAIL");

        builder.Property(v => v.Phone)
            .HasMaxLength(32)
            .HasColumnName("PHONE");

        builder.Property(v => v.MembershipType)
            .HasMaxLength(32)
            .HasColumnName("MEMBERSHIP_TYPE");

        builder.Property(v => v.JoinDate)
            .HasColumnName("JOIN_DATE");

        builder.HasIndex(v => v.Email);
    }
}
