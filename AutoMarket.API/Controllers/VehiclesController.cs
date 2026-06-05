using System.Security.Claims;
using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.DTOs.Vehicles;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>Paginated list of all vehicles. Admin only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<VehicleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<VehicleResponseDto>>> GetVehicles(
        [FromQuery] QueryParams queryParams)
    {
        var result = await _vehicleService.GetAllVehiclesAsync(queryParams);
        return Ok(result);
    }

    /// <summary>
    /// Decode a VIN via the NHTSA vPIC API. Does not persist to the database.
    /// Seller only.
    /// </summary>
    // Defined before {id:int} so the literal segment "decode-vin" is never captured as an id.
    [HttpPost("decode-vin")]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(VinDecodeResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VinDecodeResultDto>> DecodeVin(
        [FromBody] DecodeVinRequestDto dto)
    {
        var result = await _vehicleService.DecodeVinAsync(dto.VIN);
        return Ok(result);
    }

    /// <summary>Get a vehicle by id. Authenticated users only.</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(VehicleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponseDto>> GetVehicle(int id)
    {
        var result = await _vehicleService.GetVehicleByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a vehicle record manually. Seller only.</summary>
    [HttpPost]
    [Authorize(Roles = "Seller")]
    [ProducesResponseType(typeof(VehicleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VehicleResponseDto>> CreateVehicle(
        [FromBody] CreateVehicleDto dto)
    {
        var result = await _vehicleService.CreateVehicleAsync(dto);
        return CreatedAtAction(nameof(GetVehicle), new { id = result.Id }, result);
    }

    /// <summary>Update a vehicle. Owner Seller (via linked listing) or Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Seller,Admin")]
    [ProducesResponseType(typeof(VehicleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleResponseDto>> UpdateVehicle(
        int id, [FromBody] UpdateVehicleDto dto)
    {
        var result = await _vehicleService.UpdateVehicleAsync(
            id, GetCurrentUserId(), GetCurrentUserRole(), dto);
        return Ok(result);
    }

    /// <summary>All service records for a vehicle. Public.</summary>
    [HttpGet("{id:int}/service-history")]
    [ProducesResponseType(typeof(IEnumerable<ServiceRecordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ServiceRecordResponseDto>>> GetServiceHistory(int id)
    {
        var result = await _vehicleService.GetServiceHistoryByVehicleAsync(id);
        return Ok(result);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found in token."));

    private string GetCurrentUserRole()
        => User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException("User role not found in token.");
}
