using AutoMapper;
using AutoMarket.Core.DTOs.Enquiries;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Enums;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.Enquiries;
using Moq;

namespace AutoMarket.Tests.Enquiries;

public class EnquiryServiceTests
{
    private readonly Mock<IEnquiryRepository> _enquiryRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly EnquiryService _service;

    public EnquiryServiceTests()
    {
        _enquiryRepoMock = new Mock<IEnquiryRepository>();
        _listingRepoMock = new Mock<IListingRepository>();
        _userRepoMock    = new Mock<IUserRepository>();
        _mapperMock      = new Mock<IMapper>();
        _service = new EnquiryService(
            _enquiryRepoMock.Object,
            _listingRepoMock.Object,
            _userRepoMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithBuyerAsParticipant_ReturnsEnquiryResponseDto()
    {
        // Pre-populate nav props so LoadNavPropsAsync does not call repos.
        var buyer = new User { Id = 10, Username = "buyer" };
        var seller = new User { Id = 20, Username = "seller" };
        var listing = new Listing { Id = 5, Title = "Car" };
        var enquiry = new Enquiry
        {
            Id = 1, BuyerId = 10, SellerId = 20, ListingId = 5,
            Buyer = buyer, Seller = seller, Listing = listing
        };
        var responseDto = new EnquiryResponseDto { Id = 1 };

        _enquiryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(enquiry);
        _mapperMock.Setup(m => m.Map<EnquiryResponseDto>(enquiry)).Returns(responseDto);

        var result = await _service.GetByIdAsync(1, currentUserId: 10);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonParticipant_ThrowsUnauthorizedAccessException()
    {
        var enquiry = new Enquiry { Id = 1, BuyerId = 10, SellerId = 20 };
        _enquiryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(enquiry);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.GetByIdAsync(1, currentUserId: 99));
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _enquiryRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Enquiry?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(99, currentUserId: 10));
    }

    [Fact]
    public async Task CreateEnquiryAsync_WithValidDto_SetsSellerIdFromListing()
    {
        const int buyerId   = 10;
        const int sellerId  = 20;
        var dto     = new CreateEnquiryDto { ListingId = 5, Message = "Is it available?" };
        var seller  = new User { Id = sellerId, Username = "seller" };
        var listing = new Listing { Id = 5, Title = "Car", SellerId = sellerId, Seller = seller };
        var buyer   = new User { Id = buyerId, Username = "buyer" };
        var enquiry = new Enquiry { Id = 1, BuyerId = buyerId, SellerId = sellerId, ListingId = 5 };
        var responseDto = new EnquiryResponseDto { Id = 1 };

        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(dto.ListingId)).ReturnsAsync(listing);
        _userRepoMock.Setup(r => r.GetByIdAsync(buyerId)).ReturnsAsync(buyer);
        _mapperMock.Setup(m => m.Map<Enquiry>(dto)).Returns(enquiry);
        _enquiryRepoMock.Setup(r => r.AddAsync(enquiry)).ReturnsAsync(enquiry);
        _mapperMock.Setup(m => m.Map<EnquiryResponseDto>(enquiry)).Returns(responseDto);

        var result = await _service.CreateEnquiryAsync(buyerId, dto);

        Assert.Equal(1, result.Id);
        Assert.Equal(sellerId, enquiry.SellerId);
    }

    [Fact]
    public async Task CreateEnquiryAsync_WithNonExistentListing_ThrowsKeyNotFoundException()
    {
        _listingRepoMock.Setup(r => r.GetWithDetailsAsync(999)).ReturnsAsync((Listing?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateEnquiryAsync(10, new CreateEnquiryDto { ListingId = 999 }));
    }

    [Fact]
    public async Task ReplyToEnquiryAsync_BySeller_SetsReplyAndStatusReplied()
    {
        var seller  = new User { Id = 20, Username = "seller" };
        var buyer   = new User { Id = 10, Username = "buyer" };
        var listing = new Listing { Id = 5, Title = "Car" };
        var enquiry = new Enquiry
        {
            Id = 1, BuyerId = 10, SellerId = 20,
            Buyer = buyer, Seller = seller, Listing = listing,
            Status = EnquiryStatus.Open
        };
        var dto         = new ReplyEnquiryDto { Reply = "Yes, still available." };
        var responseDto = new EnquiryResponseDto { Id = 1 };

        _enquiryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(enquiry);
        _mapperMock.Setup(m => m.Map<EnquiryResponseDto>(enquiry)).Returns(responseDto);

        await _service.ReplyToEnquiryAsync(1, sellerId: 20, dto);

        Assert.Equal(dto.Reply, enquiry.SellerReply);
        Assert.Equal(EnquiryStatus.Replied, enquiry.Status);
        _enquiryRepoMock.Verify(r => r.UpdateAsync(enquiry), Times.Once);
    }

    [Fact]
    public async Task ReplyToEnquiryAsync_ByWrongSeller_ThrowsUnauthorizedAccessException()
    {
        var enquiry = new Enquiry { Id = 1, BuyerId = 10, SellerId = 20 };
        _enquiryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(enquiry);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.ReplyToEnquiryAsync(1, sellerId: 99,
                new ReplyEnquiryDto { Reply = "Hack" }));
    }

    [Fact]
    public async Task GetEnquiriesAsync_ForBuyerRole_CallsGetByBuyerIdAsync()
    {
        var paged = new PagedResult<Enquiry> { Data = new List<Enquiry>(), TotalCount = 0, Page = 1, PageSize = 10 };
        var queryParams = new QueryParams();

        _enquiryRepoMock.Setup(r => r.GetByBuyerIdAsync(10, queryParams)).ReturnsAsync(paged);
        _mapperMock.Setup(m => m.Map<IEnumerable<EnquiryResponseDto>>(It.IsAny<object>()))
            .Returns(new List<EnquiryResponseDto>());

        var result = await _service.GetEnquiriesAsync(10, "Buyer", queryParams);

        _enquiryRepoMock.Verify(r => r.GetByBuyerIdAsync(10, queryParams), Times.Once);
        _enquiryRepoMock.Verify(r => r.GetBySellerIdAsync(It.IsAny<int>(), It.IsAny<QueryParams>()), Times.Never);
    }
}
