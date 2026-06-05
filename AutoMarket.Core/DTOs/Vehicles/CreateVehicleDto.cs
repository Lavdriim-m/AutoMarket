using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Vehicles;

public class CreateVehicleDto
{
    [Required]
    [MaxLength(17)]
    public string VIN { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2100)]
    public int Year { get; set; }

    [MaxLength(100)]
    public string EngineType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string FuelType { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Mileage { get; set; }

    [MaxLength(50)]
    public string Color { get; set; } = string.Empty;
}
