using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IWatchlistRepository : IGenericRepository<WatchlistItem>
{
    Task<IEnumerable<WatchlistItem>> GetByBuyerIdAsync(int buyerId);
    Task<WatchlistItem?> GetByBuyerAndListingAsync(int buyerId, int listingId);
    Task DeleteAllByBuyerIdAsync(int buyerId);
    Task<bool> ExistsByBuyerAndListingAsync(int buyerId, int listingId);
}
