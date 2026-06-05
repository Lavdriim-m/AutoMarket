using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IEnumerable<Review>> GetByListingIdAsync(int listingId);
    Task<IEnumerable<Review>> GetByReviewerIdAsync(int reviewerId);
    Task<bool> ExistsByReviewerAndListingAsync(int reviewerId, int listingId);
    Task<double> GetAverageRatingByListingIdAsync(int listingId);
}
