using ArtGallery.Domain.Entities;

namespace ArtGallery.Infrastructure.Data;

/// <summary>
/// Provides seed data for the database.
/// </summary>
public static class SeedData
{
    public static List<Artwork> GetArtworks() => new()
    {
        new Artwork
        {
            Id = 1,
            Title = "Water Lilies",
            Artist = "Claude Monet",
            Year = 1906,
            Medium = "Oil on canvas",
            Dimensions = "89.9 × 94.1 cm",
            Description = "Part of Monet's famous Water Lilies series depicting his flower garden at Giverny.",
            ImageUrl = "/images/artworks/water-lilies.jpg",
            Collection = "Permanent",
            Status = "OnDisplay",
            EstimatedValue = 50000000,
            Location = "Gallery A",
            AcquisitionDate = new DateTime(2015, 3, 15),
            AcquisitionMethod = "Purchase",
            Provenance = "Private collection, Paris",
            Condition = "Excellent",
            Tags = new List<string> { "Impressionism", "Landscape", "French" }
        },
        new Artwork
        {
            Id = 2,
            Title = "The Starry Night",
            Artist = "Vincent van Gogh",
            Year = 1889,
            Medium = "Oil on canvas",
            Dimensions = "73.7 × 92.1 cm",
            Description = "Van Gogh's iconic depiction of a swirling night sky over a village.",
            ImageUrl = "/images/artworks/starry-night.jpg",
            Collection = "Permanent",
            Status = "OnDisplay",
            EstimatedValue = 100000000,
            Location = "Gallery B",
            AcquisitionDate = new DateTime(2010, 6, 20),
            AcquisitionMethod = "Donation",
            Provenance = "Museum of Modern Art",
            Condition = "Good",
            Tags = new List<string> { "Post-Impressionism", "Landscape", "Dutch" }
        },
        new Artwork
        {
            Id = 3,
            Title = "Girl with a Pearl Earring",
            Artist = "Johannes Vermeer",
            Year = 1665,
            Medium = "Oil on canvas",
            Dimensions = "44.5 × 39 cm",
            Description = "Known as the 'Mona Lisa of the North'.",
            ImageUrl = "/images/artworks/girl-pearl-earring.jpg",
            Collection = "Permanent",
            Status = "Available",
            EstimatedValue = 30000000,
            Location = "Storage A",
            AcquisitionDate = new DateTime(2018, 1, 10),
            AcquisitionMethod = "Purchase",
            Provenance = "Mauritshuis, The Hague",
            Condition = "Excellent",
            Tags = new List<string> { "Baroque", "Portrait", "Dutch" }
        },
        new Artwork
        {
            Id = 4,
            Title = "The Persistence of Memory",
            Artist = "Salvador Dalí",
            Year = 1931,
            Medium = "Oil on canvas",
            Dimensions = "24 × 33 cm",
            Description = "Famous surrealist painting with melting clocks.",
            ImageUrl = "/images/artworks/persistence-memory.jpg",
            Collection = "Permanent",
            Status = "OnLoan",
            EstimatedValue = 45000000,
            Location = "On Loan",
            AcquisitionDate = new DateTime(2012, 9, 5),
            AcquisitionMethod = "Purchase",
            Provenance = "Private collector, Spain",
            Condition = "Good",
            Tags = new List<string> { "Surrealism", "Modern", "Spanish" }
        },
        new Artwork
        {
            Id = 5,
            Title = "The Birth of Venus",
            Artist = "Sandro Botticelli",
            Year = 1485,
            Medium = "Tempera on canvas",
            Dimensions = "172.5 × 278.9 cm",
            Description = "Depicts the goddess Venus emerging from the sea.",
            ImageUrl = "/images/artworks/birth-venus.jpg",
            Collection = "Permanent",
            Status = "UnderRestoration",
            EstimatedValue = 80000000,
            Location = "Conservation Lab",
            AcquisitionDate = new DateTime(2008, 4, 22),
            AcquisitionMethod = "Bequest",
            Provenance = "Uffizi Gallery, Florence",
            Condition = "Fair",
            Tags = new List<string> { "Renaissance", "Mythology", "Italian" }
        },
        new Artwork
        {
            Id = 6,
            Title = "Guernica",
            Artist = "Pablo Picasso",
            Year = 1937,
            Medium = "Oil on canvas",
            Dimensions = "349.3 × 776.6 cm",
            Description = "Anti-war painting depicting the bombing of Guernica.",
            ImageUrl = "/images/artworks/guernica.jpg",
            Collection = "Permanent",
            Status = "OnDisplay",
            EstimatedValue = 200000000,
            Location = "Gallery C",
            AcquisitionDate = new DateTime(2005, 11, 30),
            AcquisitionMethod = "Long-term loan",
            Provenance = "Museo Reina Sofía, Madrid",
            Condition = "Good",
            Tags = new List<string> { "Cubism", "Modern", "Spanish", "War" }
        },
        new Artwork
        {
            Id = 7,
            Title = "The Great Wave off Kanagawa",
            Artist = "Katsushika Hokusai",
            Year = 1831,
            Medium = "Woodblock print",
            Dimensions = "25.7 × 37.9 cm",
            Description = "Iconic Japanese ukiyo-e print depicting a massive wave.",
            ImageUrl = "/images/artworks/great-wave.jpg",
            Collection = "Permanent",
            Status = "Available",
            EstimatedValue = 1500000,
            Location = "Storage B",
            AcquisitionDate = new DateTime(2019, 7, 8),
            AcquisitionMethod = "Purchase",
            Provenance = "Private collection, Tokyo",
            Condition = "Excellent",
            Tags = new List<string> { "Ukiyo-e", "Japanese", "Print" }
        },
        new Artwork
        {
            Id = 8,
            Title = "American Gothic",
            Artist = "Grant Wood",
            Year = 1930,
            Medium = "Oil on beaver board",
            Dimensions = "78 × 65.3 cm",
            Description = "Iconic American painting depicting a farmer and his daughter.",
            ImageUrl = "/images/artworks/american-gothic.jpg",
            Collection = "Temporary",
            Status = "OnDisplay",
            EstimatedValue = 25000000,
            Location = "Gallery D",
            AcquisitionDate = new DateTime(2022, 2, 14),
            AcquisitionMethod = "Loan",
            Provenance = "Art Institute of Chicago",
            Condition = "Excellent",
            Tags = new List<string> { "Regionalism", "American", "Portrait" }
        }
    };

    public static List<Exhibition> GetExhibitions() => new()
    {
        new Exhibition
        {
            Id = 1,
            Title = "Impressionism: Light and Color",
            Description = "A comprehensive look at the Impressionist movement featuring works from Monet, Renoir, and Degas.",
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 5, 31),
            Status = "Upcoming",
            Location = "Main Gallery",
            Curator = "Dr. Sarah Mitchell",
            ImageUrl = "/images/exhibitions/impressionism.jpg",
            Budget = 150000,
            ExpectedVisitors = 50000
        },
        new Exhibition
        {
            Id = 2,
            Title = "Modern Masters",
            Description = "Celebrating the revolutionary artists who defined modern art.",
            StartDate = new DateTime(2025, 11, 1),
            EndDate = new DateTime(2026, 3, 15),
            Status = "Active",
            Location = "West Wing",
            Curator = "Prof. Michael Chen",
            ImageUrl = "/images/exhibitions/modern-masters.jpg",
            Budget = 200000,
            ExpectedVisitors = 75000,
            ActualVisitors = 45000
        },
        new Exhibition
        {
            Id = 3,
            Title = "Dutch Golden Age",
            Description = "Masterpieces from the 17th century Dutch Republic.",
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2025, 9, 30),
            Status = "Past",
            Location = "East Gallery",
            Curator = "Dr. Anna van Berg",
            ImageUrl = "/images/exhibitions/dutch-golden-age.jpg",
            Budget = 175000,
            ExpectedVisitors = 60000,
            ActualVisitors = 68500
        },
        new Exhibition
        {
            Id = 4,
            Title = "Surrealism Dreams",
            Description = "Exploring the unconscious mind through surrealist art.",
            StartDate = new DateTime(2025, 3, 1),
            EndDate = new DateTime(2025, 5, 31),
            Status = "Past",
            Location = "South Gallery",
            Curator = "Dr. Carlos Rivera",
            ImageUrl = "/images/exhibitions/surrealism.jpg",
            Budget = 125000,
            ExpectedVisitors = 40000,
            ActualVisitors = 42000
        }
    };

    public static List<Visitor> GetVisitors() => new()
    {
        new Visitor { Id = 1, FirstName = "John", LastName = "Smith", Email = "john.smith@email.com", Phone = "555-0101", MembershipType = "Premium", MembershipExpiry = new DateTime(2027, 1, 1), TotalVisits = 25, LastVisit = new DateTime(2026, 1, 10), City = "New York", Country = "USA" },
        new Visitor { Id = 2, FirstName = "Emma", LastName = "Johnson", Email = "emma.j@email.com", Phone = "555-0102", MembershipType = "Patron", MembershipExpiry = new DateTime(2027, 6, 15), TotalVisits = 50, LastVisit = new DateTime(2026, 1, 15), City = "Boston", Country = "USA" },
        new Visitor { Id = 3, FirstName = "Michael", LastName = "Brown", Email = "m.brown@email.com", Phone = "555-0103", MembershipType = "Basic", MembershipExpiry = new DateTime(2026, 12, 31), TotalVisits = 10, LastVisit = new DateTime(2026, 1, 5), City = "Chicago", Country = "USA" },
        new Visitor { Id = 4, FirstName = "Sophie", LastName = "Laurent", Email = "sophie.l@email.com", Phone = "555-0104", MembershipType = "None", TotalVisits = 2, LastVisit = new DateTime(2025, 12, 20), City = "Paris", Country = "France" },
        new Visitor { Id = 5, FirstName = "Yuki", LastName = "Tanaka", Email = "yuki.t@email.com", Phone = "555-0105", MembershipType = "Student", MembershipExpiry = new DateTime(2026, 8, 31), TotalVisits = 15, LastVisit = new DateTime(2026, 1, 12), City = "Tokyo", Country = "Japan" }
    };

    public static List<Staff> GetStaff() => new()
    {
        new Staff { Id = 1, FirstName = "Sarah", LastName = "Mitchell", Email = "s.mitchell@gallery.com", Phone = "555-1001", Department = "Curatorial", Position = "Chief Curator", HireDate = new DateTime(2015, 3, 1), Salary = 95000, Status = "Active", Bio = "PhD in Art History from Yale University" },
        new Staff { Id = 2, FirstName = "James", LastName = "Wilson", Email = "j.wilson@gallery.com", Phone = "555-1002", Department = "Conservation", Position = "Head Conservator", HireDate = new DateTime(2017, 6, 15), Salary = 85000, Status = "Active", Bio = "Specialist in Renaissance art conservation" },
        new Staff { Id = 3, FirstName = "Maria", LastName = "Garcia", Email = "m.garcia@gallery.com", Phone = "555-1003", Department = "Security", Position = "Security Director", HireDate = new DateTime(2018, 1, 10), Salary = 75000, Status = "Active" },
        new Staff { Id = 4, FirstName = "David", LastName = "Lee", Email = "d.lee@gallery.com", Phone = "555-1004", Department = "Administration", Position = "Operations Manager", HireDate = new DateTime(2016, 9, 1), Salary = 70000, Status = "Active" },
        new Staff { Id = 5, FirstName = "Emily", LastName = "Brown", Email = "e.brown@gallery.com", Phone = "555-1005", Department = "Education", Position = "Education Coordinator", HireDate = new DateTime(2020, 2, 1), Salary = 55000, Status = "Active", Bio = "MA in Museum Studies" },
        new Staff { Id = 6, FirstName = "Robert", LastName = "Taylor", Email = "r.taylor@gallery.com", Phone = "555-1006", Department = "Marketing", Position = "Marketing Manager", HireDate = new DateTime(2019, 4, 15), Salary = 65000, Status = "Active" }
    };

    public static List<Loan> GetLoans() => new()
    {
        new Loan { Id = 1, ArtworkId = 4, BorrowerName = "Metropolitan Museum of Art", BorrowerType = "Museum", BorrowerContact = "loans@metmuseum.org", LoanStartDate = new DateTime(2025, 10, 1), LoanEndDate = new DateTime(2026, 4, 1), Status = "Active", InsuranceValue = 50000000, InsuranceProvider = "AXA Art", InsurancePolicyNumber = "AXA-2025-001", LoanFee = 100000, Purpose = "Surrealism Exhibition" },
        new Loan { Id = 2, ArtworkId = 3, BorrowerName = "National Gallery London", BorrowerType = "Museum", BorrowerContact = "loans@nationalgallery.org.uk", LoanStartDate = new DateTime(2026, 6, 1), LoanEndDate = new DateTime(2026, 12, 1), Status = "Pending", InsuranceValue = 35000000, InsuranceProvider = "Hiscox", InsurancePolicyNumber = "HIS-2026-042", LoanFee = 75000, Purpose = "Dutch Masters Exhibition" },
        new Loan { Id = 3, ArtworkId = 7, BorrowerName = "Tokyo National Museum", BorrowerType = "Museum", BorrowerContact = "international@tnm.jp", LoanStartDate = new DateTime(2025, 1, 1), LoanEndDate = new DateTime(2025, 6, 30), Status = "Returned", InsuranceValue = 2000000, InsuranceProvider = "AXA Art", InsurancePolicyNumber = "AXA-2024-089", LoanFee = 25000, Purpose = "Ukiyo-e Masters Exhibition", ConditionOnLoan = "Excellent", ConditionOnReturn = "Excellent" },
        new Loan { Id = 4, ArtworkId = 2, BorrowerName = "Art Institute of Chicago", BorrowerType = "Museum", BorrowerContact = "loans@artic.edu", LoanStartDate = new DateTime(2025, 3, 1), LoanEndDate = new DateTime(2025, 9, 1), Status = "Overdue", InsuranceValue = 100000000, InsuranceProvider = "Lloyd's of London", InsurancePolicyNumber = "LLO-2025-003", LoanFee = 150000, Purpose = "Van Gogh Retrospective" }
    };

    public static List<Insurance> GetInsurances() => new()
    {
        new Insurance { Id = 1, ArtworkId = 1, Provider = "AXA Art Insurance", PolicyNumber = "AXA-ART-001", CoverageAmount = 55000000, Premium = 27500, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "Active", CoverageType = "All-Risk" },
        new Insurance { Id = 2, ArtworkId = 2, Provider = "Hiscox Fine Art", PolicyNumber = "HIS-FA-002", CoverageAmount = 110000000, Premium = 55000, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "Active", CoverageType = "All-Risk" },
        new Insurance { Id = 3, ArtworkId = 5, Provider = "Lloyd's of London", PolicyNumber = "LLO-ART-003", CoverageAmount = 85000000, Premium = 42500, StartDate = new DateTime(2025, 6, 1), EndDate = new DateTime(2026, 5, 31), Status = "Active", CoverageType = "All-Risk" },
        new Insurance { Id = 4, ArtworkId = 6, Provider = "AXA Art Insurance", PolicyNumber = "AXA-ART-004", CoverageAmount = 220000000, Premium = 110000, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2026, 1, 31), Status = "Active", CoverageType = "All-Risk", Notes = "Renewal pending" },
        new Insurance { Id = 5, ArtworkId = 3, Provider = "Chubb Fine Art", PolicyNumber = "CHB-FA-005", CoverageAmount = 35000000, Premium = 17500, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 12, 31), Status = "Active", CoverageType = "Named Perils" }
    };

    public static List<Restoration> GetRestorations() => new()
    {
        new Restoration { Id = 1, ArtworkId = 5, Type = "Cleaning", Description = "Surface cleaning and varnish removal", StartDate = new DateTime(2025, 11, 1), Status = "InProgress", Conservator = "James Wilson", EstimatedCost = 50000, ConditionBefore = "Fair - yellowed varnish, surface dirt" },
        new Restoration { Id = 2, ArtworkId = 1, Type = "Conservation", Description = "Canvas stabilization and minor touch-ups", StartDate = new DateTime(2024, 6, 1), EndDate = new DateTime(2024, 9, 15), Status = "Completed", Conservator = "James Wilson", EstimatedCost = 35000, ActualCost = 32000, ConditionBefore = "Good - minor canvas tension issues", ConditionAfter = "Excellent" },
        new Restoration { Id = 3, ArtworkId = 6, Type = "Frame Restoration", Description = "Original frame repair and gold leaf restoration", StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 4, 30), Status = "Completed", Conservator = "Maria Santos", EstimatedCost = 15000, ActualCost = 18000, ConditionBefore = "Frame damage in corners", ConditionAfter = "Excellent" }
    };

    public static List<EtlSync> GetEtlSyncs() => new()
    {
        new EtlSync { Id = 1, SyncDate = new DateTime(2026, 1, 17, 2, 0, 0), Status = "Completed", RecordsProcessed = 1250, RecordsFailed = 0, Duration = 45000, SourceSystem = "OLTP", TargetSystem = "DW", SyncType = "Full" },
        new EtlSync { Id = 2, SyncDate = new DateTime(2026, 1, 16, 2, 0, 0), Status = "Completed", RecordsProcessed = 150, RecordsFailed = 2, Duration = 12000, SourceSystem = "OLTP", TargetSystem = "DW", SyncType = "Incremental", Details = "2 records skipped due to validation errors" },
        new EtlSync { Id = 3, SyncDate = new DateTime(2026, 1, 15, 2, 0, 0), Status = "Completed", RecordsProcessed = 180, RecordsFailed = 0, Duration = 15000, SourceSystem = "OLTP", TargetSystem = "DW", SyncType = "Incremental" }
    };
}
