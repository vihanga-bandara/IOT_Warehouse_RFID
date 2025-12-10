using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RfidWarehouseApi.Models;

[Table("Scanners")]
public class Scanner
{
    [Key]
    [Column("scanner_id")]
    public int ScannerId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("device_id_string")]
    public string DeviceIdString { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("location_name")]
    public string LocationName { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
