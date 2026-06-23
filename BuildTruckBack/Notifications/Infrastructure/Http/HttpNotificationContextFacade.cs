using System.Net.Http.Json;
using System.Text.Json;
using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Interfaces.ACL;

namespace BuildTruckBack.Notifications.Infrastructure.Http;

public class HttpNotificationContextFacade : INotificationContextFacade
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProjectContextService _projectContextService;
    private readonly IUserContextService _userContextService;
    private readonly ISharedEmailService _emailService;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HttpNotificationContextFacade(
        IHttpClientFactory httpClientFactory,
        IProjectContextService projectContextService,
        IUserContextService userContextService,
        ISharedEmailService emailService)
    {
        _httpClientFactory = httpClientFactory;
        _projectContextService = projectContextService;
        _userContextService = userContextService;
        _emailService = emailService;
    }

    public async Task<int> CreateNotificationForUserAsync(int userId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationService");
            var payload = new
            {
                UserId = userId,
                Type = type.Value,
                Context = context.Value,
                Title = title,
                Message = message,
                Priority = priority?.Value,
                ActionUrl = actionUrl,
                RelatedProjectId = relatedProjectId,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };

            var response = await client.PostAsJsonAsync("/internal/notifications", payload);
            if (!response.IsSuccessStatusCode) return 0;

            var id = await response.Content.ReadFromJsonAsync<int>(JsonOptions);
            return id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating notification via HTTP: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> CreateNotificationForProjectAsync(int projectId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedEntityId = null, string? relatedEntityType = null)
    {
        var userIds = new List<int>();

        var managerId = await _projectContextService.GetProjectManagerIdAsync(projectId);
        if (managerId > 0) userIds.Add(managerId);

        var supervisorId = await _projectContextService.GetProjectSupervisorIdAsync(projectId);
        if (supervisorId.HasValue && supervisorId.Value > 0) userIds.Add(supervisorId.Value);

        if (!userIds.Any()) return 0;

        return await CreateBulkNotificationsAsync(userIds.Distinct().ToList(), type, context,
            title, message, priority, actionUrl, projectId, relatedEntityId, relatedEntityType);
    }

    public async Task<int> CreateNotificationForRoleAsync(UserRole role, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null)
    {
        var userIds = role.Value switch
        {
            "Admin" => (await _userContextService.GetAdminUsersAsync()).ToList(),
            "Manager" => (await _userContextService.GetManagerUsersAsync()).ToList(),
            "Supervisor" => (await _userContextService.GetSupervisorUsersAsync()).ToList(),
            _ => new List<int>()
        };

        if (!userIds.Any()) return 0;

        return await CreateBulkNotificationsAsync(userIds, type, context,
            title, message, priority, actionUrl, relatedProjectId, relatedEntityId, relatedEntityType);
    }

    public async Task<int> CreateCriticalNotificationAsync(int userId, string title, string message,
        string projectName, string? actionUrl = null)
    {
        var notificationId = await CreateNotificationForUserAsync(
            userId, NotificationType.CriticalIncident, NotificationContext.System,
            title, message, NotificationPriority.Critical, actionUrl);

        try
        {
            var email = await _userContextService.GetUserEmailAsync(userId);
            var name = await _userContextService.GetUserNameAsync(userId);
            await _emailService.SendCriticalNotificationEmailAsync(email, name, title, message, projectName, actionUrl);
        }
        catch { }

        return notificationId;
    }

    public Task<bool> ShouldUserReceiveNotificationAsync(int userId, NotificationType type,
        NotificationContext context, NotificationPriority priority)
    {
        return Task.FromResult(true);
    }

    private async Task<int> CreateBulkNotificationsAsync(List<int> userIds, NotificationType type,
        NotificationContext context, string title, string message, NotificationPriority? priority,
        string? actionUrl, int? relatedProjectId, int? relatedEntityId, string? relatedEntityType)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationService");
            var payload = new
            {
                UserIds = userIds,
                Type = type.Value,
                Context = context.Value,
                Title = title,
                Message = message,
                Priority = priority?.Value,
                ActionUrl = actionUrl,
                RelatedProjectId = relatedProjectId
            };

            var response = await client.PostAsJsonAsync("/internal/notifications/bulk", payload);
            if (!response.IsSuccessStatusCode) return 0;

            return await response.Content.ReadFromJsonAsync<int>(JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating bulk notifications via HTTP: {ex.Message}");
            return 0;
        }
    }
}
