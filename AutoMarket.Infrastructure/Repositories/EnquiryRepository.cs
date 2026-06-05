using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class EnquiryRepository : GenericRepository<Enquiry>, IEnquiryRepository
{
    public EnquiryRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Enquiry>> GetByBuyerIdAsync(int buyerId, QueryParams queryParams)
    {
        var query = _dbSet
            .Include(e => e.Buyer)
            .Include(e => e.Seller)
            .Include(e => e.Listing)
            .Where(e => e.BuyerId == buyerId)
            .OrderByDescending(e => e.CreatedAt);

        return await BuildPagedResultAsync(query, queryParams);
    }

    public async Task<PagedResult<Enquiry>> GetBySellerIdAsync(int sellerId, QueryParams queryParams)
    {
        var query = _dbSet
            .Include(e => e.Buyer)
            .Include(e => e.Seller)
            .Include(e => e.Listing)
            .Where(e => e.SellerId == sellerId)
            .OrderByDescending(e => e.CreatedAt);

        return await BuildPagedResultAsync(query, queryParams);
    }

    public async Task<IEnumerable<Enquiry>> GetByListingIdAsync(int listingId)
        => await _dbSet
            .Include(e => e.Buyer)
            .Include(e => e.Seller)
            .Where(e => e.ListingId == listingId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    private static async Task<PagedResult<Enquiry>> BuildPagedResultAsync(
        IQueryable<Enquiry> query, QueryParams queryParams)
    {
        var totalCount = await query.CountAsync();
        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PagedResult<Enquiry>
        {
            Data = data,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }
}
