using BuildTruckBack.Notifications.Domain.Model.ValueObjects;

namespace BuildTruckBack.Notifications.Interfaces.ACL;

public interface INotificationContextFacade
{
    Task<int> CreateNotificationForUserAsync(int userId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null);

    Task<int> CreateNotificationForProjectAsync(int projectId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedEntityId = null, string? relatedEntityType = null);

    Task<int> CreateNotificationForRoleAsync(UserRole role, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null);

    Task<int> CreateCriticalNotificationAsync(int userId, string title, string message, 
        string projectName, string? actionUrl = null);

    Task<bool> ShouldUserReceiveNotificationAsync(int userId, NotificationType type, 
        NotificationContext context, NotificationPriority priority);
}