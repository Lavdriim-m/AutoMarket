using System.ComponentModel.DataAnnotations;
using AutoMarket.Core.Enums;

namespace AutoMarket.Core.DTOs.Listings;

public class UpdateListingDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
    public decimal? Price { get; set; }

    public ListingStatus? Status { get; set; }
}
