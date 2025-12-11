using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
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

        var result = await _authService.LoginAsync(loginDto);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(result);
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

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
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
            .Select(ur => string.IsNullOrWhiteSpace(ur.Role.RoleName) ? (ur.RoleId == 1 ? "Admin" : "User") : ur.Role.RoleName)
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

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
                    Name = string.IsNullOrWhiteSpace(ur.RoleName) ? (ur.RoleId == 1 ? "Admin" : "User") : ur.RoleName 
                })
                .DistinctBy(r => r.Id)
                .ToList(),
            u.TransactionCount
        });

        return Ok(result);
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var updatedUser = await _authService.UpdateUserAsync(id, dto);
        if (updatedUser == null) return NotFound();

        return Ok(new { message = "User updated successfully" });
    }

    [HttpGet("users/{userId}/transactions")]
    [Authorize(Roles = "Admin")]
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
}
