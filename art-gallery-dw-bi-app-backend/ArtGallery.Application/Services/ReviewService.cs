using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
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
    private readonly IDataSourceContext _ds;
    private readonly IConfiguration _configuration;

    public ReviewService(
        IRepository<GalleryReview> reviewRepository,
        IRepository<Visitor> visitorRepository,
        IDataSourceContext ds,
        IConfiguration configuration)
    {
        _reviewRepository = reviewRepository;
        _visitorRepository = visitorRepository;
        _ds = ds;
        _configuration = configuration;
    }

    private bool IsGlobalSource()
    {
        return _ds.Source.ToString().Equals("GLOBAL", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PaginatedResponse<ReviewResponseDto>> GetAllAsync(PagedRequest request)
    {
        if (IsGlobalSource())
        {
            return await GetAllGlobalAsync(request);
        }

        var query = _reviewRepository.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();

            query = query.Where(r =>
                r.ReviewText != null &&
                r.ReviewText.ToLower().Contains(searchTerm));
        }

        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.IsDescending
                ? query.OrderByDescending(r => r.Rating)
                : query.OrderBy(r => r.Rating),

            "reviewdate" => request.IsDescending
                ? query.OrderByDescending(r => r.ReviewDate)
                : query.OrderBy(r => r.ReviewDate),

            _ => query.OrderByDescending(r => r.ReviewDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return PaginatedResponse<ReviewResponseDto>.Create(
            dtos,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<PaginatedResponse<ReviewResponseDto>> GetAllGlobalAsync(PagedRequest request)
    {
        var reviews = await GetGlobalReviewDtosAsync();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLowerInvariant();

            reviews = reviews
                .Where(r =>
                    (!string.IsNullOrWhiteSpace(r.ReviewText) &&
                     r.ReviewText.ToLowerInvariant().Contains(searchTerm)) ||
                    (!string.IsNullOrWhiteSpace(r.VisitorName) &&
                     r.VisitorName.ToLowerInvariant().Contains(searchTerm)) ||
                    (!string.IsNullOrWhiteSpace(r.ArtworkTitle) &&
                     r.ArtworkTitle.ToLowerInvariant().Contains(searchTerm)) ||
                    (!string.IsNullOrWhiteSpace(r.ExhibitionTitle) &&
                     r.ExhibitionTitle.ToLowerInvariant().Contains(searchTerm)))
                .ToList();
        }

        reviews = request.SortBy?.ToLowerInvariant() switch
        {
            "rating" => request.IsDescending
                ? reviews.OrderByDescending(r => r.Rating).ToList()
                : reviews.OrderBy(r => r.Rating).ToList(),

            "reviewdate" => request.IsDescending
                ? reviews.OrderByDescending(r => r.ReviewDate).ToList()
                : reviews.OrderBy(r => r.ReviewDate).ToList(),

            _ => reviews.OrderByDescending(r => r.ReviewDate).ToList()
        };

        var totalCount = reviews.Count;

        var pagedItems = reviews
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToList();

        return PaginatedResponse<ReviewResponseDto>.Create(
            pagedItems,
            totalCount,
            request.Page,
            request.PageSize);
    }

    private async Task<List<ReviewResponseDto>> GetGlobalReviewDtosAsync()
    {
        var connectionString = _configuration.GetConnectionString("BddGlobalConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new List<ReviewResponseDto>();
        }

        var reviews = new List<ReviewResponseDto>();

        await using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT
                r.REVIEW_ID,
                r.VISITOR_ID,
                r.ARTWORK_ID,
                r.EXHIBITION_ID,
                r.RATING,
                r.REVIEW_TEXT,
                r.REVIEW_DATE,
                r.SOURCE_REGION,

                v.NAME AS VISITOR_NAME,
                v.EMAIL AS VISITOR_EMAIL,

                ac.TITLE AS ARTWORK_TITLE,

                ex.TITLE AS EXHIBITION_TITLE
            FROM (
                SELECT
                    REVIEW_ID,
                    VISITOR_ID,
                    ARTWORK_ID,
                    EXHIBITION_ID,
                    RATING,
                    REVIEW_TEXT,
                    REVIEW_DATE,
                    'AM' AS SOURCE_REGION
                FROM GALLERY_REVIEW_AM@link_am

                UNION ALL

                SELECT
                    REVIEW_ID,
                    VISITOR_ID,
                    ARTWORK_ID,
                    EXHIBITION_ID,
                    RATING,
                    REVIEW_TEXT,
                    REVIEW_DATE,
                    'EU' AS SOURCE_REGION
                FROM GALLERY_REVIEW_EU@link_eu
            ) r
            LEFT JOIN VISITOR v
              ON v.VISITOR_ID = r.VISITOR_ID
            LEFT JOIN ARTWORK_CORE@link_eu ac
              ON ac.ARTWORK_ID = r.ARTWORK_ID
            LEFT JOIN (
                SELECT
                    EXHIBITION_ID,
                    TITLE,
                    'AM' AS SOURCE_REGION
                FROM EXHIBITION_AM@link_am

                UNION ALL

                SELECT
                    EXHIBITION_ID,
                    TITLE,
                    'EU' AS SOURCE_REGION
                FROM EXHIBITION_EU@link_eu
            ) ex
              ON ex.EXHIBITION_ID = r.EXHIBITION_ID
             AND ex.SOURCE_REGION = r.SOURCE_REGION
            ORDER BY r.REVIEW_DATE DESC, r.REVIEW_ID DESC";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var reviewId = Convert.ToInt32(reader["REVIEW_ID"]);

            var visitorId = reader["VISITOR_ID"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["VISITOR_ID"]);

            int? artworkId = reader["ARTWORK_ID"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["ARTWORK_ID"]);

            int? exhibitionId = reader["EXHIBITION_ID"] == DBNull.Value
                ? null
                : Convert.ToInt32(reader["EXHIBITION_ID"]);

            var review = new ReviewResponseDto
            {
                Id = reviewId,

                VisitorId = visitorId,
                VisitorName = reader["VISITOR_NAME"] == DBNull.Value
                    ? $"Visitor #{visitorId}"
                    : reader["VISITOR_NAME"]?.ToString() ?? $"Visitor #{visitorId}",
                VisitorEmail = reader["VISITOR_EMAIL"] == DBNull.Value
                    ? null
                    : reader["VISITOR_EMAIL"]?.ToString(),

                ArtworkId = artworkId,
                ArtworkTitle = artworkId == null
                    ? null
                    : reader["ARTWORK_TITLE"] == DBNull.Value
                        ? $"Artwork #{artworkId.Value}"
                        : reader["ARTWORK_TITLE"]?.ToString() ?? $"Artwork #{artworkId.Value}",

                ExhibitionId = exhibitionId,
                ExhibitionTitle = exhibitionId == null
                    ? null
                    : reader["EXHIBITION_TITLE"] == DBNull.Value
                        ? $"Exhibition #{exhibitionId.Value}"
                        : reader["EXHIBITION_TITLE"]?.ToString() ?? $"Exhibition #{exhibitionId.Value}",

                Rating = reader["RATING"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["RATING"]),

                ReviewText = reader["REVIEW_TEXT"] == DBNull.Value
                    ? null
                    : reader["REVIEW_TEXT"]?.ToString(),

                ReviewDate = reader["REVIEW_DATE"] == DBNull.Value
                    ? DateTime.MinValue
                    : Convert.ToDateTime(reader["REVIEW_DATE"])
            };

            reviews.Add(review);
        }

        return reviews;
    }

    public async Task<ReviewResponseDto?> GetByIdAsync(int id)
    {
        if (IsGlobalSource())
        {
            var reviews = await GetGlobalReviewDtosAsync();
            return reviews.FirstOrDefault(r => r.Id == id);
        }

        var review = await _reviewRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == id);

        return review == null ? null : MapToDto(review);
    }

    public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto)
    {
        int visitorId;

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
            visitorId = dto.VisitorId.Value;
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

        return MapToDto(review);
    }

    public async Task<ReviewResponseDto> UpdateAsync(int id, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.Query()
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
        var review = await _reviewRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(GalleryReview), id);

        _reviewRepository.Delete(review);
        await _reviewRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetByVisitorAsync(int visitorId)
    {
        if (IsGlobalSource())
        {
            var reviews = await GetGlobalReviewDtosAsync();

            return reviews
                .Where(r => r.VisitorId == visitorId)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();
        }

        var localReviews = await _reviewRepository.Query()
            .Where(r => r.VisitorId == visitorId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();

        return localReviews.Select(MapToDto);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetByArtworkAsync(int artworkId)
    {
        if (IsGlobalSource())
        {
            var reviews = await GetGlobalReviewDtosAsync();

            return reviews
                .Where(r => r.ArtworkId == artworkId)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();
        }

        var localReviews = await _reviewRepository.Query()
            .Where(r => r.ArtworkId == artworkId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();

        return localReviews.Select(MapToDto);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetByExhibitionAsync(int exhibitionId)
    {
        if (IsGlobalSource())
        {
            var reviews = await GetGlobalReviewDtosAsync();

            return reviews
                .Where(r => r.ExhibitionId == exhibitionId)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();
        }

        var localReviews = await _reviewRepository.Query()
            .Where(r => r.ExhibitionId == exhibitionId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();

        return localReviews.Select(MapToDto);
    }

    public async Task<ReviewStatisticsDto> GetStatisticsAsync()
    {
        if (IsGlobalSource())
        {
            var reviews = await GetGlobalReviewDtosAsync();

            return new ReviewStatisticsDto
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0,
                RatingDistribution = reviews
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        var localReviews = await _reviewRepository.Query().ToListAsync();

        return new ReviewStatisticsDto
        {
            TotalReviews = localReviews.Count,
            AverageRating = localReviews.Count > 0 ? localReviews.Average(r => r.Rating) : 0,
            RatingDistribution = localReviews
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
            VisitorName = review.Visitor?.Name ?? $"Visitor #{review.VisitorId}",
            VisitorEmail = review.Visitor?.Email,

            ArtworkId = review.ArtworkId,
            ArtworkTitle = review.Artwork?.Title ??
                (review.ArtworkId.HasValue ? $"Artwork #{review.ArtworkId}" : null),

            ExhibitionId = review.ExhibitionId,
            ExhibitionTitle = review.Exhibition?.Title ??
                (review.ExhibitionId.HasValue ? $"Exhibition #{review.ExhibitionId}" : null),

            Rating = review.Rating,
            ReviewText = review.ReviewText,
            ReviewDate = review.ReviewDate
        };
    }
}