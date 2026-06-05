using System.Security.Claims;
using AutoMarket.Core.DTOs.Enquiries;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnquiriesController : ControllerBase
{
    private readonly IEnquiryService _enquiryService;

    public EnquiriesController(IEnquiryService enquiryService)
    {
        _enquiryService = enquiryService;
    }

    /// <summary>
    /// Current user's enquiries — sent if Buyer, received if Seller.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EnquiryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<EnquiryResponseDto>>> GetEnquiries(
        [FromQuery] QueryParams queryParams)
    {
        var result = await _enquiryService.GetEnquiriesAsync(
            GetCurrentUserId(), GetCurrentUserRole(), queryParams);
        return Ok(result);
    }

    /// <summary>Get a single enquiry. Only the buyer or seller participant may access it.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EnquiryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnquiryResponseDto>> GetEnquiry(int id)
    {
        var result = await _enquiryService.GetByIdAsync(id, GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>Send an enquiry about a listing. Buyer only.</summary>
    [HttpPost]
    [Authorize(Roles = "Buyer")]
    [ProducesResponseType(typeof(EnquiryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EnquiryResponseDto>> CreateEnquiry(
        [FromBody] CreateEnquiryDto dto)
    {
        var result = await _enquiryService.CreateEnquiryAsync(GetCurrentUserId(), dto);
        return CreatedAtAction(nameof(GetEnquiry), new { id = result.Id }, result);
    }

    /// <summary>Add a reply to an enquiry. Seller (receiver) only.</summary>
    [HttpPut("{id:int}/reply")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(EnquiryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnquiryResponseDto>> ReplyToEnquiry(
        int id, [FromBody] ReplyEnquiryDto dto)
    {
        var result = await _enquiryService.ReplyToEnquiryAsync(id, GetCurrentUserId(), dto);
        return Ok(result);
    }

    /// <summary>Update the status of an enquiry. Seller (receiver) only.</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(EnquiryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnquiryResponseDto>> UpdateEnquiryStatus(
        int id, [FromBody] UpdateEnquiryStatusDto dto)
    {
        var result = await _enquiryService.UpdateEnquiryStatusAsync(id, GetCurrentUserId(), dto);
        return Ok(result);
    }

    /// <summary>Delete an enquiry. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEnquiry(int id)
    {
        await _enquiryService.DeleteEnquiryAsync(id);
        return NoContent();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token."));

    private string GetCurrentUserRole()
        => User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("User role not found in token.");
}
