using AutoMarket.Core.DTOs.Reviews;

namespace AutoMarket.Core.Interfaces;

public interface IReviewService
{
    /// <summary>Returns all reviews for a listing plus the computed average rating.</summary>
    Task<ListingReviewSummaryDto> GetByListingIdAsync(int listingId);

    /// <summary>Returns a single review by id.</summary>
    Task<ReviewResponseDto> GetByIdAsync(int id);

    /// <summary>Submits a review; enforces one review per buyer per listing.</summary>
    Task<ReviewResponseDto> CreateReviewAsync(int reviewerId, CreateReviewDto dto);

    /// <summary>Updates the authenticated buyer's own review.</summary>
    Task<ReviewResponseDto> UpdateReviewAsync(int id, int reviewerId, UpdateReviewDto dto);

    /// <summary>Deletes a review; enforces ownership or Admin role.</summary>
    Task DeleteReviewAsync(int id, int currentUserId, string currentUserRole);

    /// <summary>Returns all reviews written by the current buyer.</summary>
    Task<IEnumerable<ReviewResponseDto>> GetMyReviewsAsync(int reviewerId);
}
