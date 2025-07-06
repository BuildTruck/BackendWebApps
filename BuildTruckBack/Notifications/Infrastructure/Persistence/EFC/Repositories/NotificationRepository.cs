using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Notifications.Infrastructure.Persistence.EFC.Repositories;

public class NotificationDeliveryRepository : BaseRepository<NotificationDelivery>, INotificationDeliveryRepository
{
    public NotificationDeliveryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<NotificationDelivery>> FindByNotificationIdAsync(int notificationId)
    {
        return await Context.Set<NotificationDelivery>()
            .Where(nd => nd.NotificationId == notificationId)
            .ToListAsync();
    }

    public async Task<NotificationDelivery?> FindByNotificationIdAndChannelAsync(int notificationId, NotificationChannel channel)
    {
        return await Context.Set<NotificationDelivery>()
            .FirstOrDefaultAsync(nd => nd.NotificationId == notificationId && nd.Channel == channel);
    }

    public async Task<IEnumerable<NotificationDelivery>> FindPendingDeliveriesAsync(NotificationChannel? channel = null)
    {
        var query = Context.Set<NotificationDelivery>()
            .Where(nd => nd.Status == DeliveryStatus.Pending || nd.Status == DeliveryStatus.Retrying);

        if (channel != null)
        {
            query = query.Where(nd => nd.Channel == channel);
        }

        return await query
            .OrderBy(nd => nd.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationDelivery>> FindFailedDeliveriesAsync(int maxAttempts = 3)
    {
        return await Context.Set<NotificationDelivery>()
            .Where(nd => nd.Status == DeliveryStatus.Failed && nd.AttemptCount < maxAttempts)
            .OrderBy(nd => nd.LastAttemptAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<NotificationDelivery>> FindRetryableDeliveriesAsync()
    {
        var now = DateTime.UtcNow;
        
        return await Context.Set<NotificationDelivery>()
            .Where(nd => (nd.Status == DeliveryStatus.Failed || nd.Status == DeliveryStatus.Retrying) && 
                        nd.AttemptCount < 3)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNotificationIdAndChannelAsync(int notificationId, NotificationChannel channel)
    {
        return await Context.Set<NotificationDelivery>()
            .AnyAsync(nd => nd.NotificationId == notificationId && nd.Channel == channel);
    }

    public async Task<Dictionary<string, int>> GetDeliveryStatsAsync()
    {
        var stats = await Context.Set<NotificationDelivery>()
            .GroupBy(nd => nd.Status.Value)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        return stats;
    }
}