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
        var staff = await _staffRepository.Query().Where(s => s.Status == "Active").ToListAsync();
        var loans = await _loanRepository.Query().Where(l => l.Status == "Active").ToListAsync();
        var restorations = await _restorationRepository.Query().Where(r => r.Status == "InProgress").ToListAsync();
        var insurances = await _insuranceRepository.Query().Where(i => i.Status == "Active").ToListAsync();

        return new KpiDashboardDto
        {
            TotalArtworks = artworks.Count,
            TotalVisitors = visitors.Count,
            ActiveExhibitions = exhibitions.Count,
            TotalStaff = staff.Count,
            TotalCollectionValue = artworks.Sum(a => a.EstimatedValue ?? 0),
            ActiveLoans = loans.Count,
            ArtworksUnderRestoration = restorations.Count,
            TotalInsuranceCoverage = insurances.Sum(i => i.CoverageAmount),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<VisitorTrendDto>> GetVisitorTrendsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-6);
        var end = endDate ?? DateTime.UtcNow;

        var visitors = await _visitorRepository.Query()
            .Where(v => v.CreatedAt >= start && v.CreatedAt <= end)
            .ToListAsync();

        var trends = visitors
            .GroupBy(v => v.CreatedAt.Date)
            .Select(g => new VisitorTrendDto
            {
                Date = g.Key,
                VisitorCount = g.Count(),
                NewMembers = g.Count(v => v.MembershipType != "None")
            })
            .OrderBy(t => t.Date)
            .ToList();

        return trends;
    }

    public async Task<IEnumerable<ArtworkDistributionDto>> GetArtworkDistributionAsync()
    {
        var artworks = await _artworkRepository.Query().ToListAsync();

        var distribution = artworks
            .GroupBy(a => a.Collection)
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
            .Where(e => e.ActualVisitors.HasValue)
            .ToListAsync();

        var performance = exhibitions
            .Select(e => new ExhibitionPerformanceDto
            {
                ExhibitionId = e.Id,
                Title = e.Title,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                ExpectedVisitors = e.ExpectedVisitors,
                ActualVisitors = e.ActualVisitors,
                PerformanceRatio = e.ExpectedVisitors.HasValue && e.ExpectedVisitors > 0
                    ? (double)(e.ActualVisitors ?? 0) / e.ExpectedVisitors.Value
                    : 0,
                Budget = e.Budget
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
            .Where(l => l.LoanStartDate >= start && l.LoanStartDate <= end)
            .ToListAsync();

        // Group by month
        var revenue = loans
            .GroupBy(l => new DateTime(l.LoanStartDate.Year, l.LoanStartDate.Month, 1))
            .Select(g => new RevenueDto
            {
                Period = g.Key,
                LoanFees = g.Sum(l => l.LoanFee ?? 0),
                TicketSales = 0, // Would come from a separate tickets table
                MembershipFees = 0, // Would come from membership transactions
                TotalRevenue = g.Sum(l => l.LoanFee ?? 0)
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
