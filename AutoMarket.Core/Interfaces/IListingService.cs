using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.Interfaces;

public interface IListingService
{
    /// <summary>Returns a paginated, filterable list of listings.</summary>
    Task<PagedResult<ListingResponseDto>> GetListingsAsync(ListingQueryParams queryParams);

    /// <summary>Returns a single listing with full vehicle, image, and seller details.</summary>
    Task<ListingResponseDto> GetListingByIdAsync(int id);

    /// <summary>Creates a new listing for the given seller.</summary>
    Task<ListingResponseDto> CreateListingAsync(int sellerId, CreateListingDto dto);

    /// <summary>Updates a listing; enforces ownership or Admin role.</summary>
    Task<ListingResponseDto> UpdateListingAsync(int id, int currentUserId, string currentUserRole, UpdateListingDto dto);

    /// <summary>Marks a listing as Inactive; enforces ownership or Admin role.</summary>
    Task DeleteListingAsync(int id, int currentUserId, string currentUserRole);

    /// <summary>Returns the authenticated seller's own listings, paginated.</summary>
    Task<PagedResult<ListingResponseDto>> GetMyListingsAsync(int sellerId, QueryParams queryParams);

    /// <summary>Adds an image URL to a listing; enforces ownership.</summary>
    Task<ListingImageDto> AddListingImageAsync(int listingId, int currentUserId, AddListingImageDto dto);
}
