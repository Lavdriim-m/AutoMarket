using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Enquiries;

public class ReplyEnquiryDto
{
    [Required]
    [MaxLength(1000)]
    public string Reply { get; set; } = string.Empty;
}
