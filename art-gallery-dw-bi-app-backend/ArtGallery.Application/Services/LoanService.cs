using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Loan;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class LoanService : ILoanService
{
    private readonly IRepository<Loan> _repository;
    private readonly IMapper _mapper;

    public LoanService(IRepository<Loan> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<LoanResponseDto>> GetAllAsync(PagedRequest request)
    {
        IQueryable<Loan> query = _repository.Query()
            .Include(l => l.Artwork)
            .Include(l => l.Exhibitor);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(l => (l.Exhibitor != null && l.Exhibitor.Name.ToLower().Contains(searchTerm)) ||
                                     (l.Artwork != null && l.Artwork.Title.ToLower().Contains(searchTerm)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "exhibitor" => request.IsDescending ? query.OrderByDescending(l => l.Exhibitor!.Name) : query.OrderBy(l => l.Exhibitor!.Name),
            "startdate" => request.IsDescending ? query.OrderByDescending(l => l.StartDate) : query.OrderBy(l => l.StartDate),
            "enddate" => request.IsDescending ? query.OrderByDescending(l => l.EndDate) : query.OrderBy(l => l.EndDate),
            _ => query.OrderByDescending(l => l.StartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<LoanResponseDto>>(items);

        return PaginatedResponse<LoanResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<LoanResponseDto?> GetByIdAsync(int id)
    {
        var loan = await _repository.Query()
            .Include(l => l.Artwork)
            .Include(l => l.Exhibitor)
            .FirstOrDefaultAsync(l => l.Id == id);
        return loan == null ? null : _mapper.Map<LoanResponseDto>(loan);
    }

    public async Task<LoanResponseDto> CreateAsync(CreateLoanDto dto)
    {
        var loan = _mapper.Map<Loan>(dto);
        await _repository.AddAsync(loan);
        await _repository.SaveChangesAsync();
        
        var createdLoan = await _repository.Query()
            .Include(l => l.Artwork)
            .Include(l => l.Exhibitor)
            .FirstAsync(l => l.Id == loan.Id);
        return _mapper.Map<LoanResponseDto>(createdLoan);
    }

    public async Task<LoanResponseDto> UpdateAsync(int id, UpdateLoanDto dto)
    {
        var loan = await _repository.Query()
            .Include(l => l.Artwork)
            .Include(l => l.Exhibitor)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException(nameof(Loan), id);

        if (dto.ArtworkId.HasValue) loan.ArtworkId = dto.ArtworkId.Value;
        if (dto.ExhibitorId.HasValue) loan.ExhibitorId = dto.ExhibitorId.Value;
        if (dto.StartDate.HasValue) loan.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) loan.EndDate = dto.EndDate;
        if (dto.Conditions != null) loan.Conditions = dto.Conditions;

        _repository.Update(loan);
        await _repository.SaveChangesAsync();
        return _mapper.Map<LoanResponseDto>(loan);
    }

    public async Task DeleteAsync(int id)
    {
        var loan = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Loan), id);

        _repository.Delete(loan);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<LoanResponseDto>> GetActiveAsync()
    {
        var today = DateTime.UtcNow.Date;
        var loans = await _repository.Query()
            .Include(l => l.Artwork)
            .Include(l => l.Exhibitor)
            .Where(l => l.EndDate == null || l.EndDate >= today)
            .ToListAsync();
        return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
    }

    public async Task<IEnumerable<LoanResponseDto>> GetOverdueAsync()
    {
        var today = DateTime.UtcNow.Date;
        var loans = await _repository.Query()
            .Include(l => l.Artwork)
            .Include(l => l.Exhibitor)
            .Where(l => l.EndDate != null && l.EndDate < today)
            .ToListAsync();
        return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
    }

    public async Task<LoanStatisticsDto> GetStatisticsAsync()
    {
        var loans = await _repository.Query()
            .Include(l => l.Exhibitor)
            .ToListAsync();
        var today = DateTime.UtcNow.Date;

        return new LoanStatisticsDto
        {
            TotalLoans = loans.Count,
            ActiveLoans = loans.Count(l => l.EndDate == null || l.EndDate >= today),
            ByExhibitor = loans
                .Where(l => l.Exhibitor != null)
                .GroupBy(l => l.Exhibitor!.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
