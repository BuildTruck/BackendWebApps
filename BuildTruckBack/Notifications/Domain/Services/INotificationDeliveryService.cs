using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Services;

public interface INotificationDeliveryService
{
    Task DeliverAsync(Notification notification, NotificationChannel channel);
    Task RetryFailedDeliveriesAsync();
    Task<bool> CanDeliverAsync(Notification notification, NotificationChannel channel);
    Task<Dictionary<string, int>> GetDeliveryStatsAsync();
}