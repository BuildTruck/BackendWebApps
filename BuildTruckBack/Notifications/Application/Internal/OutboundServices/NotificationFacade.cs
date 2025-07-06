using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.Commands;
using BuildTruckBack.Notifications.Domain.Model.Queries;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Services;

namespace BuildTruckBack.Notifications.Application.Internal.OutboundServices;

public class NotificationFacade : INotificationFacade
{
    private readonly INotificationCommandService _commandService;
    private readonly INotificationQueryService _queryService;
    private readonly INotificationPreferenceCommandService _preferenceCommandService;
    private readonly INotificationPreferenceQueryService _preferenceQueryService;
    private readonly INotificationDeliveryService _deliveryService;
    private readonly IWebSocketService _webSocketService;

    public NotificationFacade(
        INotificationCommandService commandService,
        INotificationQueryService queryService,
        INotificationPreferenceCommandService preferenceCommandService,
        INotificationPreferenceQueryService preferenceQueryService,
        INotificationDeliveryService deliveryService,
        IWebSocketService webSocketService)
    {
        _commandService = commandService;
        _queryService = queryService;
        _preferenceCommandService = preferenceCommandService;
        _preferenceQueryService = preferenceQueryService;
        _deliveryService = deliveryService;
        _webSocketService = webSocketService;
    }

    public async Task<int> CreateNotificationAsync(int userId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, UserRole? targetRole = null,
        NotificationScope? scope = null, string? actionUrl = null, string? actionText = null,
        string? iconClass = null, int? relatedProjectId = null, int? relatedEntityId = null,
        string? relatedEntityType = null, Dictionary<string, object>? metadata = null)
    {
        var command = new CreateNotificationCommand(
            userId, type, context, priority ?? NotificationPriority.Normal, title, message,
            targetRole ?? UserRole.Manager, scope ?? NotificationScope.User, actionUrl, actionText,
            iconClass, relatedProjectId, relatedEntityId, relatedEntityType, metadata);

        var notificationId = await _commandService.Handle(command);
        
        await _webSocketService.SendUnreadCountUpdateAsync(userId, await GetUnreadCountAsync(userId));
        
        return notificationId;
    }

    public async Task<int> CreateBulkNotificationAsync(List<int> userIds, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, UserRole? targetRole = null,
        NotificationScope? scope = null, string? actionUrl = null, int? relatedProjectId = null)
    {
        var notificationId = 0;
        
        foreach (var userId in userIds)
        {
            notificationId = await CreateNotificationAsync(userId, type, context, title, message, 
                priority, targetRole, scope, actionUrl, relatedProjectId: relatedProjectId);
        }
        
        return notificationId;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int page = 1, int size = 20,
        bool? isRead = null, NotificationContext? context = null, int? relatedProjectId = null)
    {
        var query = new GetNotificationsByUserQuery(userId, page, size, isRead, context, null, relatedProjectId);
        var result = await _queryService.Handle(query);
        return result.ToList();
    }

    public async Task<Dictionary<string, object>> GetNotificationSummaryAsync(int userId)
    {
        var query = new GetNotificationSummaryQuery(userId);
        return await _queryService.Handle(query);
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId, int userId)
    {
        var query = new GetNotificationByIdQuery(notificationId, userId);
        return await _queryService.Handle(query);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var command = new MarkAsReadCommand(notificationId, userId);
        await _commandService.Handle(command);
        
        await _webSocketService.SendNotificationReadAsync(userId, notificationId);
        await _webSocketService.SendUnreadCountUpdateAsync(userId, await GetUnreadCountAsync(userId));
    }

    public async Task MarkAllAsReadAsync(List<int> notificationIds, int userId)
    {
        var command = new BulkMarkAsReadCommand(notificationIds, userId);
        await _commandService.Handle(command);
        
        await _webSocketService.SendUnreadCountUpdateAsync(userId, await GetUnreadCountAsync(userId));
    }

    public async Task CleanOldNotificationsAsync(int userId, int daysOld = 30)
    {
        var command = new CleanOldNotificationsCommand(userId, daysOld);
        await _commandService.Handle(command);
    }

    public async Task<List<NotificationPreference>> GetUserPreferencesAsync(int userId)
    {
        var query = new GetUserPreferencesQuery(userId);
        var result = await _preferenceQueryService.Handle(query);
        return result.ToList();
    }

    public async Task UpdatePreferenceAsync(int userId, NotificationContext context, bool inAppEnabled,
        bool emailEnabled, NotificationPriority minimumPriority)
    {
        var command = new UpdatePreferenceCommand(userId, context, inAppEnabled, emailEnabled, minimumPriority);
        await _preferenceCommandService.Handle(command);
    }

    public async Task CreateDefaultPreferencesAsync(int userId)
    {
        await _preferenceCommandService.CreateDefaultPreferencesAsync(userId);
    }

    public async Task DeliverNotificationAsync(int notificationId, NotificationChannel channel)
    {
        var command = new DeliverNotificationCommand(notificationId, channel);
        await _commandService.Handle(command);
    }

    public async Task<bool> ShouldDeliverAsync(int userId, NotificationType type, NotificationContext context, NotificationPriority priority)
    {
        return true;
    }

    private async Task<int> GetUnreadCountAsync(int userId)
    {
        var summary = await GetNotificationSummaryAsync(userId);
        return (int)summary["unreadCount"];
    }
}