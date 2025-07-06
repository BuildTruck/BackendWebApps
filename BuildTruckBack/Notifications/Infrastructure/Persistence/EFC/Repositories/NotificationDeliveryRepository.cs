using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> FindByUserIdWithFiltersAsync(int userId, int page, int size, 
        bool? isRead = null, NotificationContext? context = null, 
        NotificationPriority? minimumPriority = null, int? relatedProjectId = null)
    {
        var query = Context.Set<Notification>()
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (context != null)
        {
            query = query.Where(n => n.Context == context);
        }

        if (minimumPriority != null)
        {
            query = query.Where(n => n.Priority.Level >= minimumPriority.Level);
        }

        if (relatedProjectId.HasValue)
        {
            query = query.Where(n => n.RelatedProjectId == relatedProjectId.Value);
        }

        return await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
    }

    public async Task<int> CountUnreadByUserIdAsync(int userId)
    {
        return await Context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Dictionary<string, int>> GetSummaryByUserIdAsync(int userId)
    {
        var notifications = await Context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .Select(n => new { Context = n.Context.Value })  // ← Solo seleccionar lo que necesitamos
            .ToListAsync();


        return notifications
            .GroupBy(x => x.Context)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IEnumerable<Notification>> FindByUserIdAndContextAsync(int userId, NotificationContext context)
    {
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId && n.Context == context)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> FindUnreadByUserIdAsync(int userId)
    {
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> FindOldReadNotificationsAsync(int userId, int daysOld)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        
        return await Context.Set<Notification>()
            .Where(n => n.UserId == userId && 
                       n.IsRead && 
                       n.CreatedDate < cutoffDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsByIdAndUserIdAsync(int notificationId, int userId)
    {
        return await Context.Set<Notification>()
            .AnyAsync(n => n.Id == notificationId && n.UserId == userId);
    }

    public async Task BulkMarkAsReadAsync(List<int> notificationIds, int userId)
    {
        var notifications = await Context.Set<Notification>()
            .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await Context.SaveChangesAsync();
    }

    public async Task DeleteOldNotificationsAsync(int userId, int daysOld)
    {
        var oldNotifications = await FindOldReadNotificationsAsync(userId, daysOld);
        
        foreach (var notification in oldNotifications)
        {
            Context.Set<Notification>().Remove(notification);
        }

        await Context.SaveChangesAsync();
    }
}