using AutoMarket.Core.Enums;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.DTOs.Listings;

public class ListingQueryParams : QueryParams
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ListingStatus? Status { get; set; }
    public string? FuelType { get; set; }
}
