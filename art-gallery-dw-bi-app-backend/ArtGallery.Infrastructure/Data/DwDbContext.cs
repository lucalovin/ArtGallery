using Microsoft.EntityFrameworkCore;
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
            entity.Property(e => e.FullDate).HasColumnName("FULL_DATE");
            entity.Property(e => e.DayOfWeek).HasColumnName("DAY_OF_WEEK");
            entity.Property(e => e.DayName).HasColumnName("DAY_NAME").HasMaxLength(20);
            entity.Property(e => e.DayOfMonth).HasColumnName("DAY_OF_MONTH");
            entity.Property(e => e.DayOfYear).HasColumnName("DAY_OF_YEAR");
            entity.Property(e => e.WeekOfYear).HasColumnName("WEEK_OF_YEAR");
            entity.Property(e => e.MonthNumber).HasColumnName("MONTH_NUMBER");
            entity.Property(e => e.MonthName).HasColumnName("MONTH_NAME").HasMaxLength(20);
            entity.Property(e => e.Quarter).HasColumnName("QUARTER");
            entity.Property(e => e.Year).HasColumnName("YEAR");
            entity.Property(e => e.FiscalYear).HasColumnName("FISCAL_YEAR");
            entity.Property(e => e.FiscalQuarter).HasColumnName("FISCAL_QUARTER");
            entity.Property(e => e.IsWeekend).HasColumnName("IS_WEEKEND");
            entity.Property(e => e.IsHoliday).HasColumnName("IS_HOLIDAY");
            entity.Property(e => e.HolidayName).HasColumnName("HOLIDAY_NAME").HasMaxLength(100);
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
            entity.Property(e => e.ArtworkKey).HasColumnName("ARTWORK_KEY");
            entity.Property(e => e.ArtistKey).HasColumnName("ARTIST_KEY");
            entity.Property(e => e.VisitorKey).HasColumnName("VISITOR_KEY");
            entity.Property(e => e.StaffKey).HasColumnName("STAFF_KEY");
            entity.Property(e => e.LocationKey).HasColumnName("LOCATION_KEY");
            entity.Property(e => e.InsuranceKey).HasColumnName("INSURANCE_KEY");

            // Measures
            entity.Property(e => e.VisitorCount).HasColumnName("VISITOR_COUNT");
            entity.Property(e => e.TicketRevenue).HasColumnName("TICKET_REVENUE").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.MerchandiseRevenue).HasColumnName("MERCHANDISE_REVENUE").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.TotalRevenue).HasColumnName("TOTAL_REVENUE").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.VisitDurationMinutes).HasColumnName("VISIT_DURATION_MINUTES");
            entity.Property(e => e.InsuranceValue).HasColumnName("INSURANCE_VALUE").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.OperatingCost).HasColumnName("OPERATING_COST").HasColumnType("NUMBER(18,2)");
            entity.Property(e => e.ArtworkCount).HasColumnName("ARTWORK_COUNT");
            entity.Property(e => e.PartitionYear).HasColumnName("PARTITION_YEAR");
            entity.Property(e => e.EtlLoadDate).HasColumnName("ETL_LOAD_DATE");

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

            entity.HasOne(e => e.Visitor)
                  .WithMany()
                  .HasForeignKey(e => e.VisitorKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Staff)
                  .WithMany()
                  .HasForeignKey(e => e.StaffKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Location)
                  .WithMany()
                  .HasForeignKey(e => e.LocationKey)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Insurance)
                  .WithMany()
                  .HasForeignKey(e => e.InsuranceKey)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.DateKey).HasDatabaseName("IX_FACT_DATE_KEY");
            entity.HasIndex(e => e.ExhibitionKey).HasDatabaseName("IX_FACT_EXHIBITION_KEY");
            entity.HasIndex(e => e.ArtworkKey).HasDatabaseName("IX_FACT_ARTWORK_KEY");
            entity.HasIndex(e => e.PartitionYear).HasDatabaseName("IX_FACT_PARTITION_YEAR");
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
