using System.ComponentModel.DataAnnotations;

namespace RfidWarehouseApi.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Lastname { get; set; } = string.Empty;

    public string? RfidTagUid { get; set; }

    public List<int>? RoleIds { get; set; } // Defaults to [2] (User role) if not provided

    /// <summary>
    /// If true, a 4-digit PIN will be auto-generated for MFA on RFID login
    /// </summary>
    public bool GeneratePin { get; set; } = true;
}

/// <summary>
/// Extended response for registration that includes the generated PIN (shown once)
/// </summary>
public class RegisterResponseDto : AuthResponseDto
{
    /// <summary>
    /// The auto-generated PIN shown only at registration time (if GeneratePin was true)
    /// </summary>
    public string? GeneratedPin { get; set; }
}

