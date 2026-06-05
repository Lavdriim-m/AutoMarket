using AutoMarket.Core.Enums;

namespace AutoMarket.Core.Entities;

public class ServiceRecord
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int MechanicId { get; set; }
    public ServiceType ServiceType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal Cost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Vehicle Vehicle { get; set; } = null!;
    public User Mechanic { get; set; } = null!;
}
