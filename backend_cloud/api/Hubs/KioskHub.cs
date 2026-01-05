using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Extensions;

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
        if (Context.User.TryGetUserId(out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.User(userId.ToString()));
            _logger.LogInformation("User {UserId} connected to SignalR hub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User.TryGetUserId(out var userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, HubGroups.User(userId.ToString()));
            _logger.LogInformation("User {UserId} disconnected from SignalR hub. ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Client can send ping to keep connection alive
    public async Task Ping()
    {
        await Clients.Caller.SendAsync(HubEvents.Pong);
    }

    // Browser session joins a scanner-specific group so it can receive
    // cart updates for the bound physical scanner device.
    public async Task JoinScannerGroup(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || !Context.User.TryGetUserId(out var userId))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.Scanner(deviceId));
        _logger.LogInformation("User {UserId} joined scanner group {DeviceId}", userId, deviceId);
    }
}
