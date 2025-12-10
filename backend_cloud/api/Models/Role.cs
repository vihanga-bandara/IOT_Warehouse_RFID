using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidWarehouseApi.Models;

[Table("Role")]
public class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Required]
    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<UserRight> UserRights { get; set; } = new List<UserRight>();
}
