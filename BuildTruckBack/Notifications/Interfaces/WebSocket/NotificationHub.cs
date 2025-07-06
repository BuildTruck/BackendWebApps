using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BuildTruckBack.Notifications.Interfaces.WebSocket;

public class NotificationHub : Hub
{
    public async Task JoinUserGroup()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Clients.Caller.SendAsync("JoinedUserGroup", new { userId, message = "Connected to notifications" });
        }
    }

    public async Task LeaveUserGroup()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Clients.Caller.SendAsync("LeftUserGroup", new { userId, message = "Disconnected from notifications" });
        }
    }

    public async Task JoinProjectGroup(int projectId)
    {
        var userId = GetCurrentUserId();
        if (userId > 0 && projectId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
            await Clients.Caller.SendAsync("JoinedProjectGroup", new { projectId, message = "Connected to project notifications" });
        }
    }

    public async Task LeaveProjectGroup(int projectId)
    {
        var userId = GetCurrentUserId();
        if (userId > 0 && projectId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
            await Clients.Caller.SendAsync("LeftProjectGroup", new { projectId, message = "Disconnected from project notifications" });
        }
    }

    public async Task MarkNotificationAsRead(int notificationId)
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Clients.Group($"user_{userId}").SendAsync("NotificationMarkedAsRead", new { notificationId, userId });
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Clients.Caller.SendAsync("Connected", new { 
                userId, 
                connectionId = Context.ConnectionId,
                message = "Successfully connected to BuildTruck notifications" 
            });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    public async Task RequestUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Clients.Caller.SendAsync("UnreadCountRequested", new { userId });
        }
    }

    public async Task Ping()
    {
        await Clients.Caller.SendAsync("Pong", new { timestamp = DateTime.UtcNow });
    }
}