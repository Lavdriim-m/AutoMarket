using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Vehicles;

public class DecodeVinRequestDto
{
    [Required]
    [MaxLength(17)]
    public string VIN { get; set; } = string.Empty;
}
