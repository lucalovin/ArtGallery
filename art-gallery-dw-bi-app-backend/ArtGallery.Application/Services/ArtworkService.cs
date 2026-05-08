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
    private readonly IDataSourceContext _ds;

    public ArtworkService(IRepository<Artwork> repository, IMapper mapper, IDataSourceContext ds)
    {
        _repository = repository;
        _mapper = mapper;
        _ds = ds;
    }

    private IQueryable<Artwork> WithIncludes(IQueryable<Artwork> q)
    {
        var s = _ds.Source;
        if (DataSourceCapabilities.HasArtist(s) && DataSourceCapabilities.ArtworkHasCore(s))
            q = q.Include(a => a.Artist);
        if (DataSourceCapabilities.HasCollection(s) && DataSourceCapabilities.ArtworkHasCore(s))
            q = q.Include(a => a.Collection);
        if (DataSourceCapabilities.HasLocation(s) && DataSourceCapabilities.ArtworkHasDetails(s))
            q = q.Include(a => a.Location);
        return q;
    }

    public async Task<PaginatedResponse<ArtworkListDto>> GetAllAsync(PagedRequest request)
    {
        var hasCore = DataSourceCapabilities.ArtworkHasCore(_ds.Source);
        IQueryable<Artwork> query = WithIncludes(_repository.Query());

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search) && hasCore)
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(searchTerm) ||
                                     (a.Artist != null && a.Artist.Name.ToLower().Contains(searchTerm)));
        }

        // Sorting
        var sortKey = request.SortBy?.ToLower();
        if (!hasCore && sortKey is "title" or "artist" or "year")
            sortKey = null; // those columns are absent on this source
        query = sortKey switch
        {
            "title"          => request.IsDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "artist"         => request.IsDescending ? query.OrderByDescending(a => a.Artist!.Name) : query.OrderBy(a => a.Artist!.Name),
            "year"           => request.IsDescending ? query.OrderByDescending(a => a.YearCreated) : query.OrderBy(a => a.YearCreated),
            "estimatedvalue" => request.IsDescending ? query.OrderByDescending(a => a.EstimatedValue) : query.OrderBy(a => a.EstimatedValue),
            _                => query.OrderBy(a => a.Id)
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
        var artwork = await WithIncludes(_repository.Query())
            .FirstOrDefaultAsync(a => a.Id == id);
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
        if (dto.ArtistId.HasValue) artwork.ArtistId = dto.ArtistId.Value;
        if (dto.YearCreated.HasValue) artwork.YearCreated = dto.YearCreated;
        if (dto.Medium != null) artwork.Medium = dto.Medium;
        if (dto.CollectionId.HasValue) artwork.CollectionId = dto.CollectionId;
        if (dto.LocationId.HasValue) artwork.LocationId = dto.LocationId;
        if (dto.EstimatedValue.HasValue) artwork.EstimatedValue = dto.EstimatedValue;

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
        if (!DataSourceCapabilities.ArtworkHasCore(_ds.Source))
            return Enumerable.Empty<ArtworkListDto>();
        var searchTerm = query.ToLower();
        var artworks = await WithIncludes(_repository.Query())
            .Where(a => a.Title.ToLower().Contains(searchTerm) ||
                       (a.Artist != null && a.Artist.Name.ToLower().Contains(searchTerm)))
            .ToListAsync();
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<ArtworkStatisticsDto> GetStatisticsAsync()
    {
        var artworks = await WithIncludes(_repository.Query()).ToListAsync();

        return new ArtworkStatisticsDto
        {
            TotalArtworks = artworks.Count,
            TotalEstimatedValue = artworks.Sum(a => a.EstimatedValue ?? 0),
            ByCollection = artworks
                .Where(a => a.Collection != null)
                .GroupBy(a => a.Collection!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByArtist = artworks
                .Where(a => a.Artist != null)
                .GroupBy(a => a.Artist!.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByLocation = artworks
                .Where(a => a.Location != null)
                .GroupBy(a => a.Location!.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByArtistAsync(string artist)
    {
        if (!DataSourceCapabilities.ArtworkHasCore(_ds.Source) || !DataSourceCapabilities.HasArtist(_ds.Source))
            return Enumerable.Empty<ArtworkListDto>();
        var artworks = await WithIncludes(_repository.Query())
            .Where(a => a.Artist != null && a.Artist.Name.ToLower().Contains(artist.ToLower()))
            .ToListAsync();
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByCollectionAsync(string collection)
    {
        if (!DataSourceCapabilities.ArtworkHasCore(_ds.Source) || !DataSourceCapabilities.HasCollection(_ds.Source))
            return Enumerable.Empty<ArtworkListDto>();
        var artworks = await WithIncludes(_repository.Query())
            .Where(a => a.Collection != null && a.Collection.Name.ToLower() == collection.ToLower())
            .ToListAsync();
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }

    public async Task<IEnumerable<ArtworkListDto>> GetByStatusAsync(string status)
    {
        // Note: Status field no longer exists in the new schema
        var artworks = await WithIncludes(_repository.Query()).ToListAsync();
        return _mapper.Map<IEnumerable<ArtworkListDto>>(artworks);
    }
}
