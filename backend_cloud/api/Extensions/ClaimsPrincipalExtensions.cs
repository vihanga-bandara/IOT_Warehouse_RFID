using System.Security.Claims;
using RfidWarehouseApi.Constants;

namespace RfidWarehouseApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal? user, out int userId)
    {
        userId = default;
        var userIdClaim = user?.FindFirst(CustomClaimTypes.UserId)?.Value;
        return !string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out userId);
    }
}
