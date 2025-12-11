using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.DTOs;
using RfidWarehouseApi.Models;
using BCrypt.Net;

namespace RfidWarehouseApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<User?> UpdateUserAsync(int userId, AdminUpdateUserDto dto);
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
            var isAdmin = roleIds.Contains(1);

            string? scannerDeviceId = null;
            string? scannerName = null;

            // For non-admin users, scanner name is required because they go directly to kiosk
            if (!isAdmin)
            {
                if (string.IsNullOrWhiteSpace(loginDto.ScannerName))
                {
                    _logger.LogWarning("Login failed for user {Email}: scanner name is required for non-admin users", loginDto.Email);
                    return null;
                }

                var binding = await _scannerSessionService.BindUserToScannerAsync(user.UserId, loginDto.ScannerName);
                if (binding == null)
                {
                    _logger.LogWarning("Login failed for user {Email}: scanner not found for name {ScannerName}", loginDto.Email, loginDto.ScannerName);
                    return null;
                }

                scannerDeviceId = binding.Value.DeviceId;
                scannerName = binding.Value.Name;
            }
            else
            {
                // Admins can log in without scanner; they will choose scanner later
                if (!string.IsNullOrWhiteSpace(loginDto.ScannerName))
                {
                    var binding = await _scannerSessionService.BindUserToScannerAsync(user.UserId, loginDto.ScannerName);
                    if (binding != null)
                    {
                        scannerDeviceId = binding.Value.DeviceId;
                        scannerName = binding.Value.Name;
                    }
                }
            }

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                UserId = user.UserId,
                RoleIds = roleIds,
                ScannerDeviceId = scannerDeviceId,
                ScannerName = scannerName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginDto.Email);
            return null;
        }
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
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

            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                Name = registerDto.Name,
                Lastname = registerDto.Lastname,
                RfidTagUid = registerDto.RfidTagUid
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

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                UserId = user.UserId,
                RoleIds = user.UserRights.Select(ur => ur.RoleId).ToList()
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
}
