using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Hubs;
using RfidWarehouseApi.Models;
using RfidWarehouseApi.Services;

namespace RfidWarehouseApi.Services;

public class IoTHubListenerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<KioskHub> _kioskHubContext;
    private readonly IHubContext<LoginHub> _loginHubContext;
    private readonly ICheckoutSessionManager _sessionManager;
    private readonly IScannerSessionService _scannerSessionService;
    private readonly IScannerConnectionTracker _scannerConnectionTracker;
    private readonly ILogger<IoTHubListenerService> _logger;
    private readonly IConfiguration _configuration;
    private EventHubConsumerClient? _consumerClient;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IoTHubListenerService(
        IServiceProvider serviceProvider,
        IHubContext<KioskHub> kioskHubContext,
        IHubContext<LoginHub> loginHubContext,
        ICheckoutSessionManager sessionManager,
        IScannerSessionService scannerSessionService,
        IScannerConnectionTracker scannerConnectionTracker,
        ILogger<IoTHubListenerService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _kioskHubContext = kioskHubContext;
        _loginHubContext = loginHubContext;
        _sessionManager = sessionManager;
        _scannerSessionService = scannerSessionService;
        _scannerConnectionTracker = scannerConnectionTracker;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Hub Listener Service starting...");

        var connectionString = _configuration["IoTHub:EventHubConnectionString"];
        var consumerGroup = _configuration["IoTHub:ConsumerGroup"] ?? IoTHubConstants.DefaultConsumerGroup;

        if (string.Equals(consumerGroup, IoTHubConstants.DefaultConsumerGroup, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "IoT listener is using consumer group '$Default'. If you have VS Code IoT Hub monitoring or 'az iot hub monitor-events' running, " +
                "they may also use '$Default' and prevent this app from receiving events. Consider creating a dedicated consumer group (e.g. 'rfid-api') " +
                "and set IoTHub:ConsumerGroup accordingly.");
        }

        if (string.Equals(consumerGroup, "$Default", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "IoT listener is using consumer group '$Default'. If you have VS Code IoT Hub monitoring or 'az iot hub monitor-events' running, " +
                "they may also use '$Default' and prevent this app from receiving events. Consider creating a dedicated consumer group (e.g. 'rfid-api') " +
                "and set IoTHub:ConsumerGroup accordingly.");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning(
                "IoT Hub Event Hub connection string not configured (IoTHub:EventHubConnectionString). Listener will not start. " +
                "Set it via user-secrets or environment variable 'IoTHub__EventHubConnectionString'.");
            return;
        }

        try
        {
            _consumerClient = new EventHubConsumerClient(consumerGroup, connectionString);

            try
            {
                var partitions = await _consumerClient.GetPartitionIdsAsync(stoppingToken);
                _logger.LogInformation(
                    "Connected to IoT Hub Event Hub endpoint (ConsumerGroup={ConsumerGroup}, Partitions={Partitions})",
                    consumerGroup,
                    string.Join(",", partitions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to query Event Hub partitions. The connection string may be invalid or not Event Hub-compatible.");
                throw;
            }

            // Read from each partition starting at the latest event so we don't get stuck replaying a large backlog.
            // This also makes it easier to validate end-to-end behavior during local testing.
            _logger.LogInformation("Starting IoT event read loop from EventPosition.Latest");

            var partitionReadTasks = new List<Task>(capacity: 4);
            var partitionIds = await _consumerClient.GetPartitionIdsAsync(stoppingToken);
            foreach (var partitionId in partitionIds)
            {
                partitionReadTasks.Add(ReadPartitionAsync(partitionId, stoppingToken));
            }

            await Task.WhenAll(partitionReadTasks);
        }
        catch (EventHubsException ex) when (ex.Reason == EventHubsException.FailureReason.ClientClosed && stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("IoT Hub Listener Service stopped because the Event Hubs client was closed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in IoT Hub Listener Service");
        }
    }

    private async Task ReadPartitionAsync(string partitionId, CancellationToken stoppingToken)
    {
        if (_consumerClient == null)
        {
            return;
        }

        _logger.LogInformation("Listening to IoT partition {PartitionId}", partitionId);

        await foreach (var partitionEvent in _consumerClient.ReadEventsFromPartitionAsync(
                           partitionId,
                           EventPosition.Latest,
                           stoppingToken))
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            if (partitionEvent.Data == null)
            {
                continue;
            }

            try
            {
                var eventBody = partitionEvent.Data.EventBody.ToString();
                RfidScanMessage? scanData;
                try
                {
                    scanData = JsonSerializer.Deserialize<RfidScanMessage>(eventBody, _jsonOptions);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogWarning(jsonEx, "Invalid JSON received from IoT Hub (Partition={PartitionId}). Body='{EventBody}'", partitionId, eventBody);
                    continue;
                }

                // Authoritative device identity comes from IoT Hub metadata, not the JSON payload.
                // This prevents spoofing the deviceId in message bodies.
                // Depending on the endpoint/tooling, the device id may appear in SystemProperties or Properties.
                string? deviceIdFromHub = null;
                const string deviceIdKey = "iothub-connection-device-id";
                try
                {
                    if (partitionEvent.Data.SystemProperties != null &&
                        partitionEvent.Data.SystemProperties.TryGetValue(deviceIdKey, out var deviceIdObj) &&
                        deviceIdObj != null)
                    {
                        deviceIdFromHub = deviceIdObj.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(deviceIdFromHub) &&
                        partitionEvent.Data.Properties != null &&
                        partitionEvent.Data.Properties.TryGetValue(deviceIdKey, out var deviceIdAppObj) &&
                        deviceIdAppObj != null)
                    {
                        deviceIdFromHub = deviceIdAppObj.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(deviceIdFromHub))
                    {
                        _logger.LogDebug(
                            "IoT message missing '{DeviceIdKey}' in system/app properties (Partition={PartitionId}). SystemKeys=[{SystemKeys}] AppKeys=[{AppKeys}]",
                            deviceIdKey,
                            partitionId,
                            partitionEvent.Data.SystemProperties != null ? string.Join(",", partitionEvent.Data.SystemProperties.Keys) : string.Empty,
                            partitionEvent.Data.Properties != null ? string.Join(",", partitionEvent.Data.Properties.Keys) : string.Empty);
                    }
                }
                catch
                {
                    // Best-effort: if metadata isn't present, we fall back below.
                }

                if (scanData != null)
                {
                    var effectiveDeviceId = !string.IsNullOrWhiteSpace(deviceIdFromHub)
                        ? deviceIdFromHub
                        : scanData.DeviceId;

                    if (string.IsNullOrWhiteSpace(scanData.RfidUid))
                    {
                        _logger.LogWarning(
                            "IoT message ignored: missing rfidUid in payload (Partition={PartitionId}). DeviceIdFromHub='{DeviceIdFromHub}' PayloadDeviceId='{PayloadDeviceId}'",
                            partitionId,
                            deviceIdFromHub,
                            scanData.DeviceId);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(effectiveDeviceId))
                    {
                        _logger.LogWarning("IoT message ignored: missing device id (system properties and payload) (Partition={PartitionId})", partitionId);
                        continue;
                    }

                    _logger.LogInformation(
                        "IoT telemetry received: Partition={PartitionId} DeviceId={DeviceId} RfidUid={RfidUid}",
                        partitionId,
                        effectiveDeviceId,
                        scanData.RfidUid);

                    // Check if this is a login card (format: LOGIN:uniqueid)
                    if (scanData.RfidUid.StartsWith(RfidPrefixes.UserLogin, StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessRfidLoginAsync(scanData, effectiveDeviceId);
                    }
                    else
                    {
                        await ProcessRfidScanAsync(scanData, effectiveDeviceId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IoT Hub message (Partition={PartitionId})", partitionId);
            }
        }
    }

    private async Task ProcessRfidScanAsync(RfidScanMessage scanData, string deviceId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();

        try
        {
            // Lookup item by RFID UID
            var item = await context.Items
                .Include(i => i.CurrentHolder)
                .FirstOrDefaultAsync(i => i.RfidUid == scanData.RfidUid);

            if (item == null)
            {
                _logger.LogWarning("Unknown RFID scanned: {RfidUid}", scanData.RfidUid);
                return;
            }

            var scanner = await context.Scanners
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

            if (scanner == null)
            {
                _logger.LogWarning("Unknown scanner device: {DeviceId}", deviceId);
                return;
            }

            // Determine which user is currently bound to this scanner
            var activeUserId = await _scannerSessionService.GetActiveUserForScannerAsync(deviceId);

            if (!activeUserId.HasValue)
            {
                _logger.LogInformation("RFID scan ignored because no active user is bound to scanner {DeviceId}", deviceId);
                return;
            }

            var userId = activeUserId.Value;

            // Critical guard: only accept scans when the authenticated kiosk dashboard
            // is actually connected and joined to this scanner group.
            if (!_scannerConnectionTracker.IsUserActiveOnScanner(deviceId, userId))
            {
                _logger.LogInformation(
                    "RFID scan ignored because no active kiosk dashboard session is connected for scanner {DeviceId} and user {UserId}",
                    deviceId,
                    userId);
                return;
            }

            string action;

            if (item.Status == ItemStatus.Available)
            {
                action = CartActions.Borrow;
            }
            else if (item.Status == ItemStatus.Borrowed)
            {
                action = CartActions.Return;
            }
            else
            {
                _logger.LogWarning("Item {ItemName} is in invalid state: {Status}", item.ItemName, item.Status);
                return;
            }

            // Debounce: avoid duplicates in the server-side cart
            if (_sessionManager.IsItemInCart(userId, item.ItemId))
            {
                _logger.LogInformation("Item {ItemId} already in cart for user {UserId}, ignoring duplicate scan", item.ItemId, userId);
                return;
            }

            var cartItem = new CartItemDto
            {
                ItemId = item.ItemId,
                RfidUid = item.RfidUid,
                ItemName = item.ItemName,
                Action = action,
                ScannedAt = DateTime.UtcNow
            };

            if (_sessionManager.AddItemToCart(userId, cartItem))
            {
                var cart = _sessionManager.GetUserCart(userId);

                await _kioskHubContext.Clients.Group($"scanner_{deviceId}")
                    .SendAsync("CartUpdated", cart);

                _logger.LogInformation("Item {ItemName} added to cart for user {UserId} via scanner {DeviceId}",
                    item.ItemName, userId, deviceId);
            }

            _logger.LogInformation("RFID scan processed: Item={ItemName}, Action={Action}, Device={DeviceId}",
                item.ItemName, action, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RFID scan for UID: {RfidUid}", scanData.RfidUid);
        }
    }

    private async Task ProcessRfidLoginAsync(RfidScanMessage scanData, string deviceId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        try
        {
            // Extract the actual RFID UID from the login card (remove the LOGIN: prefix)
            var rfidTagUid = scanData.RfidUid.Substring(RfidPrefixes.UserLogin.Length);

            _logger.LogInformation("Processing RFID login from device {DeviceId} with tag {RfidTagUid}",
                deviceId, rfidTagUid);

            // Check if scanner exists
            var scanner = await context.Scanners
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId);

            if (scanner == null)
            {
                _logger.LogWarning("RFID login attempt from unknown scanner: {DeviceId}", deviceId);
                // Broadcast to both hubs (LoginHub for unauthenticated clients, KioskHub for authenticated)
                await Task.WhenAll(
                    _loginHubContext.Clients.Group($"scanner_{deviceId}")
                        .SendAsync(HubEvents.LoginFailed, new { Error = "Unknown scanner device" }),
                    _kioskHubContext.Clients.Group($"scanner_{deviceId}")
                        .SendAsync(HubEvents.LoginFailed, new { Error = "Unknown scanner device" })
                );
                return;
            }

            // Try to authenticate user via RFID
            var authResult = await authService.LoginWithRfidAsync(rfidTagUid, scanner.Name);

            if (authResult == null)
            {
                _logger.LogWarning("RFID login failed: user not found for tag {RfidTagUid}", rfidTagUid);
                await Task.WhenAll(
                    _loginHubContext.Clients.Group($"scanner_{deviceId}")
                        .SendAsync(HubEvents.LoginFailed, new { Error = "User not found for this RFID card" }),
                    _kioskHubContext.Clients.Group($"scanner_{deviceId}")
                        .SendAsync(HubEvents.LoginFailed, new { Error = "User not found for this RFID card" })
                );
                return;
            }

            // Bind user to this scanner
            await _scannerSessionService.BindUserToScannerAsync(authResult.UserId, deviceId);

            var loginResponse = new RfidLoginResponseDto
            {
                Success = true,
                Token = authResult.Token,
                Email = authResult.Email,
                Name = authResult.Name,
                Lastname = authResult.Lastname,
                UserId = authResult.UserId,
                RoleIds = authResult.RoleIds,
                ScannerDeviceId = deviceId,
                ScannerName = scanner.Name
            };

            // Broadcast successful login to both hubs
            await Task.WhenAll(
                _loginHubContext.Clients.Group($"scanner_{deviceId}")
                    .SendAsync(HubEvents.RfidLoginSuccess, loginResponse),
                _kioskHubContext.Clients.Group($"scanner_{deviceId}")
                    .SendAsync(HubEvents.RfidLoginSuccess, loginResponse)
            );

            _logger.LogInformation(
                "RFID login successful: User {Email} (UserId={UserId}) logged in via scanner {DeviceId}",
                authResult.Email, authResult.UserId, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RFID login for UID: {RfidUid}", scanData.RfidUid);
            await Task.WhenAll(
                _loginHubContext.Clients.Group($"scanner_{deviceId}")
                    .SendAsync(HubEvents.LoginFailed, new { Error = "An error occurred during login" }),
                _kioskHubContext.Clients.Group($"scanner_{deviceId}")
                    .SendAsync(HubEvents.LoginFailed, new { Error = "An error occurred during login" })
            );
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Hub Listener Service stopping...");
        
        if (_consumerClient != null)
        {
            try
            {
                await _consumerClient.CloseAsync(stoppingToken);
            }
            catch (EventHubsException ex) when (ex.Reason == EventHubsException.FailureReason.ClientClosed)
            {
                _logger.LogDebug("Event Hubs consumer client was already closed.");
            }
        }

        await base.StopAsync(stoppingToken);
    }
}

public class RfidScanMessage
{
    public string RfidUid { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
