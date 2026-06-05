using AutoMarket.Core.Enums;

namespace AutoMarket.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Buyer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Enquiry> SentEnquiries { get; set; } = new List<Enquiry>();
    public ICollection<Enquiry> ReceivedEnquiries { get; set; } = new List<Enquiry>();
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>();
    public ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}
