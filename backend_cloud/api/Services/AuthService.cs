using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Constants;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Models;
using BCrypt.Net;
using System.Security.Cryptography;

namespace RfidWarehouseApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> LoginWithRfidAsync(string rfidTagUid, string? scannerName = null);
    Task<RegisterResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<User?> UpdateUserAsync(int userId, AdminUpdateUserDto dto);
    
    // PIN-related methods
    string GeneratePin();
    Task<string?> ResetUserPinAsync(int userId);
    Task<PinVerificationResponseDto> VerifyPinAsync(VerifyPinDto dto);
    Task<bool> IsUserLockedOutAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly WarehouseDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IScannerSessionService _scannerSessionService;

    public AuthService(
        WarehouseDbContext context,
        ITokenService tokenService,
        ILogger<AuthService> logger,
        IScannerSessionService scannerSessionService)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
        _scannerSessionService = scannerSessionService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRights)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {Email}", loginDto.Email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for user: {Email}", loginDto.Email);
                return null;
            }

            var token = _tokenService.GenerateToken(user);

            var roleIds = user.UserRights.Select(ur => ur.RoleId).ToList();

            // Scanner selection is now handled in a separate step after authentication
            // No longer require or bind scanner at login time

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                UserId = user.UserId,
                RoleIds = roleIds,
                ScannerDeviceId = null,
                ScannerName = null
            };
        }
        catch (InvalidOperationException)
        {
            // Scanner validation errors should be returned to the client as 400.
            // Let the controller map this exception to a helpful message.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginDto.Email);
            return null;
        }
    }

    public async Task<AuthResponseDto?> LoginWithRfidAsync(string rfidTagUid, string? scannerName = null)
    {
        try
        {
            var normalized = rfidTagUid?.Trim() ?? string.Empty;
            if (normalized.StartsWith(RfidPrefixes.UserLogin, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(RfidPrefixes.UserLogin.Length);
            }

            if (string.IsNullOrWhiteSpace(normalized))
            {
                _logger.LogWarning("RFID login attempt with empty tag UID");
                return null;
            }

            var withPrefix = $"{RfidPrefixes.UserLogin}{normalized}";

            // Look up user by RFID tag UID.
            // Accept either raw UID (preferred) or legacy values stored with the LOGIN: prefix.
            var user = await _context.Users
                .Include(u => u.UserRights)
                .FirstOrDefaultAsync(u => u.RfidTagUid == normalized || u.RfidTagUid == withPrefix);

            if (user == null)
            {
                _logger.LogWarning("RFID login attempt with unknown tag: {RfidTagUid}", normalized);
                return null;
            }

            var token = _tokenService.GenerateToken(user);

            var roleIds = user.UserRights.Select(ur => ur.RoleId).ToList();
            var isAdmin = roleIds.Contains(1);

            string? scannerDeviceId = null;
            string? boundScannerName = null;

            // If scanner name is provided, bind the user to that scanner
            if (!string.IsNullOrWhiteSpace(scannerName))
            {
                var binding = await _scannerSessionService.BindUserToScannerAsync(user.UserId, scannerName);
                if (binding != null)
                {
                    scannerDeviceId = binding.Value.DeviceId;
                    boundScannerName = binding.Value.Name;
                }
                else
                {
                    _logger.LogWarning("RFID login: Scanner not found for name {ScannerName}", scannerName);
                }
            }

            _logger.LogInformation("RFID login successful for user {Email} (UserId={UserId})", user.Email, user.UserId);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                UserId = user.UserId,
                RoleIds = roleIds,
                ScannerDeviceId = scannerDeviceId,
                ScannerName = boundScannerName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RFID login for tag: {RfidTagUid}", rfidTagUid);
            return null;
        }
    }

    public async Task<RegisterResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                return null;
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Generate PIN if requested (default: true)
            string? plainPin = null;
            string? pinHash = null;
            if (registerDto.GeneratePin)
            {
                plainPin = GeneratePin();
                pinHash = BCrypt.Net.BCrypt.HashPassword(plainPin);
            }

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Name = registerDto.Name,
                Lastname = registerDto.Lastname,
                RfidTagUid = string.IsNullOrWhiteSpace(registerDto.RfidTagUid)
                    ? null
                    : registerDto.RfidTagUid.Trim().StartsWith(RfidPrefixes.UserLogin, StringComparison.OrdinalIgnoreCase)
                        ? registerDto.RfidTagUid.Trim().Substring(RfidPrefixes.UserLogin.Length)
                        : registerDto.RfidTagUid.Trim(),
                PinHash = pinHash,
                PinFailedAttempts = 0,
                PinLockoutUntil = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign roles (default to User role if not specified)
            var roleIds = registerDto.RoleIds ?? new List<int> { 2 };
            foreach (var roleId in roleIds)
            {
                _context.UserRights.Add(new UserRight
                {
                    UserId = user.UserId,
                    RoleId = roleId
                });
            }
            await _context.SaveChangesAsync();

            // Reload user with UserRights
            user = await _context.Users
                .Include(u => u.UserRights)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            var token = _tokenService.GenerateToken(user!);

            _logger.LogInformation("New user registered: {Email}", user!.Email);

            return new RegisterResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                UserId = user.UserId,
                RoleIds = user.UserRights.Select(ur => ur.RoleId).ToList(),
                GeneratedPin = plainPin
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", registerDto.Email);
            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("ChangePasswordAsync: User not found for id {UserId}", userId);
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                _logger.LogWarning("ChangePasswordAsync: Invalid current password for user {Email}", user.Email);
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password updated for user {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user id {UserId}", userId);
            return false;
        }
    }

    public async Task<User?> UpdateUserAsync(int userId, AdminUpdateUserDto dto)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRights)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Lastname)) user.Lastname = dto.Lastname;
            
            // Handle RFID Tag UID - allow setting or clearing
            if (dto.RfidTagUid != null)
            {
                var trimmed = dto.RfidTagUid?.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    user.RfidTagUid = null;
                }
                else
                {
                    user.RfidTagUid = trimmed.StartsWith(RfidPrefixes.UserLogin, StringComparison.OrdinalIgnoreCase)
                        ? trimmed.Substring(RfidPrefixes.UserLogin.Length)
                        : trimmed;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                _context.UserRights.RemoveRange(user.UserRights);
                foreach (var rid in dto.RoleIds)
                {
                    _context.UserRights.Add(new UserRight { UserId = user.UserId, RoleId = rid });
                }
            }

            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user id {UserId}", userId);
            return null;
        }
    }

    private const int MAX_PIN_ATTEMPTS = 5;

    /// <summary>
    /// Generate a cryptographically secure 4-digit PIN
    /// </summary>
    public string GeneratePin()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var pin = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 10000;
        return pin.ToString("D4");
    }

    /// <summary>
    /// Reset a user's PIN and return the new plain-text PIN
    /// </summary>
    public async Task<string?> ResetUserPinAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("ResetUserPinAsync: User not found for id {UserId}", userId);
                return null;
            }

            var plainPin = GeneratePin();
            user.PinHash = BCrypt.Net.BCrypt.HashPassword(plainPin);
            user.PinFailedAttempts = 0;
            user.PinLockoutUntil = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("PIN reset for user {Email} (UserId={UserId})", user.Email, userId);
            return plainPin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting PIN for user id {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Check if a user is currently locked out due to failed PIN attempts
    /// </summary>
    public async Task<bool> IsUserLockedOutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return true;

        if (user.PinLockoutUntil.HasValue && user.PinLockoutUntil > DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Verify a PIN after RFID login
    /// </summary>
    public async Task<PinVerificationResponseDto> VerifyPinAsync(VerifyPinDto dto)
    {
        // Validate the MFA token
        var mfaData = _tokenService.ValidateMfaToken(dto.MfaToken);
        if (mfaData == null)
        {
            return new PinVerificationResponseDto
            {
                Success = false,
                Error = "Invalid or expired verification session. Please tap your card again."
            };
        }

        var user = await _context.Users
            .Include(u => u.UserRights)
            .FirstOrDefaultAsync(u => u.UserId == mfaData.UserId);

        if (user == null)
        {
            return new PinVerificationResponseDto
            {
                Success = false,
                Error = "User not found."
            };
        }

        // Check if user is locked out
        if (user.PinLockoutUntil.HasValue && user.PinLockoutUntil > DateTime.UtcNow)
        {
            return new PinVerificationResponseDto
            {
                Success = false,
                Error = "Account temporarily locked. Please contact administrator.",
                Locked = true
            };
        }

        // Clear lockout if it has expired
        if (user.PinLockoutUntil.HasValue && user.PinLockoutUntil <= DateTime.UtcNow)
        {
            user.PinLockoutUntil = null;
            user.PinFailedAttempts = 0;
        }

        // Check if user has a PIN set
        if (string.IsNullOrEmpty(user.PinHash))
        {
            return new PinVerificationResponseDto
            {
                Success = false,
                Error = "PIN not configured. Please contact administrator."
            };
        }

        // Verify the PIN
        if (!BCrypt.Net.BCrypt.Verify(dto.Pin, user.PinHash))
        {
            user.PinFailedAttempts++;
            var remaining = MAX_PIN_ATTEMPTS - user.PinFailedAttempts;

            if (user.PinFailedAttempts >= MAX_PIN_ATTEMPTS)
            {
                // Lock out the user - they need to re-scan their card
                user.PinLockoutUntil = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();

                _logger.LogWarning("User {Email} (UserId={UserId}) locked out due to {Attempts} failed PIN attempts",
                    user.Email, user.UserId, MAX_PIN_ATTEMPTS);

                return new PinVerificationResponseDto
                {
                    Success = false,
                    Error = "Too many failed attempts. Please contact administrator if you don't remember your PIN.",
                    Locked = true,
                    RemainingAttempts = 0
                };
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning("Failed PIN attempt for user {Email} (UserId={UserId}). Attempts: {Attempts}/{Max}",
                user.Email, user.UserId, user.PinFailedAttempts, MAX_PIN_ATTEMPTS);

            return new PinVerificationResponseDto
            {
                Success = false,
                Error = $"Incorrect PIN. {remaining} attempt{(remaining == 1 ? "" : "s")} remaining.",
                RemainingAttempts = remaining
            };
        }

        // PIN is correct - reset failed attempts and issue full token
        user.PinFailedAttempts = 0;
        user.PinLockoutUntil = null;
        await _context.SaveChangesAsync();

        // Bind user to scanner
        await _scannerSessionService.BindUserToScannerAsync(user.UserId, mfaData.ScannerDeviceId);

        var token = _tokenService.GenerateToken(user);
        var roleIds = user.UserRights.Select(ur => ur.RoleId).ToList();

        _logger.LogInformation("PIN verified successfully for user {Email} (UserId={UserId})", user.Email, user.UserId);

        return new PinVerificationResponseDto
        {
            Success = true,
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Lastname = user.Lastname,
            UserId = user.UserId,
            RoleIds = roleIds,
            ScannerDeviceId = mfaData.ScannerDeviceId,
            ScannerName = mfaData.ScannerName
        };
    }
}
