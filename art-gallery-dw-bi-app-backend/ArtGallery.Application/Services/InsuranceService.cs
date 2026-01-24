using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Insurance;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class InsuranceService : IInsuranceService
{
    private readonly IRepository<Insurance> _repository;
    private readonly IMapper _mapper;

    public InsuranceService(IRepository<Insurance> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<InsuranceResponseDto>> GetAllAsync(PagedRequest request)
    {
        IQueryable<Insurance> query = _repository.Query().Include(i => i.Artwork);

        query = request.SortBy?.ToLower() switch
        {
            "provider" => request.IsDescending ? query.OrderByDescending(i => i.Provider) : query.OrderBy(i => i.Provider),
            "enddate" => request.IsDescending ? query.OrderByDescending(i => i.EndDate) : query.OrderBy(i => i.EndDate),
            "coverageamount" => request.IsDescending ? query.OrderByDescending(i => i.CoverageAmount) : query.OrderBy(i => i.CoverageAmount),
            _ => query.OrderBy(i => i.EndDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<InsuranceResponseDto>>(items);

        return PaginatedResponse<InsuranceResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<InsuranceResponseDto?> GetByIdAsync(int id)
    {
        var insurance = await _repository.Query()
            .Include(i => i.Artwork)
            .FirstOrDefaultAsync(i => i.Id == id);
        return insurance == null ? null : _mapper.Map<InsuranceResponseDto>(insurance);
    }

    public async Task<InsuranceResponseDto> CreateAsync(CreateInsuranceDto dto)
    {
        var insurance = _mapper.Map<Insurance>(dto);
        await _repository.AddAsync(insurance);
        await _repository.SaveChangesAsync();

        var created = await _repository.Query()
            .Include(i => i.Artwork)
            .FirstAsync(i => i.Id == insurance.Id);
        return _mapper.Map<InsuranceResponseDto>(created);
    }

    public async Task<InsuranceResponseDto> UpdateAsync(int id, UpdateInsuranceDto dto)
    {
        var insurance = await _repository.Query()
            .Include(i => i.Artwork)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException(nameof(Insurance), id);

        if (dto.Provider != null) insurance.Provider = dto.Provider;
        if (dto.PolicyNumber != null) insurance.PolicyNumber = dto.PolicyNumber;
        if (dto.CoverageAmount.HasValue) insurance.CoverageAmount = dto.CoverageAmount.Value;
        if (dto.Premium.HasValue) insurance.Premium = dto.Premium.Value;
        if (dto.StartDate.HasValue) insurance.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) insurance.EndDate = dto.EndDate.Value;
        if (dto.Status != null) insurance.Status = dto.Status;
        if (dto.CoverageType != null) insurance.CoverageType = dto.CoverageType;
        if (dto.Notes != null) insurance.Notes = dto.Notes;

        _repository.Update(insurance);
        await _repository.SaveChangesAsync();
        return _mapper.Map<InsuranceResponseDto>(insurance);
    }

    public async Task DeleteAsync(int id)
    {
        var insurance = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Insurance), id);

        _repository.Delete(insurance);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<InsuranceResponseDto>> GetActiveAsync()
    {
        var insurances = await _repository.Query()
            .Include(i => i.Artwork)
            .Where(i => i.Status == "Active")
            .ToListAsync();
        return _mapper.Map<IEnumerable<InsuranceResponseDto>>(insurances);
    }

    public async Task<IEnumerable<InsuranceResponseDto>> GetExpiringAsync(int daysThreshold = 30)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
        var insurances = await _repository.Query()
            .Include(i => i.Artwork)
            .Where(i => i.EndDate <= thresholdDate && i.Status == "Active")
            .OrderBy(i => i.EndDate)
            .ToListAsync();
        return _mapper.Map<IEnumerable<InsuranceResponseDto>>(insurances);
    }

    public async Task<InsuranceStatisticsDto> GetStatisticsAsync()
    {
        var insurances = await _repository.Query().ToListAsync();
        var thresholdDate = DateTime.UtcNow.AddDays(30);

        return new InsuranceStatisticsDto
        {
            TotalPolicies = insurances.Count,
            ActivePolicies = insurances.Count(i => i.Status == "Active"),
            ExpiringPolicies = insurances.Count(i => i.EndDate <= thresholdDate && i.Status == "Active"),
            TotalCoverageAmount = insurances.Where(i => i.Status == "Active").Sum(i => i.CoverageAmount),
            TotalPremiums = insurances.Where(i => i.Status == "Active").Sum(i => i.Premium),
            ByStatus = insurances.GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByProvider = insurances.GroupBy(i => i.Provider)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
