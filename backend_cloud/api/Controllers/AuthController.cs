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

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.UserRights)
            .Select(u => new
            {
                Id = u.UserId,
                u.Email,
                u.Name,
                u.Lastname,
                RfidUid = u.RfidTagUid,
                Role = u.UserRights.Any(ur => ur.RoleId == 1) ? "Admin" : "User",
                TransactionCount = _context.Transactions.Count(t => t.UserId == u.UserId)
            })
            .ToListAsync();

        return Ok(users);
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
