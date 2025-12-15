using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;

namespace RfidWarehouseApi.Services;

public interface IScannerSessionService
{
    Task<(string DeviceId, string Name)?> BindUserToScannerAsync(int userId, string scannerName);
    Task<int?> GetActiveUserForScannerAsync(string deviceId);
}

public class ScannerSessionService : IScannerSessionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScannerSessionService> _logger;

    // In-memory map: deviceId -> userId
    private static readonly ConcurrentDictionary<string, int> _activeScannerUsers = new();

    public ScannerSessionService(IServiceProvider serviceProvider, ILogger<ScannerSessionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<(string DeviceId, string Name)?> BindUserToScannerAsync(int userId, string scannerName)
    {
        if (string.IsNullOrWhiteSpace(scannerName))
        {
            return null;
        }

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();

        var normalized = scannerName.Trim().ToLower();

        // Allow binding by either human-friendly Name (typical) or DeviceId (useful for ops/testing)
        // so the kiosk can be bound using the same identifier as the IoT Hub device.
        var scanner = await context.Scanners.FirstOrDefaultAsync(s =>
            (s.Name != null && s.Name.ToLower() == normalized) ||
            s.DeviceId.ToLower() == normalized);

        if (scanner == null)
        {
            _logger.LogWarning("BindUserToScannerAsync: Scanner not found for name {ScannerName}", scannerName);
            return null;
        }

        _activeScannerUsers[scanner.DeviceId] = userId;

        _logger.LogInformation("Scanner {DeviceId} ({Name}) bound to user {UserId}", scanner.DeviceId, scanner.Name, userId);

        return (scanner.DeviceId, scanner.Name ?? scannerName);
    }

    public Task<int?> GetActiveUserForScannerAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Task.FromResult<int?>(null);
        }

        if (_activeScannerUsers.TryGetValue(deviceId, out var userId))
        {
            return Task.FromResult<int?>(userId);
        }

        return Task.FromResult<int?>(null);
    }
}
