using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Services;

public interface INotificationScopeService
{
    Task<IEnumerable<int>> GetUsersInScopeAsync(NotificationScope scope, NotificationType type, int? projectId = null);
    Task<bool> UserHasAccessToNotificationAsync(int userId, NotificationType type, int? projectId = null);
    Task<IEnumerable<int>> GetAdminUsersAsync();
    Task<IEnumerable<int>> GetManagersForProjectAsync(int projectId);
    Task<IEnumerable<int>> GetSupervisorsForProjectAsync(int projectId);
}