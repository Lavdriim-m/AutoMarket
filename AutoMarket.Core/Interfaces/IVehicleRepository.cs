using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IVehicleRepository : IGenericRepository<Vehicle>
{
    Task<Vehicle?> GetByVinAsync(string vin);
    Task<Vehicle?> GetWithServiceHistoryAsync(int id);
}
