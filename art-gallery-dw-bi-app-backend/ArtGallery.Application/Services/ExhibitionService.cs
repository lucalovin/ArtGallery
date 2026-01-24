using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Exhibition;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

/// <summary>
/// Service implementation for exhibition operations.
/// </summary>
public class ExhibitionService : IExhibitionService
{
    private readonly IRepository<Exhibition> _repository;
    private readonly IRepository<ExhibitionArtwork> _exhibitionArtworkRepository;
    private readonly IRepository<Artwork> _artworkRepository;
    private readonly IMapper _mapper;

    public ExhibitionService(
        IRepository<Exhibition> repository,
        IRepository<ExhibitionArtwork> exhibitionArtworkRepository,
        IRepository<Artwork> artworkRepository,
        IMapper mapper)
    {
        _repository = repository;
        _exhibitionArtworkRepository = exhibitionArtworkRepository;
        _artworkRepository = artworkRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ExhibitionResponseDto>> GetAllAsync(PagedRequest request)
    {
        IQueryable<Exhibition> query = _repository.Query()
            .Include(e => e.ExhibitionArtworks);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "title" => request.IsDescending ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "startdate" => request.IsDescending ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate),
            "enddate" => request.IsDescending ? query.OrderByDescending(e => e.EndDate) : query.OrderBy(e => e.EndDate),
            _ => query.OrderByDescending(e => e.StartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = _mapper.Map<List<ExhibitionResponseDto>>(items);

        return PaginatedResponse<ExhibitionResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<ExhibitionDetailDto?> GetByIdAsync(int id)
    {
        var exhibition = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
                .ThenInclude(ea => ea.Artwork)
            .FirstOrDefaultAsync(e => e.Id == id);

        return exhibition == null ? null : _mapper.Map<ExhibitionDetailDto>(exhibition);
    }

    public async Task<ExhibitionResponseDto> CreateAsync(CreateExhibitionDto dto)
    {
        var exhibition = _mapper.Map<Exhibition>(dto);
        await _repository.AddAsync(exhibition);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ExhibitionResponseDto>(exhibition);
    }

    public async Task<ExhibitionResponseDto> UpdateAsync(int id, UpdateExhibitionDto dto)
    {
        var exhibition = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Exhibition), id);

        if (dto.Title != null) exhibition.Title = dto.Title;
        if (dto.Description != null) exhibition.Description = dto.Description;
        if (dto.StartDate.HasValue) exhibition.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) exhibition.EndDate = dto.EndDate.Value;
        if (dto.Status != null) exhibition.Status = dto.Status;
        if (dto.Location != null) exhibition.Location = dto.Location;
        if (dto.Curator != null) exhibition.Curator = dto.Curator;
        if (dto.ImageUrl != null) exhibition.ImageUrl = dto.ImageUrl;
        if (dto.Budget.HasValue) exhibition.Budget = dto.Budget;
        if (dto.ExpectedVisitors.HasValue) exhibition.ExpectedVisitors = dto.ExpectedVisitors;
        if (dto.ActualVisitors.HasValue) exhibition.ActualVisitors = dto.ActualVisitors;

        _repository.Update(exhibition);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ExhibitionResponseDto>(exhibition);
    }

    public async Task DeleteAsync(int id)
    {
        var exhibition = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Exhibition), id);

        _repository.Delete(exhibition);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExhibitionResponseDto>> GetUpcomingAsync(int limit = 5)
    {
        var exhibitions = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Where(e => e.StartDate > DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Take(limit)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(exhibitions);
    }

    public async Task<IEnumerable<ExhibitionResponseDto>> GetActiveAsync()
    {
        var today = DateTime.UtcNow.Date;
        var exhibitions = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Where(e => e.StartDate <= today && e.EndDate >= today)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(exhibitions);
    }

    public async Task<IEnumerable<ExhibitionResponseDto>> GetPastAsync()
    {
        var exhibitions = await _repository.Query()
            .Include(e => e.ExhibitionArtworks)
            .Where(e => e.EndDate < DateTime.UtcNow)
            .OrderByDescending(e => e.EndDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionResponseDto>>(exhibitions);
    }

    public async Task AddArtworkAsync(int exhibitionId, int artworkId, int? displayOrder = null)
    {
        var exhibition = await _repository.GetByIdAsync(exhibitionId)
            ?? throw new NotFoundException(nameof(Exhibition), exhibitionId);

        var artwork = await _artworkRepository.GetByIdAsync(artworkId)
            ?? throw new NotFoundException(nameof(Artwork), artworkId);

        var exists = await _exhibitionArtworkRepository.AnyAsync(
            ea => ea.ExhibitionId == exhibitionId && ea.ArtworkId == artworkId);

        if (exists)
            throw new ConflictException($"Artwork {artworkId} is already in exhibition {exhibitionId}");

        var exhibitionArtwork = new ExhibitionArtwork
        {
            ExhibitionId = exhibitionId,
            ArtworkId = artworkId,
            DisplayOrder = displayOrder
        };

        await _exhibitionArtworkRepository.AddAsync(exhibitionArtwork);
        await _exhibitionArtworkRepository.SaveChangesAsync();
    }

    public async Task RemoveArtworkAsync(int exhibitionId, int artworkId)
    {
        var exhibitionArtwork = await _exhibitionArtworkRepository.Query()
            .FirstOrDefaultAsync(ea => ea.ExhibitionId == exhibitionId && ea.ArtworkId == artworkId)
            ?? throw new NotFoundException("ExhibitionArtwork", $"{exhibitionId}-{artworkId}");

        _exhibitionArtworkRepository.Delete(exhibitionArtwork);
        await _exhibitionArtworkRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExhibitionArtworkDto>> GetArtworksAsync(int exhibitionId)
    {
        var exhibitionArtworks = await _exhibitionArtworkRepository.Query()
            .Include(ea => ea.Artwork)
            .Where(ea => ea.ExhibitionId == exhibitionId)
            .OrderBy(ea => ea.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ExhibitionArtworkDto>>(exhibitionArtworks);
    }
}
