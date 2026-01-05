using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Extensions;
using RfidWarehouseApi.Hubs;
using RfidWarehouseApi.Models;
using RfidWarehouseApi.Services;
using Microsoft.AspNetCore.SignalR;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly WarehouseDbContext _context;
    private readonly ICheckoutSessionManager _sessionManager;
    private readonly IHubContext<KioskHub> _hubContext;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        WarehouseDbContext context,
        ICheckoutSessionManager sessionManager,
        IHubContext<KioskHub> hubContext,
        ILogger<TransactionController> logger)
    {
        _context = context;
        _sessionManager = sessionManager;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost("commit")]
    public async Task<IActionResult> CommitTransaction([FromBody] CommitTransactionDto dto)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var cart = _sessionManager.GetUserCart(userId);
        if (cart == null || !cart.Items.Any())
        {
            return BadRequest(new { message = "Cart is empty" });
        }

        var executionStrategy = _context.Database.CreateExecutionStrategy();
        IActionResult? actionResult = null;
        string? scannerDeviceId = null;
        object? responseBody = null;
        int transactionCount = 0;

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var scanner = await _context.Scanners
                    .FirstOrDefaultAsync(s => s.DeviceId == dto.DeviceId);

                if (scanner == null)
                {
                    actionResult = BadRequest(new { message = "Invalid scanner device" });
                    return;
                }

                var transactions = new List<Transaction>();

                foreach (var cartItem in cart.Items)
                {
                    // Re-verify item status (concurrency check)
                    var item = await _context.Items.FindAsync(cartItem.ItemId);
                    if (item == null)
                    {
                        actionResult = BadRequest(new { message = $"Item {cartItem.ItemName} not found" });
                        return;
                    }

                    // Validate action is still valid
                    if (cartItem.Action == "Borrow" && item.Status != ItemStatus.Available)
                    {
                        actionResult = BadRequest(new { message = $"Item {cartItem.ItemName} is no longer available" });
                        return;
                    }

                    if (cartItem.Action == "Return" &&
                        (item.Status != ItemStatus.Borrowed || item.CurrentHolderId != userId))
                    {
                        actionResult = BadRequest(new { message = $"You cannot return item {cartItem.ItemName}" });
                        return;
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
                        // Reset reminder email flag when item is returned
                        item.ReminderEmailSent = false;
                        item.ReminderEmailSentAt = null;
                    }

                    item.LastUpdated = DateTime.UtcNow;

                    // Create transaction record
                    var txn = new Transaction
                    {
                        UserId = userId,
                        ItemId = item.ItemId,
                        DeviceId = scanner.DeviceId,
                        Action = cartItem.Action == "Borrow" ? TransactionAction.Checkout : TransactionAction.Checkin,
                        Timestamp = DateTime.UtcNow,
                        Notes = dto.Notes
                    };

                    transactions.Add(txn);
                    _context.Transactions.Add(txn);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                scannerDeviceId = scanner.DeviceId;
                transactionCount = transactions.Count;
                responseBody = new
                {
                    message = "Transaction completed successfully",
                    transactionCount = transactions.Count,
                    transactions = transactions.Select(t => new
                    {
                        t.TransactionId,
                        Action = t.Action.ToString(),
                        t.Timestamp
                    })
                };

                actionResult = Ok(responseBody);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error committing transaction for user {UserId}", userId);
                actionResult = StatusCode(500, new { message = "An error occurred while processing the transaction" });
            }
        });

        if (actionResult is OkObjectResult && scannerDeviceId != null)
        {
            // Clear the cart
            _sessionManager.ClearUserCart(userId);

            // Keep the kiosk UI in sync: broadcast an empty cart to this scanner group
            // (useful when the cart was populated via IoT listener and/or multiple clients).
            try
            {
                await _hubContext.Clients.Group($"scanner_{scannerDeviceId}")
                    .SendAsync("CartUpdated", new SessionCartDto
                    {
                        UserId = userId,
                        SessionStarted = DateTime.UtcNow,
                        Items = new List<CartItemDto>()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Transaction committed but failed to broadcast cart update for scanner {DeviceId}", scannerDeviceId);
            }

            _logger.LogInformation("Transaction committed for user {UserId}. {Count} items processed", userId, transactionCount);
        }

        return actionResult ?? StatusCode(500, new { message = "An error occurred while processing the transaction" });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetUserHistory()
    {
        if (!User.TryGetUserId(out var userId))
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
                Id = t.TransactionId,
                Action = t.Action.ToString(),
                Timestamp = t.Timestamp,
                ItemName = t.Item.ItemName,
                ItemId = t.Item.ItemId,
                RfidUid = t.Item.RfidUid,
                DeviceName = t.Scanner != null ? t.Scanner.Name : "Unknown"
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("all")]
    [Authorize(Roles = Roles.Admin)]
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
                    t.Scanner!.ScannerId,
                    t.Scanner.DeviceId,
                    t.Scanner.Name
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
