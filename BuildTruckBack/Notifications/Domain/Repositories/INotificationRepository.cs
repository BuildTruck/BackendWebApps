using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Notifications.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<IEnumerable<Notification>> FindByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> FindByUserIdWithFiltersAsync(int userId, int page, int size, 
        bool? isRead = null, NotificationContext? context = null, 
        NotificationPriority? minimumPriority = null, int? relatedProjectId = null);
    Task<int> CountUnreadByUserIdAsync(int userId);
    Task<Dictionary<string, int>> GetSummaryByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> FindByUserIdAndContextAsync(int userId, NotificationContext context);
    Task<IEnumerable<Notification>> FindUnreadByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> FindOldReadNotificationsAsync(int userId, int daysOld);
    Task<bool> ExistsByIdAndUserIdAsync(int notificationId, int userId);
    Task BulkMarkAsReadAsync(List<int> notificationIds, int userId);
    Task DeleteOldNotificationsAsync(int userId, int daysOld);
}