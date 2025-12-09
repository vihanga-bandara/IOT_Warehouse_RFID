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
    private readonly ILogger<IoTHubListenerService> _logger;
    private readonly IConfiguration _configuration;
    private EventHubConsumerClient? _consumerClient;

    public IoTHubListenerService(
        IServiceProvider serviceProvider,
        IHubContext<KioskHub> hubContext,
        ICheckoutSessionManager sessionManager,
        ILogger<IoTHubListenerService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _sessionManager = sessionManager;
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
                .FirstOrDefaultAsync(s => s.DeviceIdString == scanData.DeviceId);

            if (scanner == null)
            {
                _logger.LogWarning("Unknown scanner device: {DeviceId}", scanData.DeviceId);
                return;
            }

            // Determine action based on item status
            string action;
            int? targetUserId = null;

            if (item.Status == ItemStatus.Available)
            {
                action = "Borrow";
                // In real scenario, we'd need to know which user is at the kiosk
                // For now, we'll just log this - the frontend will need to handle user context
                _logger.LogInformation("Item {ItemName} scanned for borrowing", item.ItemName);
            }
            else if (item.Status == ItemStatus.Borrowed)
            {
                action = "Return";
                targetUserId = item.CurrentHolderId;
                
                if (targetUserId.HasValue)
                {
                    // Check if item is already in cart (debounce)
                    if (!_sessionManager.IsItemInCart(targetUserId.Value, item.ItemId))
                    {
                        var cartItem = new CartItemDto
                        {
                            ItemId = item.ItemId,
                            RfidUid = item.RfidUid,
                            ItemName = item.ItemName,
                            Action = action,
                            ScannedAt = DateTime.UtcNow
                        };

                        // Add to session
                        if (_sessionManager.AddItemToCart(targetUserId.Value, cartItem))
                        {
                            // Push update via SignalR
                            await _hubContext.Clients.Group($"user_{targetUserId.Value}")
                                .SendAsync("CartUpdated", cartItem);

                            _logger.LogInformation("Item {ItemName} added to cart for user {UserId}", item.ItemName, targetUserId.Value);
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("Item {ItemName} is in invalid state: {Status}", item.ItemName, item.Status);
                return;
            }

            // For "Borrow" action, we need active user context from frontend
            // The frontend should establish this when user logs in at kiosk
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
            await _consumerClient.CloseAsync(stoppingToken);
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
