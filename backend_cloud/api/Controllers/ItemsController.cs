using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.Models;
using RfidWarehouseApi.Services;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ItemsController> _logger;
    private readonly IEmailService _emailService;

    public ItemsController(
        WarehouseDbContext context, 
        ILogger<ItemsController> logger,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllItems()
    {
        var items = await _context.Items
            .Include(i => i.CurrentHolder)
            .Select(i => new
            {
                id = i.ItemId,
                itemId = i.ItemId,
                rfidUid = i.RfidUid,
                itemName = i.ItemName,
                status = i.Status.ToString(),
                lastUpdated = i.LastUpdated,
                currentHolderName = i.CurrentHolder != null ? (i.CurrentHolder.Name + " " + i.CurrentHolder.Lastname).Trim() : null,
                currentHolderEmail = i.CurrentHolder != null ? i.CurrentHolder.Email : null,
                currentHolder = i.CurrentHolder != null ? new
                {
                    userId = i.CurrentHolder.UserId,
                    name = i.CurrentHolder.Name,
                    lastname = i.CurrentHolder.Lastname,
                    email = i.CurrentHolder.Email
                } : null
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await _context.Items
            .Include(i => i.CurrentHolder)
            .Include(i => i.Transactions)
                .ThenInclude(t => t.User)
            .Where(i => i.ItemId == id)
            .Select(i => new
            {
                i.ItemId,
                i.RfidUid,
                i.ItemName,
                Status = i.Status.ToString(),
                i.LastUpdated,
                CurrentHolder = i.CurrentHolder != null ? new
                {
                    i.CurrentHolder.UserId,
                    i.CurrentHolder.Name,
                    i.CurrentHolder.Lastname
                } : null,
                RecentTransactions = i.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Take(10)
                    .Select(t => new
                    {
                        t.TransactionId,
                        Action = t.Action.ToString(),
                        t.Timestamp,
                        User = new
                        {
                            t.User.Name,
                            t.User.Lastname
                        }
                    })
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if RFID UID already exists
        if (await _context.Items.AnyAsync(i => i.RfidUid == dto.RfidUid))
        {
            return BadRequest(new { message = "An item with this RFID UID already exists" });
        }

        var item = new Item
        {
            RfidUid = dto.RfidUid,
            ItemName = dto.ItemName,
            Status = ItemStatus.Available,
            LastUpdated = DateTime.UtcNow
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New item created: {ItemName} with RFID: {RfidUid}", item.ItemName, item.RfidUid);

        return CreatedAtAction(nameof(GetItem), new { id = item.ItemId }, new
        {
            item.ItemId,
            item.RfidUid,
            item.ItemName,
            Status = item.Status.ToString(),
            item.LastUpdated
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateItemDto dto)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        item.ItemName = dto.ItemName;
        item.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            item.ItemId,
            item.RfidUid,
            item.ItemName,
            Status = item.Status.ToString(),
            item.LastUpdated
        });
    }

    /// <summary>
    /// Get all borrowed items with their borrow date, days overdue, and reminder status
    /// </summary>
    [HttpGet("borrowed")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetBorrowedItems()
    {
        const int overdueDaysThreshold = 7;
        var now = DateTime.UtcNow;

        // Get borrowed items with their last checkout transaction
        var borrowedItems = await _context.Items
            .Include(i => i.CurrentHolder)
            .Where(i => i.Status == ItemStatus.Borrowed && i.CurrentHolderId != null)
            .Select(i => new
            {
                i.ItemId,
                i.RfidUid,
                i.ItemName,
                i.ReminderEmailSent,
                i.ReminderEmailSentAt,
                HolderName = i.CurrentHolder != null ? (i.CurrentHolder.Name + " " + i.CurrentHolder.Lastname).Trim() : null,
                HolderEmail = i.CurrentHolder != null ? i.CurrentHolder.Email : null,
                HolderId = i.CurrentHolderId,
                // Get the last checkout transaction for this item
                LastBorrow = _context.Transactions
                    .Where(t => t.ItemId == i.ItemId && t.Action == TransactionAction.Checkout)
                    .OrderByDescending(t => t.Timestamp)
                    .Select(t => t.Timestamp)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var result = borrowedItems.Select(item =>
        {
            var borrowedAt = item.LastBorrow;
            var daysBorrowed = borrowedAt != default ? (int)(now - borrowedAt).TotalDays : 0;
            var isOverdue = daysBorrowed >= overdueDaysThreshold;

            return new
            {
                item.ItemId,
                item.RfidUid,
                item.ItemName,
                item.HolderName,
                item.HolderEmail,
                item.HolderId,
                BorrowedAt = borrowedAt,
                DaysBorrowed = daysBorrowed,
                IsOverdue = isOverdue,
                DaysOverdue = isOverdue ? daysBorrowed - overdueDaysThreshold + 1 : 0,
                ReminderSent = item.ReminderEmailSent,
                ReminderSentAt = item.ReminderEmailSentAt
            };
        })
        .OrderByDescending(x => x.DaysBorrowed)
        .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Manually send a reminder email for a borrowed item
    /// </summary>
    [HttpPost("{id}/send-reminder")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> SendReminderEmail(int id)
    {
        const int overdueDaysThreshold = 7;

        var item = await _context.Items
            .Include(i => i.CurrentHolder)
            .FirstOrDefaultAsync(i => i.ItemId == id);

        if (item == null)
        {
            return NotFound(new { message = "Item not found" });
        }

        if (item.Status != ItemStatus.Borrowed || item.CurrentHolder == null)
        {
            return BadRequest(new { message = "Item is not currently borrowed" });
        }

        // Get the last checkout transaction
        var lastBorrow = await _context.Transactions
            .Where(t => t.ItemId == id && t.Action == TransactionAction.Checkout)
            .OrderByDescending(t => t.Timestamp)
            .Select(t => t.Timestamp)
            .FirstOrDefaultAsync();

        if (lastBorrow == default)
        {
            return BadRequest(new { message = "Could not determine when item was borrowed" });
        }

        var daysBorrowed = (int)(DateTime.UtcNow - lastBorrow).TotalDays;
        var daysOverdue = Math.Max(0, daysBorrowed - overdueDaysThreshold + 1);

        try
        {
            await _emailService.SendOverdueItemEmailAsync(
                item.CurrentHolder.Email,
                item.CurrentHolder.Name,
                item.ItemName,
                daysOverdue > 0 ? daysOverdue : daysBorrowed,
                lastBorrow);

            // Update the reminder sent tracking
            item.ReminderEmailSent = true;
            item.ReminderEmailSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin manually sent reminder email for item {ItemId} ({ItemName}) to {Email}",
                item.ItemId, item.ItemName, item.CurrentHolder.Email);

            return Ok(new
            {
                message = "Reminder email sent successfully",
                sentTo = item.CurrentHolder.Email,
                itemName = item.ItemName,
                sentAt = item.ReminderEmailSentAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder email for item {ItemId}", id);
            return StatusCode(500, new { message = "Failed to send reminder email" });
        }
    }
}

public class CreateItemDto
{
    public string RfidUid { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
}

public class UpdateItemDto
{
    public string ItemName { get; set; } = string.Empty;
}
