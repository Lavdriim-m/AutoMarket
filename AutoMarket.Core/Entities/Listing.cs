using AutoMarket.Core.Enums;

namespace AutoMarket.Core.Entities;

public class Listing
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Active;
    public int SellerId { get; set; }
    public int VehicleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Seller { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public ICollection<ListingImage> Images { get; set; } = new List<ListingImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Enquiry> Enquiries { get; set; } = new List<Enquiry>();
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
}
