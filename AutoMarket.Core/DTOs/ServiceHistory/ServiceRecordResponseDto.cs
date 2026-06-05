using AutoMarket.Core.Enums;

namespace AutoMarket.Core.DTOs.ServiceHistory;

public class ServiceRecordResponseDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int MechanicId { get; set; }
    public string MechanicUsername { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public decimal Cost { get; set; }
    public DateTime CreatedAt { get; set; }
}
