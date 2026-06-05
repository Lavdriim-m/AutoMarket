using AutoMapper;
using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Services.ServiceHistory;

public class ServiceHistoryService : IServiceHistoryService
{
    private readonly IServiceRecordRepository _serviceRecordRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMapper _mapper;

    public ServiceHistoryService(
        IServiceRecordRepository serviceRecordRepo,
        IUserRepository userRepo,
        IMapper mapper)
    {
        _serviceRecordRepo = serviceRecordRepo;
        _userRepo = userRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceRecordResponseDto>> GetByMechanicAsync(int mechanicId)
    {
        var records = await _serviceRecordRepo.GetByMechanicIdAsync(mechanicId);
        return _mapper.Map<IEnumerable<ServiceRecordResponseDto>>(records);
    }

    public async Task<IEnumerable<ServiceRecordResponseDto>> GetByVehicleIdAsync(int vehicleId)
    {
        var records = await _serviceRecordRepo.GetByVehicleIdAsync(vehicleId);
        return _mapper.Map<IEnumerable<ServiceRecordResponseDto>>(records);
    }

    public async Task<ServiceRecordResponseDto> GetByIdAsync(int id)
    {
        var record = await _serviceRecordRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Service record {id} not found.");

        await EnsureMechanicLoadedAsync(record);
        return _mapper.Map<ServiceRecordResponseDto>(record);
    }

    public async Task<ServiceRecordResponseDto> CreateAsync(int mechanicId, CreateServiceRecordDto dto)
    {
        var record = _mapper.Map<ServiceRecord>(dto);
        record.MechanicId = mechanicId;

        var created = await _serviceRecordRepo.AddAsync(record);

        var mechanic = await _userRepo.GetByIdAsync(mechanicId);
        created.Mechanic = mechanic!;

        return _mapper.Map<ServiceRecordResponseDto>(created);
    }

    public async Task<ServiceRecordResponseDto> UpdateAsync(
        int id, int currentUserId, string currentUserRole, UpdateServiceRecordDto dto)
    {
        var record = await _serviceRecordRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Service record {id} not found.");

        if (currentUserRole != "Admin" && record.MechanicId != currentUserId)
            throw new UnauthorizedAccessException("You can only update your own service records.");

        _mapper.Map(dto, record);
        await _serviceRecordRepo.UpdateAsync(record);

        await EnsureMechanicLoadedAsync(record);
        return _mapper.Map<ServiceRecordResponseDto>(record);
    }

    public async Task DeleteAsync(int id)
    {
        if (!await _serviceRecordRepo.ExistsAsync(id))
            throw new KeyNotFoundException($"Service record {id} not found.");
        await _serviceRecordRepo.DeleteAsync(id);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task EnsureMechanicLoadedAsync(ServiceRecord record)
    {
        if (record.Mechanic == null)
        {
            var mechanic = await _userRepo.GetByIdAsync(record.MechanicId);
            record.Mechanic = mechanic!;
        }
    }
}
