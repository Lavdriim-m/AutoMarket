using System.ComponentModel.DataAnnotations;
using AutoMarket.Core.Enums;

namespace AutoMarket.Core.DTOs.ServiceHistory;

public class UpdateServiceRecordDto
{
    public ServiceType? ServiceType { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? ServiceDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Cost { get; set; }
}
