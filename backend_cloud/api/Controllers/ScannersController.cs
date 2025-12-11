using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ScannersController : ControllerBase
{
    private readonly WarehouseDbContext _context;

    public ScannersController(WarehouseDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScannerDto>>> GetScanners()
    {
        var scanners = await _context.Scanners
            .OrderBy(s => s.Name)
            .ThenBy(s => s.DeviceId)
            .ToListAsync();

        var result = scanners.Select(s => new ScannerDto
        {
            ScannerId = s.ScannerId,
            DeviceId = s.DeviceId,
            Name = s.Name ?? string.Empty,
            Status = string.IsNullOrWhiteSpace(s.Status) ? "Active" : s.Status!
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ScannerDto>> CreateScanner([FromBody] CreateScannerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        dto.DeviceId = dto.DeviceId.Trim();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.DeviceId) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "DeviceId and Name are required." });
        }

        var existing = await _context.Scanners
            .FirstOrDefaultAsync(s => s.DeviceId == dto.DeviceId);

        if (existing != null)
        {
            return Conflict(new { message = "A scanner with this DeviceId already exists." });
        }

        var scanner = new Scanner
        {
            DeviceId = dto.DeviceId,
            Name = dto.Name,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status!.Trim()
        };

        _context.Scanners.Add(scanner);
        await _context.SaveChangesAsync();

        var result = new ScannerDto
        {
            ScannerId = scanner.ScannerId,
            DeviceId = scanner.DeviceId,
            Name = scanner.Name ?? string.Empty,
            Status = scanner.Status ?? "Active"
        };

        return CreatedAtAction(nameof(GetScanners), new { id = scanner.ScannerId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScannerDto>> UpdateScanner(int id, [FromBody] UpdateScannerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        dto.DeviceId = dto.DeviceId.Trim();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.DeviceId) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "DeviceId and Name are required." });
        }

        var scanner = await _context.Scanners.FindAsync(id);
        if (scanner == null)
        {
            return NotFound();
        }

        var duplicate = await _context.Scanners
            .FirstOrDefaultAsync(s => s.DeviceId == dto.DeviceId && s.ScannerId != id);

        if (duplicate != null)
        {
            return Conflict(new { message = "Another scanner with this DeviceId already exists." });
        }

        scanner.DeviceId = dto.DeviceId;
        scanner.Name = dto.Name;
        scanner.Status = string.IsNullOrWhiteSpace(dto.Status) ? scanner.Status : dto.Status!.Trim();

        await _context.SaveChangesAsync();

        var result = new ScannerDto
        {
            ScannerId = scanner.ScannerId,
            DeviceId = scanner.DeviceId,
            Name = scanner.Name ?? string.Empty,
            Status = scanner.Status ?? "Active"
        };

        return Ok(result);
    }
}
