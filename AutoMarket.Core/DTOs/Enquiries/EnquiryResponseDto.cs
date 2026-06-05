using AutoMarket.Core.Enums;

namespace AutoMarket.Core.DTOs.Enquiries;

public class EnquiryResponseDto
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public string BuyerUsername { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SellerReply { get; set; }
    public EnquiryStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
