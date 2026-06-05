namespace AutoMarket.Core.Entities;

public class WatchlistItem
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public int ListingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Buyer { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}
