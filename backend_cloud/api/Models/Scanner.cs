using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidWarehouseApi.Models;

[Table("Scanner")]
public class Scanner
{
    [Key]
    [Column("scanner_id")]
    public int ScannerId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("name")]
    public string? Name { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    // Navigation properties
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
