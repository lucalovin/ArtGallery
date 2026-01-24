using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Restoration;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class RestorationService : IRestorationService
{
    private readonly IRepository<Restoration> _repository;
    private readonly IMapper _mapper;

    public RestorationService(IRepository<Restoration> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<RestorationResponseDto>> GetAllAsync(PagedRequest request)
    {
        IQueryable<Restoration> query = _repository.Query().Include(r => r.Artwork);

        query = request.SortBy?.ToLower() switch
        {
            "startdate" => request.IsDescending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate),
            "status" => request.IsDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "type" => request.IsDescending ? query.OrderByDescending(r => r.Type) : query.OrderBy(r => r.Type),
            _ => query.OrderByDescending(r => r.StartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<RestorationResponseDto>>(items);

        return PaginatedResponse<RestorationResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<RestorationResponseDto?> GetByIdAsync(int id)
    {
        var restoration = await _repository.Query()
            .Include(r => r.Artwork)
            .FirstOrDefaultAsync(r => r.Id == id);
        return restoration == null ? null : _mapper.Map<RestorationResponseDto>(restoration);
    }

    public async Task<RestorationResponseDto> CreateAsync(CreateRestorationDto dto)
    {
        var restoration = _mapper.Map<Restoration>(dto);
        await _repository.AddAsync(restoration);
        await _repository.SaveChangesAsync();

        var created = await _repository.Query()
            .Include(r => r.Artwork)
            .FirstAsync(r => r.Id == restoration.Id);
        return _mapper.Map<RestorationResponseDto>(created);
    }

    public async Task<RestorationResponseDto> UpdateAsync(int id, UpdateRestorationDto dto)
    {
        var restoration = await _repository.Query()
            .Include(r => r.Artwork)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(Restoration), id);

        if (dto.Type != null) restoration.Type = dto.Type;
        if (dto.Description != null) restoration.Description = dto.Description;
        if (dto.StartDate.HasValue) restoration.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) restoration.EndDate = dto.EndDate;
        if (dto.Status != null) restoration.Status = dto.Status;
        if (dto.Conservator != null) restoration.Conservator = dto.Conservator;
        if (dto.EstimatedCost.HasValue) restoration.EstimatedCost = dto.EstimatedCost;
        if (dto.ActualCost.HasValue) restoration.ActualCost = dto.ActualCost;
        if (dto.ConditionBefore != null) restoration.ConditionBefore = dto.ConditionBefore;
        if (dto.ConditionAfter != null) restoration.ConditionAfter = dto.ConditionAfter;
        if (dto.Notes != null) restoration.Notes = dto.Notes;

        _repository.Update(restoration);
        await _repository.SaveChangesAsync();
        return _mapper.Map<RestorationResponseDto>(restoration);
    }

    public async Task DeleteAsync(int id)
    {
        var restoration = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Restoration), id);

        _repository.Delete(restoration);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<RestorationResponseDto>> GetInProgressAsync()
    {
        var restorations = await _repository.Query()
            .Include(r => r.Artwork)
            .Where(r => r.Status == "InProgress")
            .ToListAsync();
        return _mapper.Map<IEnumerable<RestorationResponseDto>>(restorations);
    }

    public async Task<IEnumerable<RestorationResponseDto>> GetCompletedAsync()
    {
        var restorations = await _repository.Query()
            .Include(r => r.Artwork)
            .Where(r => r.Status == "Completed")
            .OrderByDescending(r => r.EndDate)
            .ToListAsync();
        return _mapper.Map<IEnumerable<RestorationResponseDto>>(restorations);
    }

    public async Task<RestorationStatisticsDto> GetStatisticsAsync()
    {
        var restorations = await _repository.Query().ToListAsync();

        return new RestorationStatisticsDto
        {
            TotalRestorations = restorations.Count,
            InProgressRestorations = restorations.Count(r => r.Status == "InProgress"),
            CompletedRestorations = restorations.Count(r => r.Status == "Completed"),
            TotalEstimatedCost = restorations.Sum(r => r.EstimatedCost ?? 0),
            TotalActualCost = restorations.Sum(r => r.ActualCost ?? 0),
            ByStatus = restorations.GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByType = restorations.GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
