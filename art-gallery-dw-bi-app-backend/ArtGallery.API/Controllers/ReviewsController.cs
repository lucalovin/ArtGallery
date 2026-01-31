using Microsoft.AspNetCore.Mvc;
using ArtGallery.Application.DTOs.Common;
using ArtGallery.Application.DTOs.Review;
using ArtGallery.Application.Interfaces;

namespace ArtGallery.API.Controllers;

/// <summary>
/// Controller for managing gallery reviews.
/// Reviews are the primary entity - visitors are created/selected as part of reviews.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Get all reviews with pagination.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ReviewResponseDto>>>> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _reviewService.GetAllAsync(request);
        return Ok(ApiResponse<PaginatedResponse<ReviewResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get a review by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> GetById(int id)
    {
        var result = await _reviewService.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<ReviewResponseDto>.FailureResponse($"Review with ID {id} not found"));
        
        return Ok(ApiResponse<ReviewResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Create a new review. Can create a new visitor inline or use an existing visitor.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> Create([FromBody] CreateReviewDto dto)
    {
        var result = await _reviewService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            ApiResponse<ReviewResponseDto>.SuccessResponse(result, "Review created successfully"));
    }

    /// <summary>
    /// Update an existing review.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> Update(int id, [FromBody] UpdateReviewDto dto)
    {
        var result = await _reviewService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ReviewResponseDto>.SuccessResponse(result, "Review updated successfully"));
    }

    /// <summary>
    /// Delete a review.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _reviewService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Get all reviews by a specific visitor.
    /// </summary>
    [HttpGet("by-visitor/{visitorId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponseDto>>>> GetByVisitor(int visitorId)
    {
        var result = await _reviewService.GetByVisitorAsync(visitorId);
        return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all reviews for a specific artwork.
    /// </summary>
    [HttpGet("by-artwork/{artworkId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponseDto>>>> GetByArtwork(int artworkId)
    {
        var result = await _reviewService.GetByArtworkAsync(artworkId);
        return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get all reviews for a specific exhibition.
    /// </summary>
    [HttpGet("by-exhibition/{exhibitionId:int}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponseDto>>>> GetByExhibition(int exhibitionId)
    {
        var result = await _reviewService.GetByExhibitionAsync(exhibitionId);
        return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Get review statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<ReviewStatisticsDto>>> GetStatistics()
    {
        var result = await _reviewService.GetStatisticsAsync();
        return Ok(ApiResponse<ReviewStatisticsDto>.SuccessResponse(result));
    }
}
