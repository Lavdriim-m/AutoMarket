namespace AutoMarket.Core.DTOs.Vehicles;

public class VinDecodeResultDto
{
    public string VIN { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string EngineType { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string BodyClass { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorText { get; set; }
}
