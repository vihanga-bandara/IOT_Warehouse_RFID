using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RfidWarehouseApi.Models;

[Table("Items")]
[Index(nameof(RfidUid), IsUnique = true)]
public class Item
{
    [Key]
    [Column("item_id")]
    public int ItemId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("rfid_uid")]
    public string RfidUid { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("item_name")]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    public ItemStatus Status { get; set; } = ItemStatus.Available;

    [Column("current_holder_id")]
    public int? CurrentHolderId { get; set; }

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [Column("notes")]
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if a reminder email has been sent for this item while borrowed.
    /// Resets to false when item is returned.
    /// </summary>
    [Column("reminder_email_sent")]
    public bool ReminderEmailSent { get; set; } = false;

    /// <summary>
    /// When the last reminder email was sent for this item.
    /// </summary>
    [Column("reminder_email_sent_at")]
    public DateTime? ReminderEmailSentAt { get; set; }

    // Navigation properties
    [ForeignKey("CurrentHolderId")]
    public virtual User? CurrentHolder { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
