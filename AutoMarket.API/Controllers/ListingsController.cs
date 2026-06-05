using System.Security.Claims;
using AutoMarket.Core.DTOs.Listings;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    /// <summary>Paginated, filterable list of all active listings.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ListingResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ListingResponseDto>>> GetListings(
        [FromQuery] ListingQueryParams queryParams)
    {
        var result = await _listingService.GetListingsAsync(queryParams);
        return Ok(result);
    }

    /// <summary>Get the authenticated seller's own listings, paginated.</summary>
    // Defined before {id:int} so the literal segment "my" is never captured as an id.
    [HttpGet("my")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(PagedResult<ListingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ListingResponseDto>>> GetMyListings(
        [FromQuery] QueryParams queryParams)
    {
        var result = await _listingService.GetMyListingsAsync(GetCurrentUserId(), queryParams);
        return Ok(result);
    }

    /// <summary>Get a single listing with full Vehicle, Images and Seller details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ListingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingResponseDto>> GetListing(int id)
    {
        var result = await _listingService.GetListingByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a new listing. Seller only.</summary>
    [HttpPost]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(ListingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListingResponseDto>> CreateListing(
        [FromBody] CreateListingDto dto)
    {
        var result = await _listingService.CreateListingAsync(GetCurrentUserId(), dto);
        return CreatedAtAction(nameof(GetListing), new { id = result.Id }, result);
    }

    /// <summary>Update a listing. Owner Seller or Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Seller,Admin")]
    [ProducesResponseType(typeof(ListingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingResponseDto>> UpdateListing(
        int id, [FromBody] UpdateListingDto dto)
    {
        var result = await _listingService.UpdateListingAsync(
            id, GetCurrentUserId(), GetCurrentUserRole(), dto);
        return Ok(result);
    }

    /// <summary>Mark a listing as Inactive. Owner Seller or Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Seller,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteListing(int id)
    {
        await _listingService.DeleteListingAsync(id, GetCurrentUserId(), GetCurrentUserRole());
        return NoContent();
    }

    /// <summary>Add an image URL to a listing. Owner Seller only.</summary>
    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(ListingImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListingImageDto>> AddImage(
        int id, [FromBody] AddListingImageDto dto)
    {
        var result = await _listingService.AddListingImageAsync(id, GetCurrentUserId(), dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token."));

    private string GetCurrentUserRole()
        => User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("User role not found in token.");
}
