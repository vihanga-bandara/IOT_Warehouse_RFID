using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.Models;
using RfidWarehouseApi.Services;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly WarehouseDbContext _context;
    private readonly ICheckoutSessionManager _sessionManager;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        WarehouseDbContext context,
        ICheckoutSessionManager sessionManager,
        ILogger<TransactionController> logger)
    {
        _context = context;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    [HttpPost("commit")]
    public async Task<IActionResult> CommitTransaction([FromBody] CommitTransactionDto dto)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var cart = _sessionManager.GetUserCart(userId);
        if (cart == null || !cart.Items.Any())
        {
            return BadRequest(new { message = "Cart is empty" });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var scanner = await _context.Scanners
                .FirstOrDefaultAsync(s => s.DeviceIdString == dto.DeviceId);

            if (scanner == null)
            {
                return BadRequest(new { message = "Invalid scanner device" });
            }

            var transactions = new List<Transaction>();

            foreach (var cartItem in cart.Items)
            {
                // Re-verify item status (concurrency check)
                var item = await _context.Items.FindAsync(cartItem.ItemId);
                if (item == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = $"Item {cartItem.ItemName} not found" });
                }

                // Validate action is still valid
                if (cartItem.Action == "Borrow" && item.Status != ItemStatus.Available)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = $"Item {cartItem.ItemName} is no longer available" });
                }

                if (cartItem.Action == "Return" && 
                    (item.Status != ItemStatus.Borrowed || item.CurrentHolderId != userId))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = $"You cannot return item {cartItem.ItemName}" });
                }

                // Update item status
                if (cartItem.Action == "Borrow")
                {
                    item.Status = ItemStatus.Borrowed;
                    item.CurrentHolderId = userId;
                }
                else if (cartItem.Action == "Return")
                {
                    item.Status = ItemStatus.Available;
                    item.CurrentHolderId = null;
                }

                item.LastUpdated = DateTime.UtcNow;

                // Create transaction record
                var txn = new Transaction
                {
                    UserId = userId,
                    ItemId = item.ItemId,
                    DeviceId = scanner.ScannerId,
                    Action = cartItem.Action == "Borrow" ? TransactionAction.Checkout : TransactionAction.Checkin,
                    Timestamp = DateTime.UtcNow,
                    Notes = dto.Notes
                };

                transactions.Add(txn);
                _context.Transactions.Add(txn);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Clear the cart
            _sessionManager.ClearUserCart(userId);

            _logger.LogInformation("Transaction committed for user {UserId}. {Count} items processed", userId, transactions.Count);

            return Ok(new
            {
                message = "Transaction completed successfully",
                transactionCount = transactions.Count,
                transactions = transactions.Select(t => new
                {
                    t.TransactionId,
                    Action = t.Action.ToString(),
                    t.Timestamp
                })
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error committing transaction for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while processing the transaction" });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetUserHistory()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var history = await _context.Transactions
            .Include(t => t.Item)
            .Include(t => t.Scanner)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .Take(50)
            .Select(t => new
            {
                t.TransactionId,
                Action = t.Action.ToString(),
                t.Timestamp,
                Item = new
                {
                    t.Item.ItemId,
                    t.Item.ItemName,
                    t.Item.RfidUid
                },
                Scanner = new
                {
                    t.Scanner.LocationName
                }
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllTransactions([FromQuery] int? page = 1, [FromQuery] int? pageSize = 50)
    {
        var query = _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Item)
            .Include(t => t.Scanner)
            .OrderByDescending(t => t.Timestamp);

        var total = await query.CountAsync();
        var transactions = await query
            .Skip(((page ?? 1) - 1) * (pageSize ?? 50))
            .Take(pageSize ?? 50)
            .Select(t => new
            {
                t.TransactionId,
                Action = t.Action.ToString(),
                t.Timestamp,
                User = new
                {
                    t.User.UserId,
                    t.User.Name,
                    t.User.Lastname,
                    t.User.Email
                },
                Item = new
                {
                    t.Item.ItemId,
                    t.Item.ItemName,
                    t.Item.RfidUid
                },
                Scanner = new
                {
                    t.Scanner.ScannerId,
                    t.Scanner.LocationName
                },
                t.Notes
            })
            .ToListAsync();

        return Ok(new
        {
            total,
            page = page ?? 1,
            pageSize = pageSize ?? 50,
            data = transactions
        });
    }
}

public class CommitTransactionDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
