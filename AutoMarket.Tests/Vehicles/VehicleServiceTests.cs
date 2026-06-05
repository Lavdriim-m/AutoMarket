using AutoMapper;
using AutoMarket.Core.DTOs.Vehicles;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Services.Vehicles;
using Moq;

namespace AutoMarket.Tests.Vehicles;

public class VehicleServiceTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepoMock;
    private readonly Mock<IListingRepository> _listingRepoMock;
    private readonly Mock<IServiceRecordRepository> _serviceRecordRepoMock;
    private readonly Mock<INhtsaService> _nhtsaServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly VehicleService _service;

    public VehicleServiceTests()
    {
        _vehicleRepoMock       = new Mock<IVehicleRepository>();
        _listingRepoMock       = new Mock<IListingRepository>();
        _serviceRecordRepoMock = new Mock<IServiceRecordRepository>();
        _nhtsaServiceMock      = new Mock<INhtsaService>();
        _mapperMock            = new Mock<IMapper>();
        _service = new VehicleService(
            _vehicleRepoMock.Object,
            _listingRepoMock.Object,
            _serviceRecordRepoMock.Object,
            _nhtsaServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WithValidId_ReturnsVehicleResponseDto()
    {
        var vehicle     = new Vehicle { Id = 1, Make = "Toyota", Model = "Corolla", VIN = "ABC123" };
        var responseDto = new VehicleResponseDto { Id = 1, Make = "Toyota", Model = "Corolla" };

        _vehicleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);
        _mapperMock.Setup(m => m.Map<VehicleResponseDto>(vehicle)).Returns(responseDto);

        var result = await _service.GetVehicleByIdAsync(1);

        Assert.Equal("Toyota", result.Make);
        Assert.Equal("Corolla", result.Model);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Vehicle?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetVehicleByIdAsync(99));
    }

    [Fact]
    public async Task CreateVehicleAsync_WithValidDto_ReturnsCreatedVehicleResponseDto()
    {
        var dto         = new CreateVehicleDto { VIN = "1HGBH41JXMN109186", Make = "Honda" };
        var vehicle     = new Vehicle { Id = 1, VIN = dto.VIN, Make = dto.Make };
        var responseDto = new VehicleResponseDto { Id = 1, VIN = dto.VIN, Make = dto.Make };

        _vehicleRepoMock.Setup(r => r.GetByVinAsync(dto.VIN)).ReturnsAsync((Vehicle?)null);
        _mapperMock.Setup(m => m.Map<Vehicle>(dto)).Returns(vehicle);
        _vehicleRepoMock.Setup(r => r.AddAsync(vehicle)).ReturnsAsync(vehicle);
        _mapperMock.Setup(m => m.Map<VehicleResponseDto>(vehicle)).Returns(responseDto);

        var result = await _service.CreateVehicleAsync(dto);

        Assert.Equal(dto.VIN, result.VIN);
        Assert.Equal("Honda", result.Make);
    }

    [Fact]
    public async Task CreateVehicleAsync_WithDuplicateVin_ThrowsArgumentException()
    {
        var dto             = new CreateVehicleDto { VIN = "DUPLICATE123456789" };
        var existingVehicle = new Vehicle { Id = 5, VIN = dto.VIN };

        _vehicleRepoMock.Setup(r => r.GetByVinAsync(dto.VIN)).ReturnsAsync(existingVehicle);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateVehicleAsync(dto));
    }

    [Fact]
    public async Task UpdateVehicleAsync_ByNonOwnerSeller_ThrowsUnauthorizedAccessException()
    {
        var vehicle = new Vehicle { Id = 1, Make = "Ford" };

        _vehicleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);
        // Seller has no listings linked to vehicle 1.
        _listingRepoMock.Setup(r => r.GetBySellerIdAsync(55))
            .ReturnsAsync(new List<Listing>());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.UpdateVehicleAsync(1, currentUserId: 55, "Seller", new UpdateVehicleDto()));
    }

    [Fact]
    public async Task UpdateVehicleAsync_ByAdmin_UpdatesSuccessfully()
    {
        var vehicle     = new Vehicle { Id = 1, Make = "Ford" };
        var dto         = new UpdateVehicleDto { Make = "Chevrolet" };
        var responseDto = new VehicleResponseDto { Id = 1, Make = "Chevrolet" };

        _vehicleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);
        _mapperMock.Setup(m => m.Map<VehicleResponseDto>(vehicle)).Returns(responseDto);

        var result = await _service.UpdateVehicleAsync(1, currentUserId: 999, "Admin", dto);

        Assert.Equal("Chevrolet", result.Make);
        _vehicleRepoMock.Verify(r => r.UpdateAsync(vehicle), Times.Once);
    }

    [Fact]
    public async Task DecodeVinAsync_DelegatesToNhtsaService()
    {
        const string vin = "1HGBH41JXMN109186";
        var expected     = new VinDecodeResultDto { VIN = vin, Make = "Honda", IsValid = true };
        _nhtsaServiceMock.Setup(s => s.DecodeVinAsync(vin)).ReturnsAsync(expected);

        var result = await _service.DecodeVinAsync(vin);

        Assert.Equal("Honda", result.Make);
        Assert.True(result.IsValid);
        _nhtsaServiceMock.Verify(s => s.DecodeVinAsync(vin), Times.Once);
    }

    [Fact]
    public async Task GetServiceHistoryByVehicleAsync_WithInvalidVehicleId_ThrowsKeyNotFoundException()
    {
        _vehicleRepoMock.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetServiceHistoryByVehicleAsync(99));
    }
}
