using AutoMapper;
using AutoMarket.Core.DTOs.Reviews;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.Reviews;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;

    public ReviewService(IReviewRepository reviewRepo, IUserRepository userRepo, IMapper mapper)
    {
        _reviewRepo = reviewRepo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    public async Task<ListingReviewSummaryDto> GetByListingIdAsync(int listingId)
    {
        var reviews = await _reviewRepo.GetByListingIdAsync(listingId);
        var average = await _reviewRepo.GetAverageRatingByListingIdAsync(listingId);
        var list = reviews.ToList();

        return new ListingReviewSummaryDto
        {
            ListingId = listingId,
            AverageRating = average,
            TotalReviews = list.Count,
            Reviews = _mapper.Map<List<ReviewResponseDto>>(list)
        };
    }

    public async Task<ReviewResponseDto> GetByIdAsync(int id)
    {
        var review = await _reviewRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Review {id} not found.");

        await EnsureReviewerLoadedAsync(review);
        return _mapper.Map<ReviewResponseDto>(review);
    }

    public async Task<ReviewResponseDto> CreateReviewAsync(int reviewerId, CreateReviewDto dto)
    {
        if (await _reviewRepo.ExistsByReviewerAndListingAsync(reviewerId, dto.ListingId))
            throw new InvalidOperationException("You have already reviewed this listing.");

        var reviewer = await _userRepo.GetByIdAsync(reviewerId)
            ?? throw new KeyNotFoundException($"User {reviewerId} not found.");

        var review = _mapper.Map<Review>(dto);
        review.ReviewerId = reviewerId;

        var created = await _reviewRepo.AddAsync(review);
        created.Reviewer = reviewer;

        return _mapper.Map<ReviewResponseDto>(created);
    }

    public async Task<ReviewResponseDto> UpdateReviewAsync(int id, int reviewerId, UpdateReviewDto dto)
    {
        var review = await _reviewRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Review {id} not found.");

        if (review.ReviewerId != reviewerId)
            throw new UnauthorizedAccessException("You can only update your own reviews.");

        _mapper.Map(dto, review);
        await _reviewRepo.UpdateAsync(review);

        await EnsureReviewerLoadedAsync(review);
        return _mapper.Map<ReviewResponseDto>(review);
    }

    public async Task DeleteReviewAsync(int id, int currentUserId, string currentUserRole)
    {
        var review = await _reviewRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Review {id} not found.");

        if (currentUserRole != "Admin" && review.ReviewerId != currentUserId)
            throw new UnauthorizedAccessException("You can only delete your own reviews.");

        await _reviewRepo.DeleteAsync(id);
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetMyReviewsAsync(int reviewerId)
    {
        var reviews = await _reviewRepo.GetByReviewerIdAsync(reviewerId);
        return _mapper.Map<IEnumerable<ReviewResponseDto>>(reviews);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task EnsureReviewerLoadedAsync(Review review)
    {
        if (review.Reviewer == null)
        {
            var reviewer = await _userRepo.GetByIdAsync(review.ReviewerId);
            review.Reviewer = reviewer!;
        }
    }
}
