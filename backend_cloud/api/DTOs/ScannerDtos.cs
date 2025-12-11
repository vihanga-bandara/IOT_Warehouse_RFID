using System.ComponentModel.DataAnnotations;

namespace RfidWarehouseApi.DTOs;

public class ScannerDto
{
    public int ScannerId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}

public class CreateScannerDto
{
    [Required]
    [MaxLength(255)]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Status { get; set; }
}

public class UpdateScannerDto
{
    [Required]
    [MaxLength(255)]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Status { get; set; }
}
