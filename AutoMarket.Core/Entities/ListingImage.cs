namespace AutoMarket.Core.Entities;

public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Listing Listing { get; set; } = null!;
}
