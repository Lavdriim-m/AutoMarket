using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class ListingRepository : GenericRepository<Listing>, IListingRepository
{
    public ListingRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Listing>> GetPagedAsync(ListingQueryParams queryParams)
    {
        var query = _dbSet
            .Include(l => l.Seller)
            .Include(l => l.Vehicle)
            .Include(l => l.Images)
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(queryParams.Make))
            query = query.Where(l => l.Vehicle.Make.Contains(queryParams.Make));

        if (!string.IsNullOrWhiteSpace(queryParams.Model))
            query = query.Where(l => l.Vehicle.Model.Contains(queryParams.Model));

        if (queryParams.MinYear.HasValue)
            query = query.Where(l => l.Vehicle.Year >= queryParams.MinYear.Value);

        if (queryParams.MaxYear.HasValue)
            query = query.Where(l => l.Vehicle.Year <= queryParams.MaxYear.Value);

        if (queryParams.MinPrice.HasValue)
            query = query.Where(l => l.Price >= queryParams.MinPrice.Value);

        if (queryParams.MaxPrice.HasValue)
            query = query.Where(l => l.Price <= queryParams.MaxPrice.Value);

        if (queryParams.Status.HasValue)
            query = query.Where(l => l.Status == queryParams.Status.Value);

        if (!string.IsNullOrWhiteSpace(queryParams.FuelType))
            query = query.Where(l => l.Vehicle.FuelType.Contains(queryParams.FuelType));

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
            query = query.Where(l =>
                l.Title.Contains(queryParams.Search) ||
                l.Description.Contains(queryParams.Search));

        // Sorting
        query = queryParams.SortBy?.ToLowerInvariant() switch
        {
            "price" => queryParams.SortDescending
                ? query.OrderByDescending(l => l.Price)
                : query.OrderBy(l => l.Price),
            "updatedat" => queryParams.SortDescending
                ? query.OrderByDescending(l => l.UpdatedAt)
                : query.OrderBy(l => l.UpdatedAt),
            "year" => queryParams.SortDescending
                ? query.OrderByDescending(l => l.Vehicle.Year)
                : query.OrderBy(l => l.Vehicle.Year),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PagedResult<Listing>
        {
            Data = data,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<IEnumerable<Listing>> GetBySellerIdAsync(int sellerId)
        => await _dbSet
            .Include(l => l.Seller)
            .Include(l => l.Vehicle)
            .Include(l => l.Images)
            .Where(l => l.SellerId == sellerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    public async Task<Listing?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(l => l.Seller)
            .Include(l => l.Vehicle)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id);
}
