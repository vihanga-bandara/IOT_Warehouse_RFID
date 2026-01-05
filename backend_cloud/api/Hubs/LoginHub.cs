using Microsoft.AspNetCore.SignalR;
using RfidWarehouseApi.Constants;

namespace RfidWarehouseApi.Hubs;

/// <summary>
/// SignalR hub for unauthenticated clients waiting for RFID login.
/// This hub does NOT require authentication so the login page can listen
/// for RfidLoginSuccess events before the user is authenticated.
/// </summary>
public class LoginHub : Hub
{
    private readonly ILogger<LoginHub> _logger;

    public LoginHub(ILogger<LoginHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Unauthenticated client connected to LoginHub. ConnectionId: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected from LoginHub. ConnectionId: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a scanner-specific group to receive RFID login events for that scanner.
    /// This allows the login page to wait for a user to tap their RFID card.
    /// </summary>
    public async Task JoinScannerLoginGroup(string scannerName)
    {
        if (string.IsNullOrWhiteSpace(scannerName))
        {
            _logger.LogWarning("JoinScannerLoginGroup called with empty scanner name");
            return;
        }

        // Use scanner name as group identifier (the IoT service broadcasts to scanner_{deviceId})
        // We need to look up the deviceId from the scanner name
        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.Scanner(scannerName));
        _logger.LogInformation("Client joined scanner login group for scanner: {ScannerName}, ConnectionId: {ConnectionId}",
            scannerName, Context.ConnectionId);
    }

    /// <summary>
    /// Join a scanner group using device ID directly
    /// </summary>
    public async Task JoinScannerGroupByDeviceId(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogWarning("JoinScannerGroupByDeviceId called with empty device ID");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, HubGroups.Scanner(deviceId));
        _logger.LogInformation("Client joined scanner login group for deviceId: {DeviceId}, ConnectionId: {ConnectionId}",
            deviceId, Context.ConnectionId);
    }

    /// <summary>
    /// Ping to keep connection alive
    /// </summary>
    public async Task Ping()
    {
        await Clients.Caller.SendAsync(HubEvents.Pong);
    }
}
