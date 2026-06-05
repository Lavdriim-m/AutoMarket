using System.Security.Claims;
using AutoMarket.Core.DTOs.Watchlist;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Buyer")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    /// <summary>All watchlist items for the currently authenticated buyer.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WatchlistItemResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<WatchlistItemResponseDto>>> GetWatchlist()
    {
        var result = await _watchlistService.GetWatchlistAsync(GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>Returns whether a listing is in the buyer's watchlist.</summary>
    // Defined before {listingId:int} to ensure the "check" literal is matched first.
    [HttpGet("check/{listingId:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<bool>> CheckWatchlist(int listingId)
    {
        var result = await _watchlistService.IsInWatchlistAsync(GetCurrentUserId(), listingId);
        return Ok(result);
    }

    /// <summary>Add a listing to the buyer's watchlist.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(WatchlistItemResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WatchlistItemResponseDto>> AddToWatchlist(
        [FromBody] AddToWatchlistDto dto)
    {
        try
        {
            var result = await _watchlistService.AddToWatchlistAsync(GetCurrentUserId(), dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Remove a specific listing from the buyer's watchlist.</summary>
    [HttpDelete("{listingId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWatchlist(int listingId)
    {
        await _watchlistService.RemoveFromWatchlistAsync(GetCurrentUserId(), listingId);
        return NoContent();
    }

    /// <summary>Remove all items from the buyer's watchlist.</summary>
    // Defined before {listingId:int} DELETE to clarify intent; :int constraint prevents collision anyway.
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ClearWatchlist()
    {
        await _watchlistService.ClearWatchlistAsync(GetCurrentUserId());
        return NoContent();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token."));
}
