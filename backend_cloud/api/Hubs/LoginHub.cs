using Microsoft.AspNetCore.SignalR;
using RfidWarehouseApi.Constants;

namespace RfidWarehouseApi.Hubs;

// Unauthenticated hub for RFID login flow
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

    // Join scanner group to listen for RFID login events
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

    public async Task Ping()
    {
        await Clients.Caller.SendAsync(HubEvents.Pong);
    }
}
