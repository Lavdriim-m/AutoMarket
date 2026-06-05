using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Listings;

public class AddListingImageDto
{
    [Required]
    [MaxLength(2048)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;
}
