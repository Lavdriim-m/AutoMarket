using AutoMapper;
using AutoMarket.Core.DTOs.Reviews;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.Reviews;
using Moq;

namespace AutoMarket.Tests.Reviews;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ReviewService _service;

    public ReviewServiceTests()
    {
        _reviewRepoMock = new Mock<IReviewRepository>();
        _userRepoMock   = new Mock<IUserRepository>();
        _mapperMock     = new Mock<IMapper>();
        _service = new ReviewService(
            _reviewRepoMock.Object, _userRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsReviewResponseDto()
    {
        var reviewer    = new User { Id = 10, Username = "buyer1" };
        var review      = new Review { Id = 1, ReviewerId = 10, Rating = 5, Reviewer = reviewer };
        var responseDto = new ReviewResponseDto { Id = 1, Rating = 5, ReviewerUsername = "buyer1" };

        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        _mapperMock.Setup(m => m.Map<ReviewResponseDto>(review)).Returns(responseDto);

        var result = await _service.GetByIdAsync(1);

        Assert.Equal(5, result.Rating);
        Assert.Equal("buyer1", result.ReviewerUsername);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _reviewRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateReviewAsync_WithValidDto_ReturnsCreatedReviewResponseDto()
    {
        const int reviewerId = 10;
        var dto         = new CreateReviewDto { ListingId = 5, Rating = 4, Comment = "Good car" };
        var reviewer    = new User { Id = reviewerId, Username = "buyer1" };
        var review      = new Review { Id = 1, ReviewerId = reviewerId, ListingId = 5, Rating = 4 };
        var responseDto = new ReviewResponseDto { Id = 1, Rating = 4 };

        _reviewRepoMock.Setup(r => r.ExistsByReviewerAndListingAsync(reviewerId, dto.ListingId))
            .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.GetByIdAsync(reviewerId)).ReturnsAsync(reviewer);
        _mapperMock.Setup(m => m.Map<Review>(dto)).Returns(review);
        _reviewRepoMock.Setup(r => r.AddAsync(review)).ReturnsAsync(review);
        _mapperMock.Setup(m => m.Map<ReviewResponseDto>(review)).Returns(responseDto);

        var result = await _service.CreateReviewAsync(reviewerId, dto);

        Assert.Equal(1, result.Id);
        Assert.Equal(4, result.Rating);
    }

    [Fact]
    public async Task CreateReviewAsync_WhenReviewAlreadyExists_ThrowsInvalidOperationException()
    {
        _reviewRepoMock.Setup(r => r.ExistsByReviewerAndListingAsync(10, 5)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateReviewAsync(10, new CreateReviewDto { ListingId = 5, Rating = 3 }));
    }

    [Fact]
    public async Task UpdateReviewAsync_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var review = new Review { Id = 1, ReviewerId = 10 };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateReviewAsync(1, reviewerId: 99, new UpdateReviewDto { Rating = 1 }));
    }

    [Fact]
    public async Task UpdateReviewAsync_ByOwner_UpdatesSuccessfully()
    {
        var reviewer    = new User { Id = 10, Username = "buyer1" };
        var review      = new Review { Id = 1, ReviewerId = 10, Rating = 3, Reviewer = reviewer };
        var dto         = new UpdateReviewDto { Rating = 5, Comment = "Changed my mind!" };
        var responseDto = new ReviewResponseDto { Id = 1, Rating = 5 };

        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);
        _mapperMock.Setup(m => m.Map<ReviewResponseDto>(review)).Returns(responseDto);

        var result = await _service.UpdateReviewAsync(1, reviewerId: 10, dto);

        Assert.Equal(5, result.Rating);
        _reviewRepoMock.Verify(r => r.UpdateAsync(review), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_ByAdmin_DeletesWithoutOwnershipCheck()
    {
        var review = new Review { Id = 1, ReviewerId = 10 };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        // Admin (different userId) must bypass ownership check.
        await _service.DeleteReviewAsync(1, currentUserId: 999, "Admin");

        _reviewRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var review = new Review { Id = 1, ReviewerId = 10 };
        _reviewRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteReviewAsync(1, currentUserId: 99, "Buyer"));
    }

    [Fact]
    public async Task GetByListingIdAsync_ReturnsCorrectSummary()
    {
        var reviews = new List<Review>
        {
            new() { Id = 1, Rating = 4, ListingId = 5, Reviewer = new User { Username = "u1" } },
            new() { Id = 2, Rating = 2, ListingId = 5, Reviewer = new User { Username = "u2" } }
        };
        var reviewDtos = new List<ReviewResponseDto> { new() { Id = 1 }, new() { Id = 2 } };

        _reviewRepoMock.Setup(r => r.GetByListingIdAsync(5)).ReturnsAsync(reviews);
        _reviewRepoMock.Setup(r => r.GetAverageRatingByListingIdAsync(5)).ReturnsAsync(3.0);
        _mapperMock.Setup(m => m.Map<List<ReviewResponseDto>>(It.IsAny<object>())).Returns(reviewDtos);

        var result = await _service.GetByListingIdAsync(5);

        Assert.Equal(5, result.ListingId);
        Assert.Equal(3.0, result.AverageRating);
        Assert.Equal(2, result.TotalReviews);
    }
}
