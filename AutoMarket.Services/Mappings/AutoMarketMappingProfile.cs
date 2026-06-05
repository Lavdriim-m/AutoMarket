using AutoMapper;
using AutoMarket.Core.DTOs.Auth;
using AutoMarket.Core.DTOs.Enquiries;
using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.DTOs.Reviews;
using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.DTOs.Watchlist;
using AutoMarket.Core.DTOs.Vehicles;
using AutoMarket.Core.Entities;

namespace AutoMarket.Services.Mappings;

public class AutoMarketMappingProfile : Profile
{
    public AutoMarketMappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<RegisterRequestDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Vehicle mappings
        CreateMap<Vehicle, VehicleResponseDto>();
        CreateMap<CreateVehicleDto, Vehicle>();
        CreateMap<UpdateVehicleDto, Vehicle>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Listing mappings
        CreateMap<Listing, ListingResponseDto>()
            .ForMember(dest => dest.SellerUsername, opt => opt.MapFrom(src => src.Seller.Username))
            .ForMember(dest => dest.VehicleMake, opt => opt.MapFrom(src => src.Vehicle.Make))
            .ForMember(dest => dest.VehicleModel, opt => opt.MapFrom(src => src.Vehicle.Model))
            .ForMember(dest => dest.VehicleYear, opt => opt.MapFrom(src => src.Vehicle.Year));
        CreateMap<CreateListingDto, Listing>();
        CreateMap<UpdateListingDto, Listing>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ListingImage, ListingImageDto>();

        // ServiceRecord mappings
        CreateMap<ServiceRecord, ServiceRecordResponseDto>()
            .ForMember(dest => dest.MechanicUsername, opt => opt.MapFrom(src => src.Mechanic.Username));
        CreateMap<CreateServiceRecordDto, ServiceRecord>();
        CreateMap<UpdateServiceRecordDto, ServiceRecord>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // WatchlistItem mappings
        CreateMap<WatchlistItem, WatchlistItemResponseDto>()
            .ForMember(dest => dest.ListingTitle, opt => opt.MapFrom(src => src.Listing.Title))
            .ForMember(dest => dest.ListingPrice, opt => opt.MapFrom(src => src.Listing.Price));

        // Enquiry mappings
        CreateMap<Enquiry, EnquiryResponseDto>()
            .ForMember(dest => dest.BuyerUsername, opt => opt.MapFrom(src => src.Buyer.Username))
            .ForMember(dest => dest.SellerUsername, opt => opt.MapFrom(src => src.Seller.Username))
            .ForMember(dest => dest.ListingTitle, opt => opt.MapFrom(src => src.Listing.Title));
        CreateMap<CreateEnquiryDto, Enquiry>();

        // Review mappings
        CreateMap<Review, ReviewResponseDto>()
            .ForMember(dest => dest.ReviewerUsername, opt => opt.MapFrom(src => src.Reviewer.Username));
        CreateMap<CreateReviewDto, Review>();
        CreateMap<UpdateReviewDto, Review>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
