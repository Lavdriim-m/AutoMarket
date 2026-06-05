namespace AutoMarket.Core.DTOs.Listings;

public class ListingImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
