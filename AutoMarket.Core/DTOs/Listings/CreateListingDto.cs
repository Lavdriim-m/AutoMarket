using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Listings;

public class CreateListingDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
    public decimal Price { get; set; }

    [Required]
    public int VehicleId { get; set; }
}
