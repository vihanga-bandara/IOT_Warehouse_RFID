using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Extensions;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Services;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateMfaToken(User user, string scannerDeviceId, string? scannerName);
    ClaimsPrincipal? ValidateToken(string token);
    MfaTokenData? ValidateMfaToken(string token);
}

/// <summary>
/// Data extracted from an MFA token
/// </summary>
public class MfaTokenData
{
    public int UserId { get; set; }
    public string ScannerDeviceId { get; set; } = string.Empty;
    public string? ScannerName { get; set; }
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private const string MFA_TOKEN_PURPOSE = "mfa_pin_verification";

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT Secret Key not configured");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "480");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.Name} {user.Lastname}"),
            new Claim(CustomClaimTypes.UserId, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var userRight in user.UserRights)
        {
            var roleName = RoleNameExtensions.NormalizeRoleName(userRight.Role?.RoleName, userRight.RoleId);
            claims.Add(new Claim(ClaimTypes.Role, roleName));
            claims.Add(new Claim(CustomClaimTypes.RoleId, userRight.RoleId.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate a short-lived MFA token for PIN verification after RFID scan
    /// </summary>
    public string GenerateMfaToken(User user, string scannerDeviceId, string? scannerName)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT Secret Key not configured");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(CustomClaimTypes.UserId, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("purpose", MFA_TOKEN_PURPOSE),
            new Claim("scanner_device_id", scannerDeviceId),
            new Claim("scanner_name", scannerName ?? string.Empty)
        };

        // MFA tokens expire in 5 minutes - just enough time to enter PIN
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var secretKey = _configuration["Jwt:SecretKey"] 
                ?? throw new InvalidOperationException("JWT Secret Key not configured");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validate an MFA token and extract its data
    /// </summary>
    public MfaTokenData? ValidateMfaToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        // Verify this is an MFA token
        var purposeClaim = principal.FindFirst("purpose")?.Value;
        if (purposeClaim != MFA_TOKEN_PURPOSE) return null;

        var userIdClaim = principal.FindFirst(CustomClaimTypes.UserId)?.Value;
        var scannerDeviceIdClaim = principal.FindFirst("scanner_device_id")?.Value;
        var scannerNameClaim = principal.FindFirst("scanner_name")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return null;

        if (string.IsNullOrEmpty(scannerDeviceIdClaim))
            return null;

        return new MfaTokenData
        {
            UserId = userId,
            ScannerDeviceId = scannerDeviceIdClaim,
            ScannerName = string.IsNullOrEmpty(scannerNameClaim) ? null : scannerNameClaim
        };
    }
}
