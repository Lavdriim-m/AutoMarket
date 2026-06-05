using AutoMarket.Core.DTOs.Watchlist;

namespace AutoMarket.Core.Interfaces;

public interface IWatchlistService
{
    /// <summary>Returns all watchlist items for the given buyer.</summary>
    Task<IEnumerable<WatchlistItemResponseDto>> GetWatchlistAsync(int buyerId);

    /// <summary>Adds a listing to the buyer's watchlist; throws if already present.</summary>
    Task<WatchlistItemResponseDto> AddToWatchlistAsync(int buyerId, AddToWatchlistDto dto);

    /// <summary>Removes a specific listing from the buyer's watchlist.</summary>
    Task RemoveFromWatchlistAsync(int buyerId, int listingId);

    /// <summary>Returns true if the given listing is in the buyer's watchlist.</summary>
    Task<bool> IsInWatchlistAsync(int buyerId, int listingId);

    /// <summary>Removes all items from the buyer's watchlist.</summary>
    Task ClearWatchlistAsync(int buyerId);
}
