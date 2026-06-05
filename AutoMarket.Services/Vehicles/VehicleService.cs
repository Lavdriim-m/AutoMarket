using AutoMapper;
using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.DTOs.Vehicles;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Helpers;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.Vehicles;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IListingRepository _listingRepo;
    private readonly IServiceRecordRepository _serviceRecordRepo;
    private readonly INhtsaService _nhtsaService;
    private readonly IMapper _mapper;

    public VehicleService(
        IVehicleRepository vehicleRepo,
        IListingRepository listingRepo,
        IServiceRecordRepository serviceRecordRepo,
        INhtsaService nhtsaService,
        IMapper mapper)
    {
        _vehicleRepo = vehicleRepo;
        _listingRepo = listingRepo;
        _serviceRecordRepo = serviceRecordRepo;
        _nhtsaService = nhtsaService;
        _mapper = mapper;
    }

    public async Task<PagedResult<VehicleResponseDto>> GetAllVehiclesAsync(QueryParams queryParams)
    {
        var all = (await _vehicleRepo.GetAllAsync()).ToList();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
            all = all.Where(v =>
                v.Make.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase) ||
                v.Model.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase) ||
                v.VIN.Contains(queryParams.Search, StringComparison.OrdinalIgnoreCase))
                .ToList();

        all = queryParams.SortDescending
            ? all.OrderByDescending(v => v.CreatedAt).ToList()
            : all.OrderBy(v => v.CreatedAt).ToList();

        var totalCount = all.Count;
        var data = all
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToList();

        return new PagedResult<VehicleResponseDto>
        {
            Data = _mapper.Map<List<VehicleResponseDto>>(data),
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<VehicleResponseDto> GetVehicleByIdAsync(int id)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");
        return _mapper.Map<VehicleResponseDto>(vehicle);
    }

    public async Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleDto dto)
    {
        if (await _vehicleRepo.GetByVinAsync(dto.VIN) != null)
            throw new ArgumentException($"A vehicle with VIN '{dto.VIN}' already exists.");

        var vehicle = _mapper.Map<Vehicle>(dto);
        var created = await _vehicleRepo.AddAsync(vehicle);
        return _mapper.Map<VehicleResponseDto>(created);
    }

    public async Task<VinDecodeResultDto> DecodeVinAsync(string vin)
        => await _nhtsaService.DecodeVinAsync(vin);

    public async Task<VehicleResponseDto> UpdateVehicleAsync(
        int id, int currentUserId, string currentUserRole, UpdateVehicleDto dto)
    {
        var vehicle = await _vehicleRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");

        if (currentUserRole != "Admin")
        {
            var sellerListings = await _listingRepo.GetBySellerIdAsync(currentUserId);
            if (!sellerListings.Any(l => l.VehicleId == id))
                throw new UnauthorizedAccessException("You do not own a listing for this vehicle.");
        }

        _mapper.Map(dto, vehicle);
        await _vehicleRepo.UpdateAsync(vehicle);
        return _mapper.Map<VehicleResponseDto>(vehicle);
    }

    public async Task<IEnumerable<ServiceRecordResponseDto>> GetServiceHistoryByVehicleAsync(int vehicleId)
    {
        if (!await _vehicleRepo.ExistsAsync(vehicleId))
            throw new KeyNotFoundException($"Vehicle {vehicleId} not found.");

        var records = await _serviceRecordRepo.GetByVehicleIdAsync(vehicleId);
        return _mapper.Map<IEnumerable<ServiceRecordResponseDto>>(records);
    }
}
