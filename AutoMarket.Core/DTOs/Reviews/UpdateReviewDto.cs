using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Reviews;

public class UpdateReviewDto
{
    [Range(1, 5)]
    public int? Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}
