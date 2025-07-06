using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using Microsoft.AspNetCore.SignalR;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class WebSocketService : IWebSocketService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public WebSocketService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(int userId, Notification notification)
    {
        var notificationData = new
        {
            id = notification.Id,
            title = notification.Content.Title,
            message = notification.Content.Message,
            type = notification.Type.Value,
            context = notification.Context.Value,
            priority = notification.Priority.Value,
            actionUrl = notification.Content.ActionUrl,
            actionText = notification.Content.ActionText,
            iconClass = notification.Content.IconClass,
            createdAt = notification.CreatedDate,
            isRead = notification.IsRead
        };

        await _hubContext.Clients.Group($"user_{userId}").SendAsync("NewNotification", notificationData);
    }

    public async Task SendToGroupAsync(string groupName, Notification notification)
    {
        var notificationData = new
        {
            id = notification.Id,
            title = notification.Content.Title,
            message = notification.Content.Message,
            type = notification.Type.Value,
            context = notification.Context.Value,
            priority = notification.Priority.Value,
            actionUrl = notification.Content.ActionUrl,
            createdAt = notification.CreatedDate
        };

        await _hubContext.Clients.Group(groupName).SendAsync("NewNotification", notificationData);
    }

    public async Task SendUnreadCountUpdateAsync(int userId, int unreadCount)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdate", new { unreadCount });
    }

    public async Task SendNotificationReadAsync(int userId, int notificationId)
    {
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationRead", new { notificationId });
    }

    public async Task AddUserToGroupAsync(int userId, string groupName)
    {
        await _hubContext.Groups.AddToGroupAsync($"user_{userId}", groupName);
    }

    public async Task RemoveUserFromGroupAsync(int userId, string groupName)
    {
        await _hubContext.Groups.RemoveFromGroupAsync($"user_{userId}", groupName);
    }
}

public class NotificationHub : Hub
{
    public async Task JoinUserGroup(int userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveUserGroup(int userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}