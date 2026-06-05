using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IServiceRecordRepository : IGenericRepository<ServiceRecord>
{
    Task<IEnumerable<ServiceRecord>> GetByVehicleIdAsync(int vehicleId);
    Task<IEnumerable<ServiceRecord>> GetByMechanicIdAsync(int mechanicId);
}
