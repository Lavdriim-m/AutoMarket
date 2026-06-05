using AutoMapper;
using AutoMarket.Core.DTOs.Enquiries;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Enums;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.Enquiries;

public class EnquiryService : IEnquiryService
{
    private readonly IEnquiryRepository _enquiryRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;

    public EnquiryService(
        IEnquiryRepository enquiryRepo,
        IListingRepository listingRepo,
        IUserRepository userRepo,
        IMapper mapper)
    {
        _enquiryRepo = enquiryRepo;
        _listingRepo = listingRepo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    public async Task<PagedResult<EnquiryResponseDto>> GetEnquiriesAsync(
        int userId, string role, QueryParams queryParams)
    {
        PagedResult<Enquiry> paged;

        if (role == "Seller")
            paged = await _enquiryRepo.GetBySellerIdAsync(userId, queryParams);
        else
            paged = await _enquiryRepo.GetByBuyerIdAsync(userId, queryParams);

        return new PagedResult<EnquiryResponseDto>
        {
            Data = _mapper.Map<IEnumerable<EnquiryResponseDto>>(paged.Data),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<EnquiryResponseDto> GetByIdAsync(int id, int currentUserId)
    {
        var enquiry = await _enquiryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Enquiry {id} not found.");

        if (enquiry.BuyerId != currentUserId && enquiry.SellerId != currentUserId)
            throw new UnauthorizedAccessException("You are not a participant in this enquiry.");

        await LoadNavPropsAsync(enquiry);
        return _mapper.Map<EnquiryResponseDto>(enquiry);
    }

    public async Task<EnquiryResponseDto> CreateEnquiryAsync(int buyerId, CreateEnquiryDto dto)
    {
        var listing = await _listingRepo.GetWithDetailsAsync(dto.ListingId)
            ?? throw new KeyNotFoundException($"Listing {dto.ListingId} not found.");

        var buyer = await _userRepo.GetByIdAsync(buyerId)
            ?? throw new KeyNotFoundException($"User {buyerId} not found.");

        var enquiry = _mapper.Map<Enquiry>(dto);
        enquiry.BuyerId = buyerId;
        enquiry.SellerId = listing.SellerId;

        var created = await _enquiryRepo.AddAsync(enquiry);
        created.Buyer = buyer;
        created.Seller = listing.Seller;
        created.Listing = listing;

        return _mapper.Map<EnquiryResponseDto>(created);
    }

    public async Task<EnquiryResponseDto> ReplyToEnquiryAsync(int id, int sellerId, ReplyEnquiryDto dto)
    {
        var enquiry = await _enquiryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Enquiry {id} not found.");

        if (enquiry.SellerId != sellerId)
            throw new UnauthorizedAccessException("You can only reply to enquiries addressed to you.");

        enquiry.SellerReply = dto.Reply;
        enquiry.Status = EnquiryStatus.Replied;
        await _enquiryRepo.UpdateAsync(enquiry);

        await LoadNavPropsAsync(enquiry);
        return _mapper.Map<EnquiryResponseDto>(enquiry);
    }

    public async Task<EnquiryResponseDto> UpdateEnquiryStatusAsync(
        int id, int sellerId, UpdateEnquiryStatusDto dto)
    {
        var enquiry = await _enquiryRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Enquiry {id} not found.");

        if (enquiry.SellerId != sellerId)
            throw new UnauthorizedAccessException("You can only update status of enquiries addressed to you.");

        enquiry.Status = dto.Status;
        await _enquiryRepo.UpdateAsync(enquiry);

        await LoadNavPropsAsync(enquiry);
        return _mapper.Map<EnquiryResponseDto>(enquiry);
    }

    public async Task DeleteEnquiryAsync(int id)
    {
        if (!await _enquiryRepo.ExistsAsync(id))
            throw new KeyNotFoundException($"Enquiry {id} not found.");
        await _enquiryRepo.DeleteAsync(id);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task LoadNavPropsAsync(Enquiry enquiry)
    {
        enquiry.Buyer ??= (await _userRepo.GetByIdAsync(enquiry.BuyerId))!;
        enquiry.Seller ??= (await _userRepo.GetByIdAsync(enquiry.SellerId))!;
        enquiry.Listing ??= (await _listingRepo.GetByIdAsync(enquiry.ListingId))!;
    }
}
