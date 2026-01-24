using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Artwork;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

/// <summary>
/// Service implementation for artwork operations.
/// </summary>
public class ArtworkService : IArtworkService
{
    private readonly IRepository<Artwork> _repository;
    private readonly IMapper _mapper;

    public ArtworkService(IRepository<Artwork> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ArtworkListDto>> GetAllAsync(PagedRequest request)
    {
        var query = _repository.Query();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(searchTerm) ||
                                     a.Artist.ToLower().Contains(searchTerm));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.IsDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "artist" => request.IsDescending ? query.OrderByDescending(a => a.Artist) : query.OrderBy(a => a.Artist),
            "year" => request.IsDescending ? query.OrderByDescending(a => a.Year) : query.OrderBy(a => a.Year),
            "estimatedvalue" => request.IsDescending ? query.OrderByDescending(a => a.EstimatedValue) : query.OrderBy(a => a.EstimatedValue),
            _ => query.OrderBy(a => a.Title)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ArtworkListDto>>(items);
        return PaginatedResponse<ArtworkListDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<ArtworkResponseDto?> GetByIdAsync(int id)
    {
        var artwork = await _repository.GetByIdAsync(id);
        return artwork == null ? null : _mapper.Map<ArtworkResponseDto>(artwork);
    }

    public async Task<ArtworkResponseDto> CreateAsync(CreateArtworkDto dto)
    {
        var artwork = _mapper.Map<Artwork>(dto);
        await _repository.AddAsync(artwork);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ArtworkResponseDto>(artwork);
    }

    public async Task<ArtworkResponseDto> UpdateAsync(int id, UpdateArtworkDto dto)
    {
        var artwork = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Artwork), id);

        // Update only non-null properties
        if (dto.Title != null) artwork.Title = dto.Title;
        if (dto.Artist != null) artwork.Artist = dto.Artist;
        if (dto.Year.HasValue) artwork.Year = dto.Year.Value;
        if (dto.Medium != null) artwork.Medium = dto.Medium;
        if (dto.Dimensions != null) artwork.Dimensions = dto.Dimensions;
        if (dto.Description != null) artwork.Description = dto.Description;
        if (dto.ImageUrl != null) artwork.ImageUrl = dto.ImageUrl;
        if (dto.Collection != null) artwork.Collection = dto.Collection;
        if (dto.Status != null) artwork.Status = dto.Status;
        if (dto.EstimatedValue.HasValue) artwork.EstimatedValue = dto.EstimatedValue;
        if (dto.Location != null) artwork.Location = dto.Location;
        if (dto.Condition != null) artwork.Condition = dto.Condition;
        if (dto.Tags != null) artwork.Tags = dto.Tags;

        _repository.Update(artwork);
        await _repository.SaveChangesAsync();
        return _mapper.Map<ArtworkResponseDto>(artwork);
    }

    public async Task DeleteAsync(int id)
    {
        var artwork = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Artwork), id);

        _repository.Delete(artwork);
        await _repository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ArtworkListDto>> SearchAsync(string query)
    {
        var searchTerm = query.ToLower();
        var artworks = await _repository.FindAsync(a =>
            a.Title.ToLower().Contains(searchTerm) ||
            a.Artist.ToLower().Contains(searchTerm));
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<ArtworkStatisticsDto> GetStatisticsAsync()
    {
        var artworks = await _repository.Query().ToListAsync();

        return new ArtworkStatisticsDto
        {
            TotalArtworks = artworks.Count,
            AvailableArtworks = artworks.Count(a => a.Status == "Available"),
            OnDisplayArtworks = artworks.Count(a => a.Status == "OnDisplay"),
            OnLoanArtworks = artworks.Count(a => a.Status == "OnLoan"),
            UnderRestorationArtworks = artworks.Count(a => a.Status == "UnderRestoration"),
            TotalEstimatedValue = artworks.Sum(a => a.EstimatedValue ?? 0),
            ByCollection = artworks.GroupBy(a => a.Collection)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByStatus = artworks.GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByArtistAsync(string artist)
    {
        var artworks = await _repository.FindAsync(a => 
            a.Artist.ToLower().Contains(artist.ToLower()));
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByCollectionAsync(string collection)
    {
        var artworks = await _repository.FindAsync(a => 
            a.Collection.ToLower() == collection.ToLower());
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByStatusAsync(string status)
    {
        var artworks = await _repository.FindAsync(a => 
            a.Status.ToLower() == status.ToLower());
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }
}
