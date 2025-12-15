namespace RfidWarehouseApi.Constants;

public static class HubGroups
{
    private const string UserPrefix = "user_";
    private const string ScannerPrefix = "scanner_";

    public static string User(string userId) => UserPrefix + userId;
    public static string Scanner(string deviceId) => ScannerPrefix + deviceId;
}
