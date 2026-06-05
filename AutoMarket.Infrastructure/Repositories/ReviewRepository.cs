using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Review>> GetByListingIdAsync(int listingId)
        => await _dbSet
            .Include(r => r.Reviewer)
            .Where(r => r.ListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Review>> GetByReviewerIdAsync(int reviewerId)
        => await _dbSet
            .Include(r => r.Listing)
            .Where(r => r.ReviewerId == reviewerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<bool> ExistsByReviewerAndListingAsync(int reviewerId, int listingId)
        => await _dbSet.AnyAsync(r => r.ReviewerId == reviewerId && r.ListingId == listingId);

    public async Task<double> GetAverageRatingByListingIdAsync(int listingId)
    {
        var ratings = await _dbSet
            .Where(r => r.ListingId == listingId)
            .Select(r => r.Rating)
            .ToListAsync();

        return ratings.Count == 0 ? 0 : ratings.Average();
    }
}
