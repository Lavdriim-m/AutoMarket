using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Watchlist;

public class AddToWatchlistDto
{
    [Required]
    public int ListingId { get; set; }
}
