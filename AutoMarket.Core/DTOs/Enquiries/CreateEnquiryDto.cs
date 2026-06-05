using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Enquiries;

public class CreateEnquiryDto
{
    [Required]
    public int ListingId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}
