using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Hubs;
using RfidWarehouseApi.Models;
using RfidWarehouseApi.Services;

namespace RfidWarehouseApi.Services;

public class IoTHubListenerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<KioskHub> _hubContext;
    private readonly ICheckoutSessionManager _sessionManager;
    private readonly IScannerSessionService _scannerSessionService;
    private readonly ILogger<IoTHubListenerService> _logger;
    private readonly IConfiguration _configuration;
    private EventHubConsumerClient? _consumerClient;

    public IoTHubListenerService(
        IServiceProvider serviceProvider,
        IHubContext<KioskHub> hubContext,
        ICheckoutSessionManager sessionManager,
        IScannerSessionService scannerSessionService,
        ILogger<IoTHubListenerService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _sessionManager = sessionManager;
        _scannerSessionService = scannerSessionService;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Hub Listener Service starting...");

        var connectionString = _configuration["IoTHub:EventHubConnectionString"];
        var consumerGroup = _configuration["IoTHub:ConsumerGroup"] ?? "$Default";

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("IoT Hub connection string not configured. Service will not start.");
            return;
        }

        try
        {
            _consumerClient = new EventHubConsumerClient(consumerGroup, connectionString);
            _logger.LogInformation("Connected to IoT Hub Event Hub endpoint");

            await foreach (var partitionEvent in _consumerClient.ReadEventsAsync(stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                if (partitionEvent.Data == null) continue;

                try
                {
                    var eventBody = partitionEvent.Data.EventBody.ToString();
                    var scanData = JsonSerializer.Deserialize<RfidScanMessage>(eventBody);

                    if (scanData != null)
                    {
                        await ProcessRfidScanAsync(scanData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing IoT Hub message");
                }
            }
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

    private async Task ProcessRfidScanAsync(RfidScanMessage scanData)
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

            // Get scanner info
            var scanner = await context.Scanners
                .FirstOrDefaultAsync(s => s.DeviceId == scanData.DeviceId);

            if (scanner == null)
            {
                _logger.LogWarning("Unknown scanner device: {DeviceId}", scanData.DeviceId);
                return;
            }

            // Determine which user is currently bound to this scanner
            var activeUserId = await _scannerSessionService.GetActiveUserForScannerAsync(scanData.DeviceId);

            if (!activeUserId.HasValue)
            {
                _logger.LogInformation("RFID scan ignored because no active user is bound to scanner {DeviceId}", scanData.DeviceId);
                return;
            }

            var userId = activeUserId.Value;

            // Determine action based on item status
            string action;

            if (item.Status == ItemStatus.Available)
            {
                action = "Borrow";
            }
            else if (item.Status == ItemStatus.Borrowed)
            {
                action = "Return";
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

                await _hubContext.Clients.Group($"scanner_{scanData.DeviceId}")
                    .SendAsync("CartUpdated", cart);

                _logger.LogInformation("Item {ItemName} added to cart for user {UserId} via scanner {DeviceId}",
                    item.ItemName, userId, scanData.DeviceId);
            }

            _logger.LogInformation("RFID scan processed: Item={ItemName}, Action={Action}, Device={DeviceId}",
                item.ItemName, action, scanData.DeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RFID scan for UID: {RfidUid}", scanData.RfidUid);
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
