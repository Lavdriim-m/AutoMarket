using AutoMapper;
using AutoMarket.Core.DTOs.ServiceHistory;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Enums;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.ServiceHistory;
using Moq;

namespace AutoMarket.Tests.ServiceHistory;

public class ServiceHistoryServiceTests
{
    private readonly Mock<IServiceRecordRepository> _serviceRecordRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ServiceHistoryService _service;

    public ServiceHistoryServiceTests()
    {
        _serviceRecordRepoMock = new Mock<IServiceRecordRepository>();
        _userRepoMock          = new Mock<IUserRepository>();
        _mapperMock            = new Mock<IMapper>();
        _service = new ServiceHistoryService(
            _serviceRecordRepoMock.Object, _userRepoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsServiceRecordResponseDto()
    {
        var mechanic    = new User { Id = 5, Username = "mech" };
        var record      = new ServiceRecord { Id = 1, MechanicId = 5, Mechanic = mechanic };
        var responseDto = new ServiceRecordResponseDto { Id = 1, MechanicUsername = "mech" };

        _serviceRecordRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(record);
        _mapperMock.Setup(m => m.Map<ServiceRecordResponseDto>(record)).Returns(responseDto);

        var result = await _service.GetByIdAsync(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("mech", result.MechanicUsername);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _serviceRecordRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ServiceRecord?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_SetsCorrectMechanicId()
    {
        const int mechanicId = 7;
        var dto      = new CreateServiceRecordDto { VehicleId = 2, ServiceType = ServiceType.OilChange, Cost = 50m };
        var record   = new ServiceRecord { Id = 1, MechanicId = mechanicId };
        var mechanic = new User { Id = mechanicId, Username = "wrench" };
        var responseDto = new ServiceRecordResponseDto { Id = 1, MechanicUsername = "wrench" };

        _mapperMock.Setup(m => m.Map<ServiceRecord>(dto)).Returns(record);
        _serviceRecordRepoMock.Setup(r => r.AddAsync(record)).ReturnsAsync(record);
        _userRepoMock.Setup(r => r.GetByIdAsync(mechanicId)).ReturnsAsync(mechanic);
        _mapperMock.Setup(m => m.Map<ServiceRecordResponseDto>(record)).Returns(responseDto);

        var result = await _service.CreateAsync(mechanicId, dto);

        Assert.Equal(mechanicId, record.MechanicId);
        Assert.Equal("wrench", result.MechanicUsername);
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwnerMechanic_ThrowsUnauthorizedAccessException()
    {
        var record = new ServiceRecord { Id = 1, MechanicId = 10 };
        _serviceRecordRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(record);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateAsync(1, currentUserId: 99, "Mechanic", new UpdateServiceRecordDto()));
    }

    [Fact]
    public async Task UpdateAsync_ByAdmin_UpdatesSuccessfully()
    {
        var mechanic    = new User { Id = 10, Username = "mech" };
        var record      = new ServiceRecord { Id = 1, MechanicId = 10, Mechanic = mechanic };
        var dto         = new UpdateServiceRecordDto { Cost = 200m };
        var responseDto = new ServiceRecordResponseDto { Id = 1 };

        _serviceRecordRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(record);
        _mapperMock.Setup(m => m.Map<ServiceRecordResponseDto>(record)).Returns(responseDto);

        // Admin with a different userId must be allowed to update any record.
        var result = await _service.UpdateAsync(1, currentUserId: 999, "Admin", dto);

        Assert.Equal(1, result.Id);
        _serviceRecordRepoMock.Verify(r => r.UpdateAsync(record), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _serviceRecordRepoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(99));
    }

    [Fact]
    public async Task GetByMechanicAsync_ReturnsMappedRecords()
    {
        var records = new List<ServiceRecord>
        {
            new() { Id = 1, MechanicId = 5 },
            new() { Id = 2, MechanicId = 5 }
        };
        var responseDtos = new List<ServiceRecordResponseDto>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        _serviceRecordRepoMock.Setup(r => r.GetByMechanicIdAsync(5)).ReturnsAsync(records);
        _mapperMock.Setup(m => m.Map<IEnumerable<ServiceRecordResponseDto>>(records))
            .Returns(responseDtos);

        var result = await _service.GetByMechanicAsync(5);

        Assert.Equal(2, result.Count());
    }
}
