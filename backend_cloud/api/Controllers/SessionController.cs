using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RfidWarehouseApi.Services;
using RfidWarehouseApi.Extensions;

namespace RfidWarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ICheckoutSessionManager _sessionManager;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ICheckoutSessionManager sessionManager, ILogger<SessionController> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    [HttpGet("current")]
    public IActionResult GetCurrentSession()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var cart = _sessionManager.GetUserCart(userId);
        if (cart == null)
        {
            return Ok(new
            {
                UserId = userId,
                Items = new List<object>(),
                SessionStarted = (DateTime?)null
            });
        }

        return Ok(cart);
    }

    [HttpPost("clear")]
    public IActionResult ClearSession()
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        _sessionManager.ClearUserCart(userId);
        _logger.LogInformation("Session cleared for user {UserId}", userId);

        return Ok(new { message = "Session cleared successfully" });
    }

    [HttpDelete("items/{itemId}")]
    public IActionResult RemoveItemFromCart(int itemId)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var removed = _sessionManager.RemoveItemFromCart(userId, itemId);
        if (!removed)
        {
            return NotFound(new { message = "Item not found in cart" });
        }

        return Ok(new { message = "Item removed from cart" });
    }
}
