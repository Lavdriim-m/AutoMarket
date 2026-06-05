using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Vehicles;

public class UpdateVehicleDto
{
    [MaxLength(50)]
    public string? Make { get; set; }

    [MaxLength(50)]
    public string? Model { get; set; }

    [Range(1900, 2100)]
    public int? Year { get; set; }

    [MaxLength(100)]
    public string? EngineType { get; set; }

    [MaxLength(50)]
    public string? FuelType { get; set; }

    [Range(0, int.MaxValue)]
    public int? Mileage { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }
}
