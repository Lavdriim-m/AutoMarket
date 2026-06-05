using AutoMapper;
using AutoMarket.Core.DTOs.Watchlist;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.Watchlist;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository _watchlistRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IMapper _mapper;

    public WatchlistService(
        IWatchlistRepository watchlistRepo,
        IListingRepository listingRepo,
        IMapper mapper)
    {
        _watchlistRepo = watchlistRepo;
        _listingRepo = listingRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WatchlistItemResponseDto>> GetWatchlistAsync(int buyerId)
    {
        var items = await _watchlistRepo.GetByBuyerIdAsync(buyerId);
        return _mapper.Map<IEnumerable<WatchlistItemResponseDto>>(items);
    }

    public async Task<WatchlistItemResponseDto> AddToWatchlistAsync(int buyerId, AddToWatchlistDto dto)
    {
        if (await _watchlistRepo.ExistsByBuyerAndListingAsync(buyerId, dto.ListingId))
            throw new InvalidOperationException("Listing is already in your watchlist.");

        var listing = await _listingRepo.GetByIdAsync(dto.ListingId)
            ?? throw new KeyNotFoundException($"Listing {dto.ListingId} not found.");

        var item = new WatchlistItem
        {
            BuyerId = buyerId,
            ListingId = dto.ListingId
        };

        var created = await _watchlistRepo.AddAsync(item);
        created.Listing = listing;

        return _mapper.Map<WatchlistItemResponseDto>(created);
    }

    public async Task RemoveFromWatchlistAsync(int buyerId, int listingId)
    {
        var item = await _watchlistRepo.GetByBuyerAndListingAsync(buyerId, listingId)
            ?? throw new KeyNotFoundException("Watchlist item not found.");
        await _watchlistRepo.DeleteAsync(item.Id);
    }

    public async Task<bool> IsInWatchlistAsync(int buyerId, int listingId)
        => await _watchlistRepo.ExistsByBuyerAndListingAsync(buyerId, listingId);

    public async Task ClearWatchlistAsync(int buyerId)
        => await _watchlistRepo.DeleteAllByBuyerIdAsync(buyerId);
}
