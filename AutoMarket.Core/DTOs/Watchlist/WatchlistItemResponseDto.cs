namespace AutoMarket.Core.DTOs.Watchlist;

public class WatchlistItemResponseDto
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public decimal ListingPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
