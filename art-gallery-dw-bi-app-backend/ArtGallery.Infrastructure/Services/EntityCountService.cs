using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.Interfaces;
using ArtGallery.Infrastructure.Data;

namespace ArtGallery.Infrastructure.Services;

/// <summary>
/// Service for getting entity counts from the database.
/// Used by ETL status to report record counts per data source.
/// </summary>
public class EntityCountService : IEntityCountService
{
    private readonly AppDbContext _context;

    public EntityCountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetArtworkCountAsync()
    {
        return await _context.Artworks.CountAsync();
    }

    public async Task<int> GetExhibitionCountAsync()
    {
        return await _context.Exhibitions.CountAsync();
    }

    public async Task<int> GetVisitorCountAsync()
    {
        return await _context.Visitors.CountAsync();
    }

    public async Task<int> GetStaffCountAsync()
    {
        return await _context.Staff.CountAsync();
    }

    public async Task<int> GetLoanCountAsync()
    {
        return await _context.Loans.CountAsync();
    }
}
