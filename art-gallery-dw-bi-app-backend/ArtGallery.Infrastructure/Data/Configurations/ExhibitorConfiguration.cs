using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Exhibitor entity.
/// </summary>
public class ExhibitorConfiguration : IEntityTypeConfiguration<Exhibitor>
{
    public void Configure(EntityTypeBuilder<Exhibitor> builder)
    {
        builder.ToTable("EXHIBITOR");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("EXHIBITOR_ID");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("NAME");

        builder.Property(e => e.Address)
            .HasMaxLength(256)
            .HasColumnName("ADDRESS");

        builder.Property(e => e.City)
            .HasMaxLength(64)
            .HasColumnName("CITY");

        builder.Property(e => e.ContactInfo)
            .HasMaxLength(256)
            .HasColumnName("CONTACT_INFO");

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.City);
    }
}
