using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArtGallery.Domain.Entities.DW;

namespace ArtGallery.Infrastructure.Data.Configurations.DW;

/// <summary>
/// EF Core configuration for DimDate entity in Data Warehouse.
/// </summary>
public class DimDateConfiguration : IEntityTypeConfiguration<DimDate>
{
    public void Configure(EntityTypeBuilder<DimDate> builder)
    {
        builder.ToTable("DIM_DATE", "ART_GALLERY_DW");

        builder.HasKey(d => d.DateKey);
        builder.Property(d => d.DateKey)
            .HasColumnName("DATE_KEY");

        builder.Property(d => d.CalendarDate)
            .IsRequired()
            .HasColumnName("CALENDAR_DATE");

        builder.Property(d => d.CalendarYear)
            .IsRequired()
            .HasColumnName("CALENDAR_YEAR");

        builder.Property(d => d.CalendarMonth)
            .IsRequired()
            .HasColumnName("CALENDAR_MONTH");

        builder.Property(d => d.CalendarDay)
            .IsRequired()
            .HasColumnName("CALENDAR_DAY");

        builder.Property(d => d.MonthName)
            .HasMaxLength(20)
            .HasColumnName("MONTH_NAME");

        builder.Property(d => d.Quarter)
            .HasColumnName("QUARTER");

        builder.Property(d => d.IsWeekend)
            .HasMaxLength(1)
            .HasColumnName("IS_WEEKEND");
    }
}
