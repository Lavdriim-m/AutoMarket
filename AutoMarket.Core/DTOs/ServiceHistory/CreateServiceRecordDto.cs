using System.ComponentModel.DataAnnotations;
using AutoMarket.Core.Enums;

namespace AutoMarket.Core.DTOs.ServiceHistory;

public class CreateServiceRecordDto
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public ServiceType ServiceType { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime ServiceDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Cost { get; set; }
}
