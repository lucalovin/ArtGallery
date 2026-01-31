using Microsoft.EntityFrameworkCore;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Review;
using ArtGallery.Application.Exceptions;
using ArtGallery.Application.Interfaces;
using ArtGallery.Domain.Entities;
using ArtGallery.Domain.Interfaces;

namespace ArtGallery.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IRepository<GalleryReview> _reviewRepository;
    private readonly IRepository<Visitor> _visitorRepository;

    public ReviewService(
        IRepository<GalleryReview> reviewRepository,
        IRepository<Visitor> visitorRepository)
    {
        _reviewRepository = reviewRepository;
        _visitorRepository = visitorRepository;
    }

    public async Task<PaginatedResponse<ReviewResponseDto>> GetAllAsync(PagedRequest request)
    {
        var query = _reviewRepository.Query()
            .Include(r => r.Visitor)
            .Include(r => r.Artwork)
            .Include(r => r.Exhibition)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(r => 
                (r.Visitor != null && r.Visitor.Name.ToLower().Contains(searchTerm)) ||
                (r.ReviewText != null && r.ReviewText.ToLower().Contains(searchTerm)) ||
                (r.Artwork != null && r.Artwork.Title.ToLower().Contains(searchTerm)) ||
                (r.Exhibition != null && r.Exhibition.Title.ToLower().Contains(searchTerm)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "visitorname" => request.IsDescending 
                ? query.OrderByDescending(r => r.Visitor!.Name) 
                : query.OrderBy(r => r.Visitor!.Name),
            "rating" => request.IsDescending 
                ? query.OrderByDescending(r => r.Rating) 
                : query.OrderBy(r => r.Rating),
            "reviewdate" => request.IsDescending 
                ? query.OrderByDescending(r => r.ReviewDate) 
                : query.OrderBy(r => r.ReviewDate),
            _ => query.OrderByDescending(r => r.ReviewDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync();
        var dtos = items.Select(MapToDto).ToList();

        return PaginatedResponse<ReviewResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<ReviewResponseDto?> GetByIdAsync(int id)
    {
        var review = await _reviewRepository.Query()
            .Include(r => r.Visitor)
            .Include(r => r.Artwork)
            .Include(r => r.Exhibition)
            .FirstOrDefaultAsync(r => r.Id == id);

        return review == null ? null : MapToDto(review);
    }

    public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto)
    {
        int visitorId;

        // If creating a new visitor with the review
        if (dto.VisitorId == null && dto.NewVisitor != null)
        {
            var visitor = new Visitor
            {
                Name = dto.NewVisitor.Name,
                Email = dto.NewVisitor.Email,
                Phone = dto.NewVisitor.Phone,
                MembershipType = dto.NewVisitor.MembershipType,
                JoinDate = DateTime.Today
            };
            await _visitorRepository.AddAsync(visitor);
            await _visitorRepository.SaveChangesAsync();
            visitorId = visitor.Id;
        }
        else if (dto.VisitorId.HasValue)
        {
            // Verify visitor exists
            var visitor = await _visitorRepository.GetByIdAsync(dto.VisitorId.Value)
                ?? throw new NotFoundException(nameof(Visitor), dto.VisitorId.Value);
            visitorId = visitor.Id;
        }
        else
        {
            throw new ValidationException("VisitorId", "Either VisitorId or NewVisitor must be provided");
        }

        var review = new GalleryReview
        {
            VisitorId = visitorId,
            ArtworkId = dto.ArtworkId,
            ExhibitionId = dto.ExhibitionId,
            Rating = dto.Rating,
            ReviewText = dto.ReviewText,
            ReviewDate = dto.ReviewDate
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();

        // Reload with navigation properties
        return (await GetByIdAsync(review.Id))!;
    }

    public async Task<ReviewResponseDto> UpdateAsync(int id, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.Query()
            .Include(r => r.Visitor)
            .Include(r => r.Artwork)
            .Include(r => r.Exhibition)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(GalleryReview), id);

        if (dto.ArtworkId.HasValue) review.ArtworkId = dto.ArtworkId;
        if (dto.ExhibitionId.HasValue) review.ExhibitionId = dto.ExhibitionId;
        if (dto.Rating.HasValue) review.Rating = dto.Rating.Value;
        if (dto.ReviewText != null) review.ReviewText = dto.ReviewText;
        if (dto.ReviewDate.HasValue) review.ReviewDate = dto.ReviewDate.Value;

        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        return MapToDto(review);
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(GalleryReview), id);

        _reviewRepository.Delete(review);
        await _reviewRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetByVisitorAsync(int visitorId)
    {
        var reviews = await _reviewRepository.Query()
            .Include(r => r.Visitor)
            .Include(r => r.Artwork)
            .Include(r => r.Exhibition)
            .Where(r => r.VisitorId == visitorId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();

        return reviews.Select(MapToDto);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetByArtworkAsync(int artworkId)
    {
        var reviews = await _reviewRepository.Query()
            .Include(r => r.Visitor)
            .Include(r => r.Artwork)
            .Include(r => r.Exhibition)
            .Where(r => r.ArtworkId == artworkId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();

        return reviews.Select(MapToDto);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetByExhibitionAsync(int exhibitionId)
    {
        var reviews = await _reviewRepository.Query()
            .Include(r => r.Visitor)
            .Include(r => r.Artwork)
            .Include(r => r.Exhibition)
            .Where(r => r.ExhibitionId == exhibitionId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();

        return reviews.Select(MapToDto);
    }

    public async Task<ReviewStatisticsDto> GetStatisticsAsync()
    {
        var reviews = await _reviewRepository.Query().ToListAsync();

        return new ReviewStatisticsDto
        {
            TotalReviews = reviews.Count,
            AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
            RatingDistribution = reviews
                .GroupBy(r => r.Rating)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private static ReviewResponseDto MapToDto(GalleryReview review)
    {
        return new ReviewResponseDto
        {
            Id = review.Id,
            VisitorId = review.VisitorId,
            VisitorName = review.Visitor?.Name ?? "Unknown",
            VisitorEmail = review.Visitor?.Email,
            ArtworkId = review.ArtworkId,
            ArtworkTitle = review.Artwork?.Title,
            ExhibitionId = review.ExhibitionId,
            ExhibitionTitle = review.Exhibition?.Title,
            Rating = review.Rating,
            ReviewText = review.ReviewText,
            ReviewDate = review.ReviewDate
        };
    }
}
