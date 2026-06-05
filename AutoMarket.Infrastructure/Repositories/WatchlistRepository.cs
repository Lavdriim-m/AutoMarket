using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class WatchlistRepository : GenericRepository<WatchlistItem>, IWatchlistRepository
{
    public WatchlistRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<WatchlistItem>> GetByBuyerIdAsync(int buyerId)
        => await _dbSet
            .Include(wi => wi.Listing)
            .Where(wi => wi.BuyerId == buyerId)
            .OrderByDescending(wi => wi.CreatedAt)
            .ToListAsync();

    public async Task<WatchlistItem?> GetByBuyerAndListingAsync(int buyerId, int listingId)
        => await _dbSet.FirstOrDefaultAsync(wi => wi.BuyerId == buyerId && wi.ListingId == listingId);

    public async Task DeleteAllByBuyerIdAsync(int buyerId)
    {
        var items = await _dbSet.Where(wi => wi.BuyerId == buyerId).ToListAsync();
        _dbSet.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByBuyerAndListingAsync(int buyerId, int listingId)
        => await _dbSet.AnyAsync(wi => wi.BuyerId == buyerId && wi.ListingId == listingId);
}
