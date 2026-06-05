using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Reviews;

public class CreateReviewDto
{
    [Required]
    public int ListingId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;
}
