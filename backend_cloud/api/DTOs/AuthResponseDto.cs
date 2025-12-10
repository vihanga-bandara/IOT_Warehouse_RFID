namespace RfidWarehouseApi.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = new List<int>();
}
