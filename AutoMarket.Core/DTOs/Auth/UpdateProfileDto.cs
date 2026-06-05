using System.ComponentModel.DataAnnotations;

namespace AutoMarket.Core.DTOs.Auth;

public class UpdateProfileDto
{
    [MaxLength(50)]
    public string? Username { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }
}
