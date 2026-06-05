using System.Security.Claims;
using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[ApiController]
[Route("api/service-history")]
public class ServiceHistoryController : ControllerBase
{
    private readonly IServiceHistoryService _serviceHistoryService;

    public ServiceHistoryController(IServiceHistoryService serviceHistoryService)
    {
        _serviceHistoryService = serviceHistoryService;
    }

    /// <summary>All service records logged by the currently authenticated mechanic.</summary>
    [HttpGet]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(IEnumerable<ServiceRecordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ServiceRecordResponseDto>>> GetMyRecords()
    {
        var result = await _serviceHistoryService.GetByMechanicAsync(GetCurrentUserId());
        return Ok(result);
    }

    /// <summary>All service records for a specific vehicle.</summary>
    // Defined before {id:int} to ensure the literal "vehicle" segment is matched first.
    [HttpGet("vehicle/{vehicleId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ServiceRecordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ServiceRecordResponseDto>>> GetByVehicle(int vehicleId)
    {
        var result = await _serviceHistoryService.GetByVehicleIdAsync(vehicleId);
        return Ok(result);
    }

    /// <summary>Get a single service record by id.</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceRecordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRecordResponseDto>> GetRecord(int id)
    {
        var result = await _serviceHistoryService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Log a new service record. Mechanic only.</summary>
    [HttpPost]
    [Authorize(Roles = "Mechanic")]
    [ProducesResponseType(typeof(ServiceRecordResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ServiceRecordResponseDto>> CreateRecord(
        [FromBody] CreateServiceRecordDto dto)
    {
        var result = await _serviceHistoryService.CreateAsync(GetCurrentUserId(), dto);
        return CreatedAtAction(nameof(GetRecord), new { id = result.Id }, result);
    }

    /// <summary>Update a service record. Record-owner Mechanic or Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Mechanic,Admin")]
    [ProducesResponseType(typeof(ServiceRecordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRecordResponseDto>> UpdateRecord(
        int id, [FromBody] UpdateServiceRecordDto dto)
    {
        var result = await _serviceHistoryService.UpdateAsync(
            id, GetCurrentUserId(), GetCurrentUserRole(), dto);
        return Ok(result);
    }

    /// <summary>Hard-delete a service record. Admin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        await _serviceHistoryService.DeleteAsync(id);
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
