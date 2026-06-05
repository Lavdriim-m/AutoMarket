using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class ServiceRecordRepository : GenericRepository<ServiceRecord>, IServiceRecordRepository
{
    public ServiceRecordRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<ServiceRecord>> GetByVehicleIdAsync(int vehicleId)
        => await _dbSet
            .Include(sr => sr.Mechanic)
            .Where(sr => sr.VehicleId == vehicleId)
            .OrderByDescending(sr => sr.ServiceDate)
            .ToListAsync();

    public async Task<IEnumerable<ServiceRecord>> GetByMechanicIdAsync(int mechanicId)
        => await _dbSet
            .Include(sr => sr.Vehicle)
            .Include(sr => sr.Mechanic)
            .Where(sr => sr.MechanicId == mechanicId)
            .OrderByDescending(sr => sr.ServiceDate)
            .ToListAsync();
}
