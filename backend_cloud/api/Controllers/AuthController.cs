using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Extensions;
using RfidWarehouseApi.Services;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly WarehouseDbContext _context;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, WarehouseDbContext context)
    {
        _authService = authService;
        _logger = logger;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
            return Ok(result);
        }
        catch (InvalidOperationException ioe)
        {
            // Known validation (scanner missing / not found) -> return 400 with details
            return BadRequest(new { message = ioe.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(500, new { message = "An unexpected error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(registerDto);

        if (result == null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Login using RFID card UID (for kiosk RFID-based authentication)
    /// </summary>
    [HttpPost("login/rfid")]
    public async Task<IActionResult> LoginWithRfid([FromBody] RfidLoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.RfidTagUid))
        {
            return BadRequest(new { message = "RFID tag UID is required" });
        }

        try
        {
            var result = await _authService.LoginWithRfidAsync(loginDto.RfidTagUid, loginDto.ScannerName);
            if (result == null)
            {
                return Unauthorized(new { message = "No user found for this RFID card" });
            }
            return Ok(new RfidLoginResponseDto
            {
                Success = true,
                Token = result.Token,
                Email = result.Email,
                Name = result.Name,
                Lastname = result.Lastname,
                UserId = result.UserId,
                RoleIds = result.RoleIds,
                ScannerDeviceId = result.ScannerDeviceId,
                ScannerName = result.ScannerName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during RFID login");
            return StatusCode(500, new { message = "An unexpected error occurred during login" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.UserRights)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            return NotFound();
        }

        var roles = user.UserRights
            .Select(ur => RoleNameExtensions.NormalizeRoleName(ur.Role?.RoleName, ur.RoleId))
            .Distinct()
            .ToList();

        var profile = new UserProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Name = user.Name,
            Lastname = user.Lastname,
            RfidTagUid = user.RfidTagUid,
            Roles = roles
        };

        return Ok(profile);
    }

    [HttpPut("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

        if (!result)
        {
            return BadRequest(new { message = "Current password is incorrect" });
        }

        return Ok(new { message = "Password updated successfully" });
    }

    [HttpGet("roles")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Select(r => new
            {
                r.RoleId,
                r.RoleName,
                r.Description
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("users")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.UserRights)
            .ThenInclude(ur => ur.Role)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.Name,
                u.Lastname,
                u.RfidTagUid,
                UserRights = u.UserRights.Select(ur => new { ur.RoleId, ur.Role.RoleName }),
                TransactionCount = u.Transactions.Count()
            })
            .ToListAsync();

        var result = users.Select(u => new
        {
            Id = u.UserId,
            u.Email,
            u.Name,
            u.Lastname,
            RfidUid = u.RfidTagUid,
            Roles = u.UserRights
                .Select(ur => new { 
                    Id = ur.RoleId, 
                    Name = RoleNameExtensions.NormalizeRoleName(ur.RoleName, ur.RoleId)
                })
                .DistinctBy(r => r.Id)
                .ToList(),
            u.TransactionCount
        });

        return Ok(result);
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var updatedUser = await _authService.UpdateUserAsync(id, dto);
        if (updatedUser == null) return NotFound();

        return Ok(new { message = "User updated successfully" });
    }

    [HttpGet("users/{userId}/transactions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetUserTransactions(int userId)
    {
        var transactions = await _context.Transactions
            .Include(t => t.Item)
            .Include(t => t.Scanner)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new
            {
                Id = t.TransactionId,
                Action = t.Action.ToString(),
                Timestamp = t.Timestamp,
                ItemName = t.Item.ItemName,
                ItemId = t.Item.ItemId,
                DeviceName = t.Scanner != null ? t.Scanner.Name : "Unknown"
            })
            .ToListAsync();

        return Ok(transactions);
    }

    /// <summary>
    /// Validate and bind a scanner to the current user session.
    /// Called after login to select which scanner/kiosk to use.
    /// </summary>
    [HttpPost("bind-scanner")]
    [Authorize]
    public async Task<IActionResult> BindScanner([FromBody] BindScannerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ScannerName))
        {
            return BadRequest(new { message = "Scanner name is required" });
        }

        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // Look up scanner by name (case-insensitive)
        var normalized = dto.ScannerName.Trim().ToLower();
        var scanner = await _context.Scanners
            .FirstOrDefaultAsync(s => s.Name != null && s.Name.ToLower() == normalized);

        if (scanner == null)
        {
            return NotFound(new { message = $"Scanner '{dto.ScannerName}' not found. Please check the scanner name and try again." });
        }

        // Bind user to scanner session
        var scannerSessionService = HttpContext.RequestServices.GetRequiredService<IScannerSessionService>();
        await scannerSessionService.BindUserToScannerAsync(userId, scanner.Name!);

        _logger.LogInformation("User {UserId} bound to scanner {ScannerName} ({DeviceId})", userId, scanner.Name, scanner.DeviceId);

        return Ok(new 
        { 
            scannerDeviceId = scanner.DeviceId, 
            scannerName = scanner.Name,
            message = "Scanner bound successfully"
        });
    }

    [HttpGet("generate-rfid-uid")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GenerateUniqueRfidUid()
    {
        string newUid;
        do
        {
            // Generate a unique raw UID (no LOGIN: prefix - the prefix is added by the scanner when sending)
            newUid = Guid.NewGuid().ToString("N")[..14].ToUpperInvariant();
        }
        while (await _context.Users.AnyAsync(u => u.RfidTagUid == newUid));

        return Ok(new { rfidUid = newUid });
    }

    [HttpGet("check-rfid-uid")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CheckRfidUidAvailable([FromQuery] string rfidUid, [FromQuery] int? excludeUserId = null)
    {
        var query = _context.Users.Where(u => u.RfidTagUid == rfidUid);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        var isInUse = await query.AnyAsync();
        return Ok(new { available = !isInUse });
    }
}
