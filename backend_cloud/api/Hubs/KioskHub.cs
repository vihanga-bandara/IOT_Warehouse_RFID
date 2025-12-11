using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace RfidWarehouseApi.Hubs;

[Authorize]
public class KioskHub : Hub
{
    private readonly ILogger<KioskHub> _logger;

    public KioskHub(ILogger<KioskHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to SignalR hub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from SignalR hub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Client can send ping to keep connection alive
    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong");
    }

    // Browser session joins a scanner-specific group so it can receive
    // cart updates for the bound physical scanner device.
    public async Task JoinScannerGroup(string deviceId)
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"scanner_{deviceId}");
        _logger.LogInformation("User {UserId} joined scanner group {DeviceId}", userId, deviceId);
    }
}
