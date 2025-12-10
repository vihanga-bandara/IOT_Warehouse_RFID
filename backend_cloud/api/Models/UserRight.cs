using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RfidWarehouseApi.Models;

[Table("UserRight")]
[PrimaryKey(nameof(UserId), nameof(RoleId))]
[Index(nameof(UserId), nameof(RoleId), IsUnique = true, Name = "user_id,role_id")]
public class UserRight
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = null!;
}
