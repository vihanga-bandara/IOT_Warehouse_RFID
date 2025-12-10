using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidWarehouseApi.Models;

[Table("Transactions")]
public class Transaction
{
    [Key]
    [Column("transaction_id")]
    public int TransactionId { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [Column("action")]
    public TransactionAction Action { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Column("notes")]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ItemId")]
    public virtual Item Item { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Scanner? Scanner { get; set; }
}
