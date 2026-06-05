namespace AutoMarket.Core.DTOs.Reviews;

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int ReviewerId { get; set; }
    public string ReviewerUsername { get; set; } = string.Empty;
    public int ListingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
