using System.Collections.Concurrent;
using RfidWarehouseApi.DTOs;

namespace RfidWarehouseApi.Services;

public interface ICheckoutSessionManager
{
    bool AddItemToCart(int userId, CartItemDto item);
    SessionCartDto? GetUserCart(int userId);
    bool RemoveItemFromCart(int userId, int itemId);
    void ClearUserCart(int userId);
    bool IsItemInCart(int userId, int itemId);
}

public class CheckoutSessionManager : ICheckoutSessionManager
{
    private readonly ConcurrentDictionary<int, SessionCartDto> _activeSessions = new();
    private readonly ILogger<CheckoutSessionManager> _logger;

    public CheckoutSessionManager(ILogger<CheckoutSessionManager> logger)
    {
        _logger = logger;
    }

    public bool AddItemToCart(int userId, CartItemDto item)
    {
        try
        {
            var session = _activeSessions.GetOrAdd(userId, _ => new SessionCartDto
            {
                UserId = userId,
                SessionStarted = DateTime.UtcNow,
                Items = new List<CartItemDto>()
            });

            // Debounce: Check if item is already in cart (duplicate prevention)
            if (session.Items.Any(i => i.ItemId == item.ItemId))
            {
                _logger.LogInformation("Item {ItemId} already in cart for user {UserId}, ignoring duplicate scan", item.ItemId, userId);
                return false;
            }

            session.Items.Add(item);
            _logger.LogInformation("Added item {ItemId} to cart for user {UserId}. Action: {Action}", item.ItemId, userId, item.Action);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
            return false;
        }
    }

    public SessionCartDto? GetUserCart(int userId)
    {
        _activeSessions.TryGetValue(userId, out var session);
        return session;
    }

    public bool RemoveItemFromCart(int userId, int itemId)
    {
        if (_activeSessions.TryGetValue(userId, out var session))
        {
            var item = session.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                session.Items.Remove(item);
                _logger.LogInformation("Removed item {ItemId} from cart for user {UserId}", itemId, userId);
                return true;
            }
        }
        return false;
    }

    public void ClearUserCart(int userId)
    {
        if (_activeSessions.TryRemove(userId, out var session))
        {
            _logger.LogInformation("Cleared cart for user {UserId}. Had {ItemCount} items", userId, session.Items.Count);
        }
    }

    public bool IsItemInCart(int userId, int itemId)
    {
        if (_activeSessions.TryGetValue(userId, out var session))
        {
            return session.Items.Any(i => i.ItemId == itemId);
        }
        return false;
    }
}
