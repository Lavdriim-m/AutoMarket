using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.Interfaces;

public interface IEnquiryRepository : IGenericRepository<Enquiry>
{
    Task<PagedResult<Enquiry>> GetByBuyerIdAsync(int buyerId, QueryParams queryParams);
    Task<PagedResult<Enquiry>> GetBySellerIdAsync(int sellerId, QueryParams queryParams);
    Task<IEnumerable<Enquiry>> GetByListingIdAsync(int listingId);
}
