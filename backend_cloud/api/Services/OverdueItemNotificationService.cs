using Microsoft.EntityFrameworkCore;
using RfidWarehouseApi.Data;
using RfidWarehouseApi.Models;

namespace RfidWarehouseApi.Services;

public class OverdueItemNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OverdueItemNotificationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Check every 6 hours
    private const int OverdueDaysThreshold = 7;

    public OverdueItemNotificationService(
        IServiceProvider serviceProvider,
        ILogger<OverdueItemNotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Overdue Item Notification Service started");

        // Wait a bit before first run to let the app fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyOverdueItemsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for overdue items");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndNotifyOverdueItemsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var emailService = scope.ServiceProvider.GetService<IEmailService>();

        var overdueThreshold = DateTime.UtcNow.AddDays(-OverdueDaysThreshold);

        // Find items that are currently borrowed and have been for > 7 days
        // and have NOT already received a reminder email
        var overdueItems = await dbContext.Items
            .Include(i => i.CurrentHolder)
            .Where(i => i.Status == ItemStatus.Borrowed 
                && i.CurrentHolderId != null 
                && !i.ReminderEmailSent) // Only items that haven't been reminded yet
            .Join(
                dbContext.Transactions
                    .Where(t => t.Action == TransactionAction.Checkout)
                    .GroupBy(t => t.ItemId)
                    .Select(g => new { ItemId = g.Key, LastBorrow = g.Max(t => t.Timestamp) }),
                item => item.ItemId,
                trans => trans.ItemId,
                (item, trans) => new { Item = item, LastBorrow = trans.LastBorrow }
            )
            .Where(x => x.LastBorrow < overdueThreshold)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} overdue items that need reminders", overdueItems.Count);

        foreach (var overdueItem in overdueItems)
        {
            var user = overdueItem.Item.CurrentHolder!;
            var daysOverdue = (int)(DateTime.UtcNow - overdueItem.LastBorrow).TotalDays;

            // Send email if email service is available
            if (emailService != null)
            {
                try
                {
                    await emailService.SendOverdueItemEmailAsync(
                        user.Email,
                        user.Name,
                        overdueItem.Item.ItemName,
                        daysOverdue,
                        overdueItem.LastBorrow,
                        cancellationToken);

                    // Mark item as reminded so we don't send again automatically
                    overdueItem.Item.ReminderEmailSent = true;
                    overdueItem.Item.ReminderEmailSentAt = DateTime.UtcNow;

                    _logger.LogInformation(
                        "Sent overdue reminder email for user {UserId} ({Email}) - Item: {ItemName} ({ItemId}), Days overdue: {Days}",
                        user.UserId, user.Email, overdueItem.Item.ItemName, overdueItem.Item.ItemId, daysOverdue);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send overdue email to {Email}", user.Email);
                }
            }
            else
            {
                _logger.LogWarning("Email service not available, cannot send overdue reminder");
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
