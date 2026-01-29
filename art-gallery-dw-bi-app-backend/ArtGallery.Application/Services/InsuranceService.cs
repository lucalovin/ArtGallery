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
        IQueryable<Insurance> query = _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy);

        query = request.SortBy?.ToLower() switch
        {
            "provider" => request.IsDescending ? query.OrderByDescending(i => i.Policy!.Provider) : query.OrderBy(i => i.Policy!.Provider),
            "insuredamount" => request.IsDescending ? query.OrderByDescending(i => i.InsuredAmount) : query.OrderBy(i => i.InsuredAmount),
            _ => query.OrderBy(i => i.Id)
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
            .Include(i => i.Policy)
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
            .Include(i => i.Policy)
            .FirstAsync(i => i.Id == insurance.Id);
        return _mapper.Map<InsuranceResponseDto>(created);
    }

    public async Task<InsuranceResponseDto> UpdateAsync(int id, UpdateInsuranceDto dto)
    {
        var insurance = await _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException(nameof(Insurance), id);

        if (dto.ArtworkId.HasValue) insurance.ArtworkId = dto.ArtworkId.Value;
        if (dto.PolicyId.HasValue) insurance.PolicyId = dto.PolicyId.Value;
        if (dto.InsuredAmount.HasValue) insurance.InsuredAmount = dto.InsuredAmount.Value;

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
        var today = DateTime.UtcNow.Date;
        var insurances = await _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy)
            .Where(i => i.Policy != null && i.Policy.EndDate >= today)
            .ToListAsync();
        return _mapper.Map<IEnumerable<InsuranceResponseDto>>(insurances);
    }

    public async Task<IEnumerable<InsuranceResponseDto>> GetExpiringAsync(int daysThreshold = 30)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold).Date;
        var today = DateTime.UtcNow.Date;
        var insurances = await _repository.Query()
            .Include(i => i.Artwork)
            .Include(i => i.Policy)
            .Where(i => i.Policy != null && i.Policy.EndDate >= today && i.Policy.EndDate <= thresholdDate)
            .OrderBy(i => i.Policy!.EndDate)
            .ToListAsync();
        return _mapper.Map<IEnumerable<InsuranceResponseDto>>(insurances);
    }

    public async Task<InsuranceStatisticsDto> GetStatisticsAsync()
    {
        var insurances = await _repository.Query()
            .Include(i => i.Policy)
            .ToListAsync();

        return new InsuranceStatisticsDto
        {
            TotalInsurances = insurances.Count,
            TotalPolicies = insurances.Select(i => i.PolicyId).Distinct().Count(),
            TotalInsuredAmount = insurances.Sum(i => i.InsuredAmount),
            ByProvider = insurances
                .Where(i => i.Policy != null)
                .GroupBy(i => i.Policy!.Provider)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
