using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Artist entity.
/// </summary>
public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("ARTIST");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("ARTIST_ID");

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnName("NAME");

        builder.Property(a => a.Nationality)
            .HasMaxLength(64)
            .HasColumnName("NATIONALITY");

        builder.Property(a => a.BirthYear)
            .HasColumnName("BIRTH_YEAR");

        builder.Property(a => a.DeathYear)
            .HasColumnName("DEATH_YEAR");

        builder.HasIndex(a => a.Name);
    }
}
