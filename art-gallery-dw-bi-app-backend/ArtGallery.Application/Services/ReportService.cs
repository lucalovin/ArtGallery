using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Reports;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class ReportService : IReportService
{
    private readonly IRepository<Artwork> _artworkRepository;
    private readonly IRepository<Exhibition> _exhibitionRepository;
    private readonly IRepository<Visitor> _visitorRepository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<Loan> _loanRepository;
    private readonly IRepository<Insurance> _insuranceRepository;
    private readonly IRepository<Restoration> _restorationRepository;

    public ReportService(
        IRepository<Artwork> artworkRepository,
        IRepository<Exhibition> exhibitionRepository,
        IRepository<Visitor> visitorRepository,
        IRepository<Staff> staffRepository,
        IRepository<Loan> loanRepository,
        IRepository<Insurance> insuranceRepository,
        IRepository<Restoration> restorationRepository)
    {
        _artworkRepository = artworkRepository;
        _exhibitionRepository = exhibitionRepository;
        _visitorRepository = visitorRepository;
        _staffRepository = staffRepository;
        _loanRepository = loanRepository;
        _insuranceRepository = insuranceRepository;
        _restorationRepository = restorationRepository;
    }

    public async Task<KpiDashboardDto> GetKpisAsync()
    {
        var today = DateTime.UtcNow.Date;

        var artworks = await _artworkRepository.Query().ToListAsync();
        var visitors = await _visitorRepository.Query().ToListAsync();
        var exhibitions = await _exhibitionRepository.Query()
            .Where(e => e.StartDate <= today && e.EndDate >= today)
            .ToListAsync();
        var staff = await _staffRepository.Query().ToListAsync();
        var loans = await _loanRepository.Query().Where(l => l.EndDate == null || l.EndDate >= today).ToListAsync();
        var restorations = await _restorationRepository.Query().Where(r => r.EndDate == null).ToListAsync();
        var insurances = await _insuranceRepository.Query()
            .Include(i => i.Policy)
            .Where(i => i.Policy != null && i.Policy.EndDate >= today)
            .ToListAsync();

        return new KpiDashboardDto
        {
            TotalArtworks = artworks.Count,
            TotalVisitors = visitors.Count,
            ActiveExhibitions = exhibitions.Count,
            TotalStaff = staff.Count,
            TotalCollectionValue = artworks.Sum(a => a.EstimatedValue ?? 0),
            ActiveLoans = loans.Count,
            ArtworksUnderRestoration = restorations.Count,
            TotalInsuranceCoverage = insurances.Sum(i => i.InsuredAmount),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<VisitorTrendDto>> GetVisitorTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-6);
        var end = endDate ?? DateTime.UtcNow;

        var visitors = await _visitorRepository.Query()
            .Where(v => v.JoinDate != null && v.JoinDate >= start && v.JoinDate <= end)
            .ToListAsync();

        var trends = visitors
            .GroupBy(v => v.JoinDate!.Value.Date)
            .Select(g => new VisitorTrendDto
            {
                Date = g.Key,
                VisitorCount = g.Count(),
                NewMembers = g.Count(v => v.MembershipType != null && v.MembershipType != "None")
            })
            .OrderBy(t => t.Date)
            .ToList();

        return trends;
    }

    public async Task<IEnumerable<ArtworkDistributionDto>> GetArtworkDistributionAsync()
    {
        var artworks = await _artworkRepository.Query()
            .Include(a => a.Collection)
            .ToListAsync();

        var distribution = artworks
            .Where(a => a.Collection != null)
            .GroupBy(a => a.Collection!.Name)
            .Select(g => new ArtworkDistributionDto
            {
                Category = g.Key,
                Count = g.Count(),
                TotalValue = g.Sum(a => a.EstimatedValue ?? 0)
            })
            .OrderByDescending(d => d.Count)
            .ToList();

        return distribution;
    }

    public async Task<IEnumerable<ExhibitionPerformanceDto>> GetExhibitionPerformanceAsync()
    {
        var exhibitions = await _exhibitionRepository.Query()
            .Include(e => e.ExhibitionArtworks)
            .ToListAsync();

        var performance = exhibitions
            .Select(e => new ExhibitionPerformanceDto
            {
                ExhibitionId = e.Id,
                Title = e.Title,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                ExpectedVisitors = null,
                ActualVisitors = null,
                PerformanceRatio = 0,
                Budget = null
            })
            .OrderByDescending(p => p.EndDate)
            .ToList();

        return performance;
    }

    public async Task<IEnumerable<RevenueDto>> GetRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = endDate ?? DateTime.UtcNow;

        var loans = await _loanRepository.Query()
            .Where(l => l.StartDate >= start && l.StartDate <= end)
            .ToListAsync();

        // Group by month
        var revenue = loans
            .GroupBy(l => new DateTime(l.StartDate.Year, l.StartDate.Month, 1))
            .Select(g => new RevenueDto
            {
                Period = g.Key,
                LoanFees = 0, // Loan fees not in the new schema
                TicketSales = 0, // Would come from a separate tickets table
                MembershipFees = 0, // Would come from membership transactions
                TotalRevenue = 0
            })
            .OrderBy(r => r.Period)
            .ToList();

        return revenue;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        return new DashboardDto
        {
            Kpis = await GetKpisAsync(),
            VisitorTrends = (await GetVisitorTrendsAsync()).ToList(),
            ArtworkDistribution = (await GetArtworkDistributionAsync()).ToList(),
            ExhibitionPerformance = (await GetExhibitionPerformanceAsync()).ToList(),
            RevenueData = (await GetRevenueAsync()).ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }
}
