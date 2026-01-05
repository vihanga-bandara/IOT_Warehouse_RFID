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
}
