namespace AutoMarket.Core.DTOs.Reviews;

public class ListingReviewSummaryDto
{
    public int ListingId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewResponseDto> Reviews { get; set; } = new();
}
