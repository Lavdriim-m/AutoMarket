using AutoMarket.Core.Enums;

namespace AutoMarket.Core.Entities;

public class Enquiry
{
    public int Id { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public int ListingId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SellerReply { get; set; }
    public EnquiryStatus Status { get; set; } = EnquiryStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public Listing Listing { get; set; } = null!;
}
