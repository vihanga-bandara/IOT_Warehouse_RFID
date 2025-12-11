namespace RfidWarehouseApi.DTOs;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string? RfidTagUid { get; set; }
    public List<string> Roles { get; set; } = new();
}
