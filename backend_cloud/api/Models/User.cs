using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidWarehouseApi.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("rfid_tag_uid")]
    public string? RfidTagUid { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("lastname")]
    public string Lastname { get; set; } = string.Empty;

    /// <summary>
    /// Hashed 4-digit PIN for MFA on RFID login
    /// </summary>
    [MaxLength(500)]
    [Column("pin_hash")]
    public string? PinHash { get; set; }

    /// <summary>
    /// Number of consecutive failed PIN attempts. Resets on successful verification.
    /// </summary>
    [Column("pin_failed_attempts")]
    public int PinFailedAttempts { get; set; } = 0;

    /// <summary>
    /// Timestamp of when PIN was last locked out (after 5 failed attempts)
    /// </summary>
    [Column("pin_lockout_until")]
    public DateTime? PinLockoutUntil { get; set; }

    // Navigation properties
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<UserRight> UserRights { get; set; } = new List<UserRight>();
}
