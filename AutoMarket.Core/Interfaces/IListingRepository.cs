using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.Interfaces;

public interface IListingRepository : IGenericRepository<Listing>
{
    Task<PagedResult<Listing>> GetPagedAsync(ListingQueryParams queryParams);
    Task<IEnumerable<Listing>> GetBySellerIdAsync(int sellerId);
    Task<Listing?> GetWithDetailsAsync(int id);
}
