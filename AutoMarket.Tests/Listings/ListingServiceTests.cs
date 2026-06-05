using AutoMapper;
using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.Listings;
using Moq;

namespace AutoMarket.Tests.Listings;

public class ListingServiceTests
{
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IGenericRepository<ListingImage>> _imageRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ListingService _service;

    public ListingServiceTests()
    {
        _listingRepoMock = new Mock<IListingRepository>();
        _imageRepoMock   = new Mock<IGenericRepository<ListingImage>>();
        _mapperMock      = new Mock<IMapper>();
        _service = new ListingService(
            _listingRepoMock.Object, _imageRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetListingByIdAsync_WithValidId_ReturnsListingResponseDto()
    {
        var listing     = new Listing { Id = 1, Title = "Test Car", SellerId = 5 };
        var responseDto = new ListingResponseDto { Id = 1, Title = "Test Car" };

        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(listing);
        _mapperMock.Setup(m => m.Map<ListingResponseDto>(listing)).Returns(responseDto);

        var result = await _service.GetListingByIdAsync(1);

        Assert.Equal(responseDto.Title, result.Title);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetListingByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(99)).ReturnsAsync((Listing?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetListingByIdAsync(99));
    }

    [Fact]
    public async Task CreateListingAsync_WithValidDto_ReturnsCreatedListing()
    {
        var dto         = new CreateListingDto { Title = "New Car", VehicleId = 2, Price = 5000 };
        var listing     = new Listing { Id = 1, Title = "New Car", SellerId = 10, VehicleId = 2 };
        var responseDto = new ListingResponseDto { Id = 1, Title = "New Car" };

        _mapperMock.Setup(m => m.Map<Listing>(dto)).Returns(listing);
        _listingRepoMock.Setup(r => r.AddAsync(listing)).ReturnsAsync(listing);
        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(listing);
        _mapperMock.Setup(m => m.Map<ListingResponseDto>(listing)).Returns(responseDto);

        var result = await _service.CreateListingAsync(10, dto);

        Assert.Equal(1, result.Id);
        Assert.Equal("New Car", result.Title);
    }

    [Fact]
    public async Task UpdateListingAsync_WithValidOwner_UpdatesSuccessfully()
    {
        var listing     = new Listing { Id = 1, Title = "Old Title", SellerId = 10 };
        var dto         = new UpdateListingDto { Title = "New Title" };
        var responseDto = new ListingResponseDto { Id = 1, Title = "New Title" };

        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(listing);
        // Map(dto, listing) return value is discarded by the service — no setup needed.
        _mapperMock.Setup(m => m.Map<ListingResponseDto>(listing)).Returns(responseDto);

        var result = await _service.UpdateListingAsync(1, currentUserId: 10, "Seller", dto);

        Assert.Equal("New Title", result.Title);
        _listingRepoMock.Verify(r => r.UpdateAsync(listing), Times.Once);
    }

    [Fact]
    public async Task UpdateListingAsync_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var listing = new Listing { Id = 1, SellerId = 10 };
        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(listing);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateListingAsync(1, currentUserId: 99, "Seller", new UpdateListingDto()));
    }

    [Fact]
    public async Task DeleteListingAsync_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var listing = new Listing { Id = 1, SellerId = 10 };
        _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listing);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.DeleteListingAsync(1, currentUserId: 99, "Seller"));
    }

    [Fact]
    public async Task DeleteListingAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _listingRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Listing?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteListingAsync(99, 10, "Seller"));
    }

    [Fact]
    public async Task DeleteListingAsync_ByAdmin_SetsStatusInactiveRegardlessOfOwner()
    {
        var listing = new Listing { Id = 1, SellerId = 10, Status = ListingStatus.Active };
        _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listing);

        // Admin (different userId) must be allowed to delete any listing.
        await _service.DeleteListingAsync(1, currentUserId: 999, "Admin");

        Assert.Equal(ListingStatus.Inactive, listing.Status);
        _listingRepoMock.Verify(r => r.UpdateAsync(listing), Times.Once);
    }

    [Fact]
    public async Task AddListingImageAsync_ByNonOwner_ThrowsUnauthorizedAccessException()
    {
        var listing = new Listing { Id = 1, SellerId = 10 };
        _listingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(listing);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.AddListingImageAsync(1, currentUserId: 99,
                new AddListingImageDto { ImageUrl = "http://example.com/img.jpg" }));
    }
}
