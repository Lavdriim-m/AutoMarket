using AutoMapper;
using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.Listings;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepo;
    private readonly IGenericRepository<ListingImage> _imageRepo;
    private readonly IMapper _mapper;

    public ListingService(
        IListingRepository listingRepo,
        IGenericRepository<ListingImage> imageRepo,
        IMapper mapper)
    {
        _listingRepo = listingRepo;
        _imageRepo = imageRepo;
        _mapper = mapper;
    }

    public async Task<PagedResult<ListingResponseDto>> GetListingsAsync(ListingQueryParams queryParams)
    {
        var paged = await _listingRepo.GetPagedAsync(queryParams);
        return new PagedResult<ListingResponseDto>
        {
            Data = _mapper.Map<IEnumerable<ListingResponseDto>>(paged.Data),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<ListingResponseDto> GetListingByIdAsync(int id)
    {
        var listing = await _listingRepo.GetWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"Listing {id} not found.");
        return _mapper.Map<ListingResponseDto>(listing);
    }

    public async Task<ListingResponseDto> CreateListingAsync(int sellerId, CreateListingDto dto)
    {
        var listing = _mapper.Map<Listing>(dto);
        listing.SellerId = sellerId;

        var created = await _listingRepo.AddAsync(listing);

        // Reload with navigation properties needed for the response DTO
        var detailed = await _listingRepo.GetWithDetailsAsync(created.Id)
            ?? throw new KeyNotFoundException($"Listing {created.Id} not found after creation.");
        return _mapper.Map<ListingResponseDto>(detailed);
    }

    public async Task<ListingResponseDto> UpdateListingAsync(
        int id, int currentUserId, string currentUserRole, UpdateListingDto dto)
    {
        var listing = await _listingRepo.GetWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"Listing {id} not found.");

        EnforceOwnership(listing.SellerId, currentUserId, currentUserRole);

        _mapper.Map(dto, listing);
        listing.UpdatedAt = DateTime.UtcNow;
        await _listingRepo.UpdateAsync(listing);

        return _mapper.Map<ListingResponseDto>(listing);
    }

    public async Task DeleteListingAsync(int id, int currentUserId, string currentUserRole)
    {
        var listing = await _listingRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Listing {id} not found.");

        EnforceOwnership(listing.SellerId, currentUserId, currentUserRole);

        listing.Status = Core.Enums.ListingStatus.Inactive;
        listing.UpdatedAt = DateTime.UtcNow;
        await _listingRepo.UpdateAsync(listing);
    }

    public async Task<PagedResult<ListingResponseDto>> GetMyListingsAsync(int sellerId, QueryParams queryParams)
    {
        var all = (await _listingRepo.GetBySellerIdAsync(sellerId)).ToList();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
            all = all.Where(l =>
                l.Title.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase) ||
                l.Description.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        all = queryParams.SortBy?.ToLowerInvariant() switch
        {
            "price" => queryParams.SortDescending
                ? all.OrderByDescending(l => l.Price).ToList()
                : all.OrderBy(l => l.Price).ToList(),
            "updatedat" => queryParams.SortDescending
                ? all.OrderByDescending(l => l.UpdatedAt).ToList()
                : all.OrderBy(l => l.UpdatedAt).ToList(),
            _ => all // repo already orders by CreatedAt desc
        };

        var totalCount = all.Count;
        var data = all
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToList();

        return new PagedResult<ListingResponseDto>
        {
            Data = _mapper.Map<List<ListingResponseDto>>(data),
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<ListingImageDto> AddListingImageAsync(
        int listingId, int currentUserId, AddListingImageDto dto)
    {
        var listing = await _listingRepo.GetByIdAsync(listingId)
            ?? throw new KeyNotFoundException($"Listing {listingId} not found.");

        EnforceOwnership(listing.SellerId, currentUserId, "Seller");

        var image = new ListingImage
        {
            ListingId = listingId,
            ImageUrl = dto.ImageUrl,
            IsPrimary = dto.IsPrimary
        };

        var created = await _imageRepo.AddAsync(image);
        return _mapper.Map<ListingImageDto>(created);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static void EnforceOwnership(int ownerId, int currentUserId, string currentUserRole)
    {
        if (currentUserRole == "Admin") return;
        if (ownerId != currentUserId)
            throw new UnauthorizedAccessException("You do not have permission to modify this listing.");
    }
}
