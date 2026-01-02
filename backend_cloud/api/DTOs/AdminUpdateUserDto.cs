using System.ComponentModel.DataAnnotations;

namespace RfidWarehouseApi.DTOs;

public class AdminUpdateUserDto
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Lastname { get; set; } = string.Empty;

    public List<int> RoleIds { get; set; } = new();

    public string? NewPassword { get; set; }

    public string? RfidTagUid { get; set; }
}
