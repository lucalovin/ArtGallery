using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// Provides seed data for the database.
/// Note: The OLTP database (ART_GALLERY_OLTP) should be populated using the create_oltp.sql script.
/// This seed data is primarily for development/testing purposes.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Gets seed artists.
    /// </summary>
    public static List<Artist> GetArtists() => new()
    {
        new Artist { Id = 1, Name = "Pablo Picasso", Nationality = "Spanish", BirthYear = 1881, DeathYear = 1973 },
        new Artist { Id = 2, Name = "Vincent van Gogh", Nationality = "Dutch", BirthYear = 1853, DeathYear = 1890 },
        new Artist { Id = 3, Name = "Claude Monet", Nationality = "French", BirthYear = 1840, DeathYear = 1926 },
        new Artist { Id = 4, Name = "Salvador Dali", Nationality = "Spanish", BirthYear = 1904, DeathYear = 1989 }
    };

    /// <summary>
    /// Gets seed collections.
    /// </summary>
    public static List<Collection> GetCollections() => new()
    {
        new Collection { Id = 1, Name = "Modern Masters", Description = "Key works of modern art", CreatedDate = new DateTime(2020, 1, 15) },
        new Collection { Id = 2, Name = "Impressionist Highlights", Description = "Selected impressionist paintings", CreatedDate = new DateTime(2021, 3, 10) },
        new Collection { Id = 3, Name = "Surreal Visions", Description = "Surrealist paintings and objects", CreatedDate = new DateTime(2022, 5, 20) },
        new Collection { Id = 4, Name = "Permanent Collection", Description = "Core museum holdings", CreatedDate = new DateTime(2019, 9, 1) }
    };

    /// <summary>
    /// Gets seed locations.
    /// </summary>
    public static List<Location> GetLocations() => new()
    {
        new Location { Id = 1, Name = "Main Hall", GalleryRoom = "R1", Type = "Exhibit", Capacity = 50 },
        new Location { Id = 2, Name = "East Wing", GalleryRoom = "R2", Type = "Exhibit", Capacity = 40 },
        new Location { Id = 3, Name = "Storage A", GalleryRoom = "S1", Type = "Storage", Capacity = 200 },
        new Location { Id = 4, Name = "Storage B", GalleryRoom = "S2", Type = "Storage", Capacity = 150 }
    };

    /// <summary>
    /// Gets seed exhibitors.
    /// </summary>
    public static List<Exhibitor> GetExhibitors() => new()
    {
        new Exhibitor { Id = 1, Name = "Louvre Museum", Address = "Rue de Rivoli", City = "Paris", ContactInfo = "contact@louvre.fr" },
        new Exhibitor { Id = 2, Name = "MoMA", Address = "11 W 53rd St", City = "New York", ContactInfo = "info@moma.org" },
        new Exhibitor { Id = 3, Name = "Tate Modern", Address = "Bankside", City = "London", ContactInfo = "contact@tate.org.uk" },
        new Exhibitor { Id = 4, Name = "Reina Sofia", Address = "Calle de Santa Isabel", City = "Madrid", ContactInfo = "info@museoreinasofia.es" }
    };

    /// <summary>
    /// Gets seed insurance policies.
    /// </summary>
    public static List<InsurancePolicy> GetInsurancePolicies() => new()
    {
        new InsurancePolicy { Id = 1, Provider = "Global Insurance", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2025, 1, 1), TotalCoverageAmount = 3000000 },
        new InsurancePolicy { Id = 2, Provider = "ArtSecure", StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2026, 1, 1), TotalCoverageAmount = 1500000 },
        new InsurancePolicy { Id = 3, Provider = "FineArt Shield", StartDate = new DateTime(2023, 6, 1), EndDate = new DateTime(2027, 6, 1), TotalCoverageAmount = 2000000 },
        new InsurancePolicy { Id = 4, Provider = "Museum Protect", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2028, 1, 1), TotalCoverageAmount = 2500000 }
    };

    /// <summary>
    /// Gets seed artworks.
    /// </summary>
    public static List<Artwork> GetArtworks() => new()
    {
        new Artwork { Id = 1, Title = "Les Demoiselles d'Avignon", ArtistId = 1, YearCreated = 1907, Medium = "Oil on Canvas", CollectionId = 1, LocationId = 1, EstimatedValue = 1000000 },
        new Artwork { Id = 2, Title = "Guernica", ArtistId = 1, YearCreated = 1937, Medium = "Oil on Canvas", CollectionId = 1, LocationId = 2, EstimatedValue = 1500000 },
        new Artwork { Id = 3, Title = "Starry Night", ArtistId = 2, YearCreated = 1889, Medium = "Oil on Canvas", CollectionId = 2, LocationId = 1, EstimatedValue = 1200000 },
        new Artwork { Id = 4, Title = "Sunflowers", ArtistId = 2, YearCreated = 1888, Medium = "Oil on Canvas", CollectionId = 2, LocationId = 2, EstimatedValue = 800000 },
        new Artwork { Id = 5, Title = "Water Lilies", ArtistId = 3, YearCreated = 1916, Medium = "Oil on Canvas", CollectionId = 2, LocationId = 1, EstimatedValue = 900000 },
        new Artwork { Id = 6, Title = "Impression, Sunrise", ArtistId = 3, YearCreated = 1872, Medium = "Oil on Canvas", CollectionId = 2, LocationId = 2, EstimatedValue = 700000 },
        new Artwork { Id = 7, Title = "The Persistence of Memory", ArtistId = 4, YearCreated = 1931, Medium = "Oil on Canvas", CollectionId = 3, LocationId = 1, EstimatedValue = 600000 },
        new Artwork { Id = 8, Title = "Swans Reflecting Elephants", ArtistId = 4, YearCreated = 1937, Medium = "Oil on Canvas", CollectionId = 3, LocationId = 2, EstimatedValue = 550000 }
    };

    /// <summary>
    /// Gets seed exhibitions.
    /// </summary>
    public static List<Exhibition> GetExhibitions() => new()
    {
        new Exhibition { Id = 1, Title = "Modern Icons", StartDate = new DateTime(2024, 1, 10), EndDate = new DateTime(2024, 3, 10), ExhibitorId = 1, Description = "Masterpieces of modern art" },
        new Exhibition { Id = 2, Title = "Impressionist Seasons", StartDate = new DateTime(2024, 4, 1), EndDate = new DateTime(2024, 6, 30), ExhibitorId = 2, Description = "Key impressionist works" },
        new Exhibition { Id = 3, Title = "Surreal Moments", StartDate = new DateTime(2024, 7, 1), EndDate = new DateTime(2024, 9, 15), ExhibitorId = 3, Description = "Surrealist paintings and sculptures" },
        new Exhibition { Id = 4, Title = "Van Gogh Focus", StartDate = new DateTime(2024, 10, 1), EndDate = new DateTime(2024, 12, 31), ExhibitorId = 4, Description = "A selection of Van Gogh's works" }
    };

    /// <summary>
    /// Gets seed visitors.
    /// </summary>
    public static List<Visitor> GetVisitors() => new()
    {
        new Visitor { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Phone = "+40111111111", MembershipType = "Standard", JoinDate = new DateTime(2023, 1, 10) },
        new Visitor { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Phone = "+40111111112", MembershipType = "VIP", JoinDate = new DateTime(2023, 2, 15) },
        new Visitor { Id = 3, Name = "Carla Pop", Email = "carla@example.com", Phone = "+40111111113", MembershipType = "Student", JoinDate = new DateTime(2023, 3, 20) },
        new Visitor { Id = 4, Name = "Dan Ionescu", Email = "dan@example.com", Phone = "+40111111114", MembershipType = "Standard", JoinDate = new DateTime(2023, 4, 5) }
    };

    /// <summary>
    /// Gets seed staff members.
    /// </summary>
    public static List<Staff> GetStaff() => new()
    {
        new Staff { Id = 1, Name = "Elena Curator", Role = "Curator", HireDate = new DateTime(2020, 1, 1), CertificationLevel = "Level 2" },
        new Staff { Id = 2, Name = "Mihai Restorer", Role = "Restorer", HireDate = new DateTime(2021, 5, 10), CertificationLevel = "Level 3" },
        new Staff { Id = 3, Name = "Ioana Registrar", Role = "Registrar", HireDate = new DateTime(2022, 3, 15), CertificationLevel = "Level 1" },
        new Staff { Id = 4, Name = "Andrei Manager", Role = "Manager", HireDate = new DateTime(2019, 9, 1), CertificationLevel = "Level 3" }
    };

    /// <summary>
    /// Gets seed loans.
    /// </summary>
    public static List<Loan> GetLoans() => new()
    {
        new Loan { Id = 1, ArtworkId = 2, ExhibitorId = 2, StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 3, 1), Conditions = "Standard insurance" },
        new Loan { Id = 2, ArtworkId = 4, ExhibitorId = 1, StartDate = new DateTime(2023, 2, 15), EndDate = new DateTime(2023, 4, 15), Conditions = "Climate control required" },
        new Loan { Id = 3, ArtworkId = 6, ExhibitorId = 3, StartDate = new DateTime(2023, 5, 1), EndDate = new DateTime(2023, 7, 1), Conditions = "Framed transport" },
        new Loan { Id = 4, ArtworkId = 8, ExhibitorId = 4, StartDate = new DateTime(2023, 8, 1), EndDate = new DateTime(2023, 10, 1), Conditions = "Handle with care" }
    };

    /// <summary>
    /// Gets seed insurance records.
    /// </summary>
    public static List<Insurance> GetInsurances() => new()
    {
        new Insurance { Id = 1, ArtworkId = 1, PolicyId = 1, InsuredAmount = 900000 },
        new Insurance { Id = 2, ArtworkId = 2, PolicyId = 1, InsuredAmount = 1200000 },
        new Insurance { Id = 3, ArtworkId = 3, PolicyId = 2, InsuredAmount = 1000000 },
        new Insurance { Id = 4, ArtworkId = 4, PolicyId = 2, InsuredAmount = 700000 },
        new Insurance { Id = 5, ArtworkId = 5, PolicyId = 3, InsuredAmount = 800000 },
        new Insurance { Id = 6, ArtworkId = 6, PolicyId = 3, InsuredAmount = 600000 },
        new Insurance { Id = 7, ArtworkId = 7, PolicyId = 4, InsuredAmount = 500000 },
        new Insurance { Id = 8, ArtworkId = 8, PolicyId = 4, InsuredAmount = 450000 }
    };

    /// <summary>
    /// Gets seed restorations.
    /// </summary>
    public static List<Restoration> GetRestorations() => new()
    {
        new Restoration { Id = 1, ArtworkId = 1, StaffId = 2, StartDate = new DateTime(2022, 1, 10), EndDate = new DateTime(2022, 2, 10), Description = "Varnish cleaning" },
        new Restoration { Id = 2, ArtworkId = 3, StaffId = 2, StartDate = new DateTime(2021, 3, 1), EndDate = new DateTime(2021, 4, 1), Description = "Canvas stabilization" },
        new Restoration { Id = 3, ArtworkId = 5, StaffId = 2, StartDate = new DateTime(2020, 9, 15), EndDate = new DateTime(2020, 10, 15), Description = "Color retouching" },
        new Restoration { Id = 4, ArtworkId = 7, StaffId = 2, StartDate = new DateTime(2023, 1, 5), EndDate = new DateTime(2023, 2, 5), Description = "Frame repair" }
    };

    /// <summary>
    /// Gets seed exhibition-artwork relationships.
    /// </summary>
    public static List<ExhibitionArtwork> GetExhibitionArtworks() => new()
    {
        new ExhibitionArtwork { ExhibitionId = 1, ArtworkId = 1, PositionInGallery = "Room 1 - Center", FeaturedStatus = "Featured" },
        new ExhibitionArtwork { ExhibitionId = 1, ArtworkId = 2, PositionInGallery = "Room 1 - Left", FeaturedStatus = "Regular" },
        new ExhibitionArtwork { ExhibitionId = 2, ArtworkId = 3, PositionInGallery = "Room 2 - Center", FeaturedStatus = "Featured" },
        new ExhibitionArtwork { ExhibitionId = 2, ArtworkId = 4, PositionInGallery = "Room 2 - Right", FeaturedStatus = "Regular" },
        new ExhibitionArtwork { ExhibitionId = 2, ArtworkId = 5, PositionInGallery = "Room 3 - Left", FeaturedStatus = "Regular" },
        new ExhibitionArtwork { ExhibitionId = 3, ArtworkId = 7, PositionInGallery = "Room 4 - Center", FeaturedStatus = "Featured" },
        new ExhibitionArtwork { ExhibitionId = 3, ArtworkId = 8, PositionInGallery = "Room 4 - Right", FeaturedStatus = "Regular" },
        new ExhibitionArtwork { ExhibitionId = 4, ArtworkId = 3, PositionInGallery = "Room 5 - Center", FeaturedStatus = "Featured" }
    };

    /// <summary>
    /// Gets seed ETL sync records.
    /// </summary>
    public static List<EtlSync> GetEtlSyncs() => new()
    {
        new EtlSync
        {
            Id = 1,
            SyncDate = DateTime.UtcNow.AddDays(-7),
            Status = "Completed",
            RecordsProcessed = 100,
            RecordsFailed = 0,
            Duration = 5000,
            SourceSystem = "OLTP",
            TargetSystem = "DW",
            SyncType = "Full"
        },
        new EtlSync
        {
            Id = 2,
            SyncDate = DateTime.UtcNow.AddDays(-1),
            Status = "Completed",
            RecordsProcessed = 25,
            RecordsFailed = 2,
            Duration = 1200,
            SourceSystem = "OLTP",
            TargetSystem = "DW",
            SyncType = "Incremental",
            ErrorMessage = "2 records failed validation"
        }
    };
}
