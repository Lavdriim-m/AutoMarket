namespace AutoMarket.Core.Entities;

public class Review
{
    public int Id { get; set; }
    public int ReviewerId { get; set; }
    public int ListingId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Reviewer { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}
