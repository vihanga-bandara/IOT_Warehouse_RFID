using System.ComponentModel.DataAnnotations;

namespace RfidWarehouseApi.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    // Optional scanner name entered at login (e.g. "Front Desk Kiosk")
    public string? ScannerName { get; set; }
}
