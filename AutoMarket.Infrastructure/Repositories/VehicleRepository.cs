using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(AppDbContext context) : base(context) { }

    public async Task<Vehicle?> GetByVinAsync(string vin)
        => await _dbSet.FirstOrDefaultAsync(v => v.VIN == vin);

    public async Task<Vehicle?> GetWithServiceHistoryAsync(int id)
        => await _dbSet
            .Include(v => v.ServiceRecords)
                .ThenInclude(sr => sr.Mechanic)
            .FirstOrDefaultAsync(v => v.Id == id);
}
