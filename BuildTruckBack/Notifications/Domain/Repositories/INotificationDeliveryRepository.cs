using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Notifications.Domain.Repositories;

public interface INotificationDeliveryRepository : IBaseRepository<NotificationDelivery>
{
    Task<IEnumerable<NotificationDelivery>> FindByNotificationIdAsync(int notificationId);
    Task<NotificationDelivery?> FindByNotificationIdAndChannelAsync(int notificationId, NotificationChannel channel);
    Task<IEnumerable<NotificationDelivery>> FindPendingDeliveriesAsync(NotificationChannel? channel = null);
    Task<IEnumerable<NotificationDelivery>> FindFailedDeliveriesAsync(int maxAttempts = 3);
    Task<IEnumerable<NotificationDelivery>> FindRetryableDeliveriesAsync();
    Task<bool> ExistsByNotificationIdAndChannelAsync(int notificationId, NotificationChannel channel);
    Task<Dictionary<string, int>> GetDeliveryStatsAsync();
}