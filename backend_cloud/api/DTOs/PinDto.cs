using System.ComponentModel.DataAnnotations;

namespace RfidWarehouseApi.DTOs;

/// <summary>
/// Request DTO for verifying a PIN after RFID login
/// </summary>
public class VerifyPinDto
{
    /// <summary>
    /// The 4-digit PIN entered by the user
    /// </summary>
    [Required]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits")]
    public string Pin { get; set; } = string.Empty;

    /// <summary>
    /// The temporary MFA token received after RFID scan
    /// </summary>
    [Required]
    public string MfaToken { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for PIN verification
/// </summary>
public class PinVerificationResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public int? UserId { get; set; }
    public List<int>? RoleIds { get; set; }
    public string? ScannerDeviceId { get; set; }
    public string? ScannerName { get; set; }
    public string? Error { get; set; }
    public int? RemainingAttempts { get; set; }
    public bool? Locked { get; set; }
}

/// <summary>
/// Response DTO for generating/resetting a PIN (admin only)
/// </summary>
public class PinGeneratedResponseDto
{
    /// <summary>
    /// The plain-text PIN (only shown once at creation or reset)
    /// </summary>
    public string Pin { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
