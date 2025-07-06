using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Domain.Services;

public interface INotificationRuleService
{
    Task<bool> CanUserReceiveNotificationAsync(int userId, NotificationType type, NotificationContext context, NotificationPriority priority);
    Task<bool> ShouldSendEmailAsync(int userId, NotificationType type, NotificationPriority priority);
    Task<IEnumerable<int>> GetEligibleUsersAsync(NotificationType type, int? projectId = null);
    Task<UserRole> GetUserRoleAsync(int userId);
    Task<IEnumerable<int>> GetUserProjectsAsync(int userId);
}