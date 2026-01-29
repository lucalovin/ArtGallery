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
        IQueryable<Restoration> query = _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff);

        query = request.SortBy?.ToLower() switch
        {
            "startdate" => request.IsDescending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate),
            "staff" => request.IsDescending ? query.OrderByDescending(r => r.Staff!.Name) : query.OrderBy(r => r.Staff!.Name),
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
            .Include(r => r.Staff)
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
            .Include(r => r.Staff)
            .FirstAsync(r => r.Id == restoration.Id);
        return _mapper.Map<RestorationResponseDto>(created);
    }

    public async Task<RestorationResponseDto> UpdateAsync(int id, UpdateRestorationDto dto)
    {
        var restoration = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(Restoration), id);

        if (dto.ArtworkId.HasValue) restoration.ArtworkId = dto.ArtworkId.Value;
        if (dto.StaffId.HasValue) restoration.StaffId = dto.StaffId.Value;
        if (dto.StartDate.HasValue) restoration.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) restoration.EndDate = dto.EndDate;
        if (dto.Description != null) restoration.Description = dto.Description;

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
            .Include(r => r.Staff)
            .Where(r => r.EndDate == null)
            .ToListAsync();
        return _mapper.Map<IEnumerable<RestorationResponseDto>>(restorations);
    }

    public async Task<IEnumerable<RestorationResponseDto>> GetCompletedAsync()
    {
        var restorations = await _repository.Query()
            .Include(r => r.Artwork)
            .Include(r => r.Staff)
            .Where(r => r.EndDate != null)
            .OrderByDescending(r => r.EndDate)
            .ToListAsync();
        return _mapper.Map<IEnumerable<RestorationResponseDto>>(restorations);
    }

    public async Task<RestorationStatisticsDto> GetStatisticsAsync()
    {
        var restorations = await _repository.Query()
            .Include(r => r.Staff)
            .ToListAsync();

        return new RestorationStatisticsDto
        {
            TotalRestorations = restorations.Count,
            InProgressRestorations = restorations.Count(r => r.EndDate == null),
            CompletedRestorations = restorations.Count(r => r.EndDate != null),
            ByStaff = restorations
                .Where(r => r.Staff != null)
                .GroupBy(r => r.Staff!.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
