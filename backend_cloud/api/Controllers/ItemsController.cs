using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(WarehouseDbContext context, ILogger<ItemsController> logger)
    {
        _context = context;
        _logger = logger;
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
