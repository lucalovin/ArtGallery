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
        IQueryable<Loan> query = _repository.Query().Include(l => l.Artwork);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(l => l.BorrowerName.ToLower().Contains(searchTerm) ||
                                     l.Artwork.Title.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "borrowername" => request.IsDescending ? query.OrderByDescending(l => l.BorrowerName) : query.OrderBy(l => l.BorrowerName),
            "startdate" => request.IsDescending ? query.OrderByDescending(l => l.LoanStartDate) : query.OrderBy(l => l.LoanStartDate),
            "enddate" => request.IsDescending ? query.OrderByDescending(l => l.LoanEndDate) : query.OrderBy(l => l.LoanEndDate),
            _ => query.OrderByDescending(l => l.LoanStartDate)
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
            .FirstAsync(l => l.Id == loan.Id);
        return _mapper.Map<LoanResponseDto>(createdLoan);
    }

    public async Task<LoanResponseDto> UpdateAsync(int id, UpdateLoanDto dto)
    {
        var loan = await _repository.Query()
            .Include(l => l.Artwork)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException(nameof(Loan), id);

        if (dto.BorrowerName != null) loan.BorrowerName = dto.BorrowerName;
        if (dto.BorrowerType != null) loan.BorrowerType = dto.BorrowerType;
        if (dto.BorrowerContact != null) loan.BorrowerContact = dto.BorrowerContact;
        if (dto.BorrowerAddress != null) loan.BorrowerAddress = dto.BorrowerAddress;
        if (dto.LoanStartDate.HasValue) loan.LoanStartDate = dto.LoanStartDate.Value;
        if (dto.LoanEndDate.HasValue) loan.LoanEndDate = dto.LoanEndDate.Value;
        if (dto.Status != null) loan.Status = dto.Status;
        if (dto.InsuranceValue.HasValue) loan.InsuranceValue = dto.InsuranceValue;
        if (dto.InsuranceProvider != null) loan.InsuranceProvider = dto.InsuranceProvider;
        if (dto.InsurancePolicyNumber != null) loan.InsurancePolicyNumber = dto.InsurancePolicyNumber;
        if (dto.LoanFee.HasValue) loan.LoanFee = dto.LoanFee;
        if (dto.Purpose != null) loan.Purpose = dto.Purpose;
        if (dto.ConditionOnLoan != null) loan.ConditionOnLoan = dto.ConditionOnLoan;
        if (dto.ConditionOnReturn != null) loan.ConditionOnReturn = dto.ConditionOnReturn;
        if (dto.Notes != null) loan.Notes = dto.Notes;

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
        var loans = await _repository.Query()
            .Include(l => l.Artwork)
            .Where(l => l.Status == "Active")
            .ToListAsync();
        return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
    }

    public async Task<IEnumerable<LoanResponseDto>> GetOverdueAsync()
    {
        var today = DateTime.UtcNow.Date;
        var loans = await _repository.Query()
            .Include(l => l.Artwork)
            .Where(l => l.LoanEndDate < today && l.Status == "Active")
            .ToListAsync();
        return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
    }

    public async Task<LoanStatisticsDto> GetStatisticsAsync()
    {
        var loans = await _repository.Query().ToListAsync();
        var today = DateTime.UtcNow.Date;

        return new LoanStatisticsDto
        {
            TotalLoans = loans.Count,
            ActiveLoans = loans.Count(l => l.Status == "Active"),
            OverdueLoans = loans.Count(l => l.LoanEndDate < today && l.Status == "Active"),
            TotalInsuranceValue = loans.Where(l => l.Status == "Active").Sum(l => l.InsuranceValue ?? 0),
            TotalLoanFees = loans.Sum(l => l.LoanFee ?? 0),
            ByStatus = loans.GroupBy(l => l.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
