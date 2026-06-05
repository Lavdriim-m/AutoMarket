using AutoMarket.Core.DTOs.Enquiries;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.Interfaces;

public interface IEnquiryService
{
    /// <summary>Returns the current user's enquiries — sent if Buyer, received if Seller.</summary>
    Task<PagedResult<EnquiryResponseDto>> GetEnquiriesAsync(int userId, string role, QueryParams queryParams);

    /// <summary>Returns a single enquiry; throws if the caller is not a participant.</summary>
    Task<EnquiryResponseDto> GetByIdAsync(int id, int currentUserId);

    /// <summary>Sends a new enquiry about a listing (Buyer only).</summary>
    Task<EnquiryResponseDto> CreateEnquiryAsync(int buyerId, CreateEnquiryDto dto);

    /// <summary>Adds the seller's reply to an enquiry; enforces that caller is the receiver.</summary>
    Task<EnquiryResponseDto> ReplyToEnquiryAsync(int id, int sellerId, ReplyEnquiryDto dto);

    /// <summary>Updates the status of an enquiry; enforces that caller is the receiver.</summary>
    Task<EnquiryResponseDto> UpdateEnquiryStatusAsync(int id, int sellerId, UpdateEnquiryStatusDto dto);

    /// <summary>Deletes an enquiry (Admin only).</summary>
    Task DeleteEnquiryAsync(int id);
}
