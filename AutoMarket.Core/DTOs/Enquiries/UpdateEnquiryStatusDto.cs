using System.ComponentModel.DataAnnotations;
using AutoMarket.Core.Enums;

namespace AutoMarket.Core.DTOs.Enquiries;

public class UpdateEnquiryStatusDto
{
    [Required]
    public EnquiryStatus Status { get; set; }
}
