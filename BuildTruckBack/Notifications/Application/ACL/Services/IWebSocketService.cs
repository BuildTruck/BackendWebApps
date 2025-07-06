using BuildTruckBack.Notifications.Domain.Model.Aggregates;

namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IWebSocketService
{
    Task SendToUserAsync(int userId, Notification notification);
    Task SendToGroupAsync(string groupName, Notification notification);
    Task SendUnreadCountUpdateAsync(int userId, int unreadCount);
    Task SendNotificationReadAsync(int userId, int notificationId);
    Task AddUserToGroupAsync(int userId, string groupName);
    Task RemoveUserFromGroupAsync(int userId, string groupName);
}