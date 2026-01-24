using Microsoft.EntityFrameworkCore;
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
        builder.ToTable("Visitors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.Phone)
            .HasMaxLength(20);

        builder.Property(v => v.MembershipType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("None");

        builder.Property(v => v.Address)
            .HasMaxLength(255);

        builder.Property(v => v.City)
            .HasMaxLength(100);

        builder.Property(v => v.Country)
            .HasMaxLength(100);

        builder.Property(v => v.Notes)
            .HasMaxLength(1000);

        builder.Ignore(v => v.FullName);

        builder.HasIndex(v => v.Email).IsUnique();
        builder.HasIndex(v => v.MembershipType);
    }
}
