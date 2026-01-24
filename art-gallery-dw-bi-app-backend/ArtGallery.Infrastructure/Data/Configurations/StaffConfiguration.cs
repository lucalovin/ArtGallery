using Microsoft.EntityFrameworkCore;
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
        builder.ToTable("Staff");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Phone)
            .HasMaxLength(20);

        builder.Property(s => s.Department)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Position)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Salary)
            .HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Bio)
            .HasMaxLength(2000);

        builder.Ignore(s => s.FullName);

        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.Department);
        builder.HasIndex(s => s.Status);
    }
}
