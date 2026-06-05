using AutoMarket.Core.DTOs.ServiceHistory;

namespace AutoMarket.Core.Interfaces;

public interface IServiceHistoryService
{
    /// <summary>Returns all service records logged by the given mechanic.</summary>
    Task<IEnumerable<ServiceRecordResponseDto>> GetByMechanicAsync(int mechanicId);

    /// <summary>Returns all service records for a vehicle.</summary>
    Task<IEnumerable<ServiceRecordResponseDto>> GetByVehicleIdAsync(int vehicleId);

    /// <summary>Returns a single service record by id.</summary>
    Task<ServiceRecordResponseDto> GetByIdAsync(int id);

    /// <summary>Logs a new service record for the given mechanic.</summary>
    Task<ServiceRecordResponseDto> CreateAsync(int mechanicId, CreateServiceRecordDto dto);

    /// <summary>Updates a service record; enforces mechanic ownership or Admin role.</summary>
    Task<ServiceRecordResponseDto> UpdateAsync(int id, int currentUserId, string currentUserRole, UpdateServiceRecordDto dto);

    /// <summary>Hard-deletes a service record (Admin only).</summary>
    Task DeleteAsync(int id);
}
