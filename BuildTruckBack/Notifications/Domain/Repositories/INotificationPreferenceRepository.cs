using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Notifications.Domain.Repositories;

public interface INotificationPreferenceRepository : IBaseRepository<NotificationPreference>
{
    Task<IEnumerable<NotificationPreference>> FindByUserIdAsync(int userId);
    Task<NotificationPreference?> FindByUserIdAndContextAsync(int userId, NotificationContext context);
    Task<bool> ExistsByUserIdAndContextAsync(int userId, NotificationContext context);
    Task CreateDefaultPreferencesAsync(int userId);
    Task<IEnumerable<NotificationPreference>> FindEnabledEmailPreferencesAsync();
    Task<bool> ShouldReceiveNotificationAsync(int userId, NotificationContext context, NotificationPriority priority);
    Task<bool> ShouldReceiveEmailAsync(int userId, NotificationContext context, NotificationPriority priority);
}