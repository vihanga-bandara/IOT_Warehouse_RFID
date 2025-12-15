using RfidWarehouseApi.Constants;

namespace RfidWarehouseApi.Extensions;

public static class RoleNameExtensions
{
    public static string NormalizeRoleName(string? roleName, int roleId)
    {
        if (!string.IsNullOrWhiteSpace(roleName))
        {
            return roleName;
        }

        return roleId == RoleIds.Admin ? Roles.Admin : Roles.User;
    }
}
