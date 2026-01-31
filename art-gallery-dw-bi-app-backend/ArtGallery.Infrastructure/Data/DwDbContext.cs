﻿using Microsoft.EntityFrameworkCore;
using ArtGallery.Domain.Entities.DW;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the Art Gallery Data Warehouse.
/// Configured to work with Oracle Database DW schema.
/// </summary>
public class DwDbContext : DbContext
{
    public DwDbContext(DbContextOptions<DwDbContext> options) : base(options)
    {
    }

    // Dimension Tables
    public DbSet<DimArtist> DimArtists => Set<DimArtist>();
    public DbSet<DimArtwork> DimArtworks => Set<DimArtwork>();
    public DbSet<DimExhibition> DimExhibitions => Set<DimExhibition>();
    public DbSet<DimDate> DimDates => Set<DimDate>();
    public DbSet<DimVisitor> DimVisitors => Set<DimVisitor>();
    public DbSet<DimStaff> DimStaff => Set<DimStaff>();
    public DbSet<DimInsurance> DimInsurances => Set<DimInsurance>();
    public DbSet<DimLocation> DimLocations => Set<DimLocation>();

    // Fact Table
    public DbSet<FactExhibitionActivity> FactExhibitionActivities => Set<FactExhibitionActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for Oracle DW
        modelBuilder.HasDefaultSchema("ART_GALLERY_DW");

        // Configure dimension tables
        ConfigureDimArtist(modelBuilder);
        ConfigureDimArtwork(modelBuilder);
        ConfigureDimExhibition(modelBuilder);
        ConfigureDimDate(modelBuilder);
        ConfigureDimVisitor(modelBuilder);
        ConfigureDimStaff(modelBuilder);
        ConfigureDimInsurance(modelBuilder);
        ConfigureDimLocation(modelBuilder);
        ConfigureFactExhibitionActivity(modelBuilder);

        // Configure Oracle-specific conventions
        ConfigureOracleConventions(modelBuilder);
    }

    private void ConfigureDimArtist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimArtist>(entity =>
        {
            entity.ToTable("DIM_ARTIST");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ARTIST_KEY");
            entity.Property(e => e.ArtistNk).HasColumnName("ARTIST_NK").IsRequired();
            entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            entity.Property(e => e.FirstName).HasColumnName("FIRST_NAME").HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("LAST_NAME").HasMaxLength(100);
            entity.Property(e => e.FullName).HasColumnName("FULL_NAME").HasMaxLength(200);
            entity.Property(e => e.Nationality).HasColumnName("NATIONALITY").HasMaxLength(100);
            entity.Property(e => e.BirthYear).HasColumnName("BIRTH_YEAR");
            entity.Property(e => e.DeathYear).HasColumnName("DEATH_YEAR");
            entity.Property(e => e.ArtMovement).HasColumnName("ART_MOVEMENT").HasMaxLength(100);
            entity.Property(e => e.EffectiveStartDate).HasColumnName("EFFECTIVE_START_DATE");
            entity.Property(e => e.EffectiveEndDate).HasColumnName("EFFECTIVE_END_DATE");
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

            entity.HasIndex(e => new { e.ArtistNk, e.IsCurrent }).HasDatabaseName("IX_DIM_ARTIST_NK_CURRENT");
        });
    }

    private void ConfigureDimArtwork(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimArtwork>(entity =>
        {
            entity.ToTable("DIM_ARTWORK");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ARTWORK_KEY");
            entity.Property(e => e.ArtworkNk).HasColumnName("ARTWORK_NK").IsRequired();
            entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(500);
            entity.Property(e => e.ArtistKey).HasColumnName("ARTIST_KEY");
            entity.Property(e => e.CreationYear).HasColumnName("CREATION_YEAR");
            entity.Property(e => e.Medium).HasColumnName("MEDIUM").HasMaxLength(200);
            entity.Property(e => e.Dimensions).HasColumnName("DIMENSIONS").HasMaxLength(100);
            entity.Property(e => e.CollectionType).HasColumnName("COLLECTION_TYPE").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(50);
            entity.Property(e => e.EstimatedValue).HasColumnName("ESTIMATED_VALUE").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.AcquisitionDate).HasColumnName("ACQUISITION_DATE");
            entity.Property(e => e.EffectiveStartDate).HasColumnName("EFFECTIVE_START_DATE");
            entity.Property(e => e.EffectiveEndDate).HasColumnName("EFFECTIVE_END_DATE");
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");

            entity.HasOne(e => e.Artist)
                  .WithMany()
                  .HasForeignKey(e => e.ArtistKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ArtworkNk, e.IsCurrent }).HasDatabaseName("IX_DIM_ARTWORK_NK_CURRENT");
        });
    }

    private void ConfigureDimExhibition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimExhibition>(entity =>
        {
            entity.ToTable("DIM_EXHIBITION");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("EXHIBITION_KEY");
            entity.Property(e => e.ExhibitionNk).HasColumnName("EXHIBITION_NK").IsRequired();
            entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(4000);
            entity.Property(e => e.StartDate).HasColumnName("START_DATE");
            entity.Property(e => e.EndDate).HasColumnName("END_DATE");
            entity.Property(e => e.DurationDays).HasColumnName("DURATION_DAYS");
            entity.Property(e => e.Location).HasColumnName("LOCATION").HasMaxLength(200);
            entity.Property(e => e.ExhibitionType).HasColumnName("EXHIBITION_TYPE").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(50);
            entity.Property(e => e.TicketPrice).HasColumnName("TICKET_PRICE").HasColumnType("NUMBER(10,2)");
            entity.Property(e => e.Budget).HasColumnName("BUDGET").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        });
    }

    private void ConfigureDimDate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimDate>(entity =>
        {
            entity.ToTable("DIM_DATE");
            entity.HasKey(e => e.DateKey);
            entity.Property(e => e.DateKey).HasColumnName("DATE_KEY").ValueGeneratedNever();
            entity.Property(e => e.CalendarDate).HasColumnName("CALENDAR_DATE").IsRequired();
            entity.Property(e => e.CalendarYear).HasColumnName("CALENDAR_YEAR").IsRequired();
            entity.Property(e => e.CalendarMonth).HasColumnName("CALENDAR_MONTH").IsRequired();
            entity.Property(e => e.CalendarDay).HasColumnName("CALENDAR_DAY").IsRequired();
            entity.Property(e => e.MonthName).HasColumnName("MONTH_NAME").HasMaxLength(20);
            entity.Property(e => e.Quarter).HasColumnName("QUARTER");
            entity.Property(e => e.IsWeekend).HasColumnName("IS_WEEKEND").HasMaxLength(1);
        });
    }

    private void ConfigureDimVisitor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimVisitor>(entity =>
        {
            entity.ToTable("DIM_VISITOR");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("VISITOR_KEY");
            entity.Property(e => e.VisitorNk).HasColumnName("VISITOR_NK").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("FULL_NAME").HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(255);
            entity.Property(e => e.MembershipType).HasColumnName("MEMBERSHIP_TYPE").HasMaxLength(50);
            entity.Property(e => e.AgeGroup).HasColumnName("AGE_GROUP").HasMaxLength(20);
            entity.Property(e => e.Country).HasColumnName("COUNTRY").HasMaxLength(100);
            entity.Property(e => e.City).HasColumnName("CITY").HasMaxLength(100);
            entity.Property(e => e.FirstVisitDate).HasColumnName("FIRST_VISIT_DATE");
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        });
    }

    private void ConfigureDimStaff(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimStaff>(entity =>
        {
            entity.ToTable("DIM_STAFF");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("STAFF_KEY");
            entity.Property(e => e.StaffNk).HasColumnName("STAFF_NK").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("FULL_NAME").HasMaxLength(200);
            entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(255);
            entity.Property(e => e.JobTitle).HasColumnName("JOB_TITLE").HasMaxLength(100);
            entity.Property(e => e.Department).HasColumnName("DEPARTMENT").HasMaxLength(100);
            entity.Property(e => e.EmploymentStatus).HasColumnName("EMPLOYMENT_STATUS").HasMaxLength(50);
            entity.Property(e => e.HireDate).HasColumnName("HIRE_DATE");
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        });
    }

    private void ConfigureDimInsurance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimInsurance>(entity =>
        {
            entity.ToTable("DIM_INSURANCE");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("INSURANCE_KEY");
            entity.Property(e => e.InsuranceNk).HasColumnName("INSURANCE_NK").IsRequired();
            entity.Property(e => e.PolicyNumber).HasColumnName("POLICY_NUMBER").HasMaxLength(50);
            entity.Property(e => e.Provider).HasColumnName("PROVIDER").HasMaxLength(200);
            entity.Property(e => e.CoverageType).HasColumnName("COVERAGE_TYPE").HasMaxLength(100);
            entity.Property(e => e.CoverageAmount).HasColumnName("COVERAGE_AMOUNT").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.Premium).HasColumnName("PREMIUM").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.StartDate).HasColumnName("START_DATE");
            entity.Property(e => e.EndDate).HasColumnName("END_DATE");
            entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(50);
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        });
    }

    private void ConfigureDimLocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimLocation>(entity =>
        {
            entity.ToTable("DIM_LOCATION");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("LOCATION_KEY");
            entity.Property(e => e.LocationNk).HasColumnName("LOCATION_NK").IsRequired();
            entity.Property(e => e.LocationName).HasColumnName("LOCATION_NAME").HasMaxLength(200);
            entity.Property(e => e.Building).HasColumnName("BUILDING").HasMaxLength(100);
            entity.Property(e => e.Floor).HasColumnName("FLOOR").HasMaxLength(50);
            entity.Property(e => e.Room).HasColumnName("ROOM").HasMaxLength(100);
            entity.Property(e => e.LocationType).HasColumnName("LOCATION_TYPE").HasMaxLength(50);
            entity.Property(e => e.Capacity).HasColumnName("CAPACITY");
            entity.Property(e => e.SquareFootage).HasColumnName("SQUARE_FOOTAGE").HasColumnType("NUMBER(10,2)");
            entity.Property(e => e.IsClimateControlled).HasColumnName("IS_CLIMATE_CONTROLLED");
            entity.Property(e => e.IsPublicAccessible).HasColumnName("IS_PUBLIC_ACCESSIBLE");
            entity.Property(e => e.IsCurrent).HasColumnName("IS_CURRENT").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(e => e.UpdatedAt).HasColumnName("UPDATED_AT");
        });
    }

    private void ConfigureFactExhibitionActivity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FactExhibitionActivity>(entity =>
        {
            entity.ToTable("FACT_EXHIBITION_ACTIVITY");
            entity.HasKey(e => e.FactKey);
            entity.Property(e => e.FactKey).HasColumnName("FACT_KEY");
            entity.Property(e => e.DateKey).HasColumnName("DATE_KEY");
            entity.Property(e => e.ExhibitionKey).HasColumnName("EXHIBITION_KEY");
            entity.Property(e => e.ExhibitorKey).HasColumnName("EXHIBITOR_KEY");
            entity.Property(e => e.ArtworkKey).HasColumnName("ARTWORK_KEY");
            entity.Property(e => e.ArtistKey).HasColumnName("ARTIST_KEY");
            entity.Property(e => e.CollectionKey).HasColumnName("COLLECTION_KEY");
            entity.Property(e => e.LocationKey).HasColumnName("LOCATION_KEY");
            entity.Property(e => e.PolicyKey).HasColumnName("POLICY_KEY");

            // Measures
            entity.Property(e => e.EstimatedValue).HasColumnName("ESTIMATED_VALUE").HasColumnType("NUMBER(12,2)");
            entity.Property(e => e.InsuredAmount).HasColumnName("INSURED_AMOUNT").HasColumnType("NUMBER(14,2)");
            entity.Property(e => e.LoanFlag).HasColumnName("LOAN_FLAG");
            entity.Property(e => e.RestorationCount).HasColumnName("RESTORATION_COUNT");
            entity.Property(e => e.ReviewCount).HasColumnName("REVIEW_COUNT");
            entity.Property(e => e.AvgRating).HasColumnName("AVG_RATING").HasColumnType("NUMBER(5,2)");

            // Foreign keys
            entity.HasOne(e => e.Date)
                  .WithMany()
                  .HasForeignKey(e => e.DateKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Exhibition)
                  .WithMany()
                  .HasForeignKey(e => e.ExhibitionKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Artwork)
                  .WithMany()
                  .HasForeignKey(e => e.ArtworkKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Artist)
                  .WithMany()
                  .HasForeignKey(e => e.ArtistKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Location)
                  .WithMany()
                  .HasForeignKey(e => e.LocationKey)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureOracleConventions(ModelBuilder modelBuilder)
    {
        // Configure all string properties to use VARCHAR2 by default
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // Map decimal to NUMBER if not already specified
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    if (property.GetColumnType() == null)
                    {
                        property.SetColumnType("NUMBER(18,2)");
                    }
                }

                // Map DateTime to TIMESTAMP
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("TIMESTAMP");
                }
            }
        }
    }
}
