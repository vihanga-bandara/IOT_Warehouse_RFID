using System.Collections.Concurrent;

namespace RfidWarehouseApi.Services;

public interface IScannerConnectionTracker
{
    void TrackJoin(string deviceId, string connectionId, int userId);
    void TrackDisconnect(string connectionId);
    bool IsUserActiveOnScanner(string deviceId, int userId);
}

// Tracks active kiosk SignalR connections per scanner
public class ScannerConnectionTracker : IScannerConnectionTracker
{
    // deviceId -> (connectionId -> userId)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> _scannerConnections = new();

    // connectionId -> deviceId
    private readonly ConcurrentDictionary<string, string> _connectionToScanner = new();

    public void TrackJoin(string deviceId, string connectionId, int userId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        // If the same connection previously joined a different scanner, remove it first.
        if (_connectionToScanner.TryGetValue(connectionId, out var previousDeviceId)
            && !string.Equals(previousDeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
        {
            RemoveConnectionFromScanner(previousDeviceId, connectionId);
        }

        var connections = _scannerConnections.GetOrAdd(deviceId, _ => new ConcurrentDictionary<string, int>());
        connections[connectionId] = userId;
        _connectionToScanner[connectionId] = deviceId;
    }

    public void TrackDisconnect(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        if (_connectionToScanner.TryRemove(connectionId, out var deviceId))
        {
            RemoveConnectionFromScanner(deviceId, connectionId);
        }
    }

    public bool IsUserActiveOnScanner(string deviceId, int userId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return false;
        }

        if (!_scannerConnections.TryGetValue(deviceId, out var connections) || connections.IsEmpty)
        {
            return false;
        }

        return connections.Values.Any(activeUserId => activeUserId == userId);
    }

    private void RemoveConnectionFromScanner(string deviceId, string connectionId)
    {
        if (!_scannerConnections.TryGetValue(deviceId, out var connections))
        {
            return;
        }

        connections.TryRemove(connectionId, out _);

        if (connections.IsEmpty)
        {
            _scannerConnections.TryRemove(deviceId, out _);
        }
    }
}
