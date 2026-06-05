using AutoMapper;
using AutoMarket.Core.DTOs.Watchlist;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.Watchlist;
using Moq;

namespace AutoMarket.Tests.Watchlist;

public class WatchlistServiceTests
{
    private readonly Mock<IWatchlistRepository> _watchlistRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly WatchlistService _service;

    public WatchlistServiceTests()
    {
        _watchlistRepoMock = new Mock<IWatchlistRepository>();
        _listingRepoMock   = new Mock<IListingRepository>();
        _mapperMock        = new Mock<IMapper>();
        _service = new WatchlistService(
            _watchlistRepoMock.Object, _listingRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetWatchlistAsync_ReturnsMappedItemsForBuyer()
    {
        var items = new List<WatchlistItem>
        {
            new() { Id = 1, BuyerId = 10, ListingId = 5 },
            new() { Id = 2, BuyerId = 10, ListingId = 6 }
        };
        var dtos = new List<WatchlistItemResponseDto>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        _watchlistRepoMock.Setup(r => r.GetByBuyerIdAsync(10)).ReturnsAsync(items);
        _mapperMock.Setup(m => m.Map<IEnumerable<WatchlistItemResponseDto>>(items)).Returns(dtos);

        var result = await _service.GetWatchlistAsync(10);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithValidDto_ReturnsWatchlistItemResponseDto()
    {
        const int buyerId    = 10;
        var dto              = new AddToWatchlistDto { ListingId = 5 };
        var listing          = new Listing { Id = 5, Title = "Car" };
        var item             = new WatchlistItem { Id = 1, BuyerId = buyerId, ListingId = 5 };
        var responseDto      = new WatchlistItemResponseDto { Id = 1, ListingTitle = "Car" };

        _watchlistRepoMock.Setup(r => r.ExistsByBuyerAndListingAsync(buyerId, dto.ListingId))
            .ReturnsAsync(false);
        _listingRepoMock.Setup(r => r.GetByIdAsync(dto.ListingId)).ReturnsAsync(listing);
        _watchlistRepoMock.Setup(r => r.AddAsync(It.IsAny<WatchlistItem>())).ReturnsAsync(item);
        _mapperMock.Setup(m => m.Map<WatchlistItemResponseDto>(item)).Returns(responseDto);

        var result = await _service.AddToWatchlistAsync(buyerId, dto);

        Assert.Equal(1, result.Id);
        Assert.Equal("Car", result.ListingTitle);
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithDuplicateEntry_ThrowsInvalidOperationException()
    {
        _watchlistRepoMock.Setup(r => r.ExistsByBuyerAndListingAsync(10, 5)).ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddToWatchlistAsync(10, new AddToWatchlistDto { ListingId = 5 }));
    }

    [Fact]
    public async Task AddToWatchlistAsync_WithNonExistentListing_ThrowsKeyNotFoundException()
    {
        var dto = new AddToWatchlistDto { ListingId = 999 };

        _watchlistRepoMock.Setup(r => r.ExistsByBuyerAndListingAsync(10, 999)).ReturnsAsync(false);
        _listingRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Listing?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddToWatchlistAsync(10, dto));
    }

    [Fact]
    public async Task RemoveFromWatchlistAsync_WithNonExistentItem_ThrowsKeyNotFoundException()
    {
        _watchlistRepoMock.Setup(r => r.GetByBuyerAndListingAsync(10, 5))
            .ReturnsAsync((WatchlistItem?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.RemoveFromWatchlistAsync(10, 5));
    }

    [Fact]
    public async Task IsInWatchlistAsync_WhenPresent_ReturnsTrue()
    {
        _watchlistRepoMock.Setup(r => r.ExistsByBuyerAndListingAsync(10, 5)).ReturnsAsync(true);

        var result = await _service.IsInWatchlistAsync(10, 5);

        Assert.True(result);
    }

    [Fact]
    public async Task ClearWatchlistAsync_CallsDeleteAllByBuyerId()
    {
        await _service.ClearWatchlistAsync(10);

        _watchlistRepoMock.Verify(r => r.DeleteAllByBuyerIdAsync(10), Times.Once);
    }
}
