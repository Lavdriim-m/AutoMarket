using System.Security.Claims;
using AutoMarket.Core.DTOs.Reviews;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>All reviews for a listing plus the computed average rating.</summary>
    // Defined before {id:int} to ensure the "listing" literal segment is matched first.
    [HttpGet("listing/{listingId:int}")]
    [ProducesResponseType(typeof(ListingReviewSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingReviewSummaryDto>> GetByListing(int listingId)
    {
        var result = await _reviewService.GetByListingIdAsync(listingId);
        return Ok(result);
    }

    /// <summary>All reviews written by the currently authenticated buyer.</summary>
    // Defined before {id:int} to ensure the "my" literal segment is matched first.
    [HttpGet("my")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(IEnumerable<ReviewResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetMyReviews()
    {
        var result = await _reviewService.GetMyReviewsAsync(GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>Get a single review by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> GetReview(int id)
    {
        var result = await _reviewService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Submit a review for a listing. One review per buyer per listing. Buyer only.</summary>
    [HttpPost]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview(
        [FromBody] CreateReviewDto dto)
    {
        try
        {
            var result = await _reviewService.CreateReviewAsync(GetCurrentUserId(), dto);
            return CreatedAtAction(nameof(GetReview), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Update the authenticated buyer's own review.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(ReviewResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> UpdateReview(
        int id, [FromBody] UpdateReviewDto dto)
    {
        var result = await _reviewService.UpdateReviewAsync(id, GetCurrentUserId(), dto);
        return Ok(result);
    }

    /// <summary>Delete a review. Owner Buyer or Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Buyer,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(int id)
    {
        await _reviewService.DeleteReviewAsync(id, GetCurrentUserId(), GetCurrentUserRole());
        return NoContent();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token."));

    private string GetCurrentUserRole()
        => User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("User role not found in token.");
}
