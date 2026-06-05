using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.DTOs.Vehicles;
using AutoMarket.Core.Helpers;

namespace AutoMarket.Core.Interfaces;

public interface IVehicleService
{
    /// <summary>Returns a paginated list of all vehicles (Admin only).</summary>
    Task<PagedResult<VehicleResponseDto>> GetAllVehiclesAsync(QueryParams queryParams);

    /// <summary>Returns a vehicle by id.</summary>
    Task<VehicleResponseDto> GetVehicleByIdAsync(int id);

    /// <summary>Creates a new vehicle record.</summary>
    Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto dto);

    /// <summary>Decodes a VIN via the NHTSA API without persisting to the database.</summary>
    Task<VinDecodeResultDto> DecodeVinAsync(string vin);

    /// <summary>Updates a vehicle; enforces ownership via linked listing or Admin role.</summary>
    Task<VehicleResponseDto> UpdateVehicleAsync(int id, int currentUserId, string currentUserRole, UpdateVehicleDto dto);

    /// <summary>Returns all service records for a vehicle.</summary>
    Task<IEnumerable<ServiceRecordResponseDto>> GetServiceHistoryByVehicleAsync(int vehicleId);
}
