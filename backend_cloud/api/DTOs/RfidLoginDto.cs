namespace RfidWarehouseApi.DTOs;

public class RfidLoginDto
{
    public string RfidTagUid { get; set; } = string.Empty;
    public string? ScannerName { get; set; }
}

public class RfidLoginResponseDto
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
    
    /// <summary>
    /// If true, the user must verify their PIN before full access is granted
    /// </summary>
    public bool RequiresPinVerification { get; set; }
    
    /// <summary>
    /// Temporary MFA token for PIN verification (only set when RequiresPinVerification is true)
    /// </summary>
    public string? MfaToken { get; set; }
}
