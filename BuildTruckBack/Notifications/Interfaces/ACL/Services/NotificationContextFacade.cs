using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Application.Internal.OutboundServices;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Interfaces.ACL;

namespace BuildTruckBack.Notifications.Interfaces.ACL.Services;

public class NotificationContextFacade : INotificationContextFacade
{
    private readonly INotificationFacade _notificationFacade;
    private readonly IProjectContextService _projectContextService;
    private readonly IUserContextService _userContextService;
    private readonly ISharedEmailService _emailService;

    public NotificationContextFacade(
        INotificationFacade notificationFacade,
        IProjectContextService projectContextService,
        IUserContextService userContextService,
        ISharedEmailService emailService)
    {
        _notificationFacade = notificationFacade;
        _projectContextService = projectContextService;
        _userContextService = userContextService;
        _emailService = emailService;
    }

    public async Task<int> CreateNotificationForUserAsync(int userId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null)
    {
        var userRole = await GetUserRoleAsync(userId);
        var scope = relatedProjectId.HasValue ? NotificationScope.Project : NotificationScope.User;

        return await _notificationFacade.CreateNotificationAsync(
            userId, type, context, title, message, priority, userRole, scope, 
            actionUrl, null, null, relatedProjectId, relatedEntityId, relatedEntityType);
    }

    public async Task<int> CreateNotificationForProjectAsync(int projectId, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedEntityId = null, string? relatedEntityType = null)
    {
        var userIds = await GetProjectUsersAsync(projectId);
        var notificationId = 0;

        foreach (var userId in userIds)
        {
            notificationId = await CreateNotificationForUserAsync(userId, type, context, title, message, 
                priority, actionUrl, projectId, relatedEntityId, relatedEntityType);
        }

        return notificationId;
    }

    public async Task<int> CreateNotificationForRoleAsync(UserRole role, NotificationType type, NotificationContext context,
        string title, string message, NotificationPriority? priority = null, string? actionUrl = null,
        int? relatedProjectId = null, int? relatedEntityId = null, string? relatedEntityType = null)
    {
        Console.WriteLine($"🔍 DEBUG - Facade recibió rol: '{role.Value}'");
    
        var userIds = await GetUsersByRoleAsync(role);
        Console.WriteLine($"🔍 DEBUG - Usuarios encontrados para rol '{role.Value}': [{string.Join(", ", userIds)}]");
    
        var notificationId = 0;
        
        foreach (var userId in userIds)
        {
            notificationId = await CreateNotificationForUserAsync(userId, type, context, title, message, 
                priority, actionUrl, relatedProjectId, relatedEntityId, relatedEntityType);
        }

        return notificationId;
    }

    public async Task<int> CreateCriticalNotificationAsync(int userId, string title, string message, 
        string projectName, string? actionUrl = null)
    {
        var notificationId = await CreateNotificationForUserAsync(
            userId, 
            NotificationType.CriticalIncident, 
            NotificationContext.System, 
            title, 
            message, 
            NotificationPriority.Critical, 
            actionUrl
        );

        try
        {
            var userEmail = await _userContextService.GetUserEmailAsync(userId);
            var userName = await _userContextService.GetUserNameAsync(userId);
            
            await _emailService.SendCriticalNotificationEmailAsync(userEmail, userName, title, message, projectName, actionUrl);
        }
        catch
        {
            // Email delivery failed, but notification was created
        }

        return notificationId;
    }

    public async Task<bool> ShouldUserReceiveNotificationAsync(int userId, NotificationType type, 
        NotificationContext context, NotificationPriority priority)
    {
        return await _notificationFacade.ShouldDeliverAsync(userId, type, context, priority);
    }

    private async Task<UserRole> GetUserRoleAsync(int userId)
    {
        var roleString = await _userContextService.GetUserRoleAsync(userId);
        return UserRole.FromString(roleString);
    }

    private async Task<IEnumerable<int>> GetProjectUsersAsync(int projectId)
    {
        var userIds = new List<int>();

        var managerId = await _projectContextService.GetProjectManagerIdAsync(projectId);
        if (managerId > 0)
            userIds.Add(managerId);

        var supervisorId = await _projectContextService.GetProjectSupervisorIdAsync(projectId);
        if (supervisorId.HasValue && supervisorId.Value > 0)
            userIds.Add(supervisorId.Value);

        return userIds.Distinct();
    }

    private async Task<IEnumerable<int>> GetUsersByRoleAsync(UserRole role)
    {
        Console.WriteLine($"🔍 DEBUG - GetUsersByRoleAsync llamado con rol: '{role.Value}'");
    
        var result = role.Value switch
        {
            "Admin" => await _userContextService.GetAdminUsersAsync(),
            "Manager" => await _userContextService.GetManagerUsersAsync(),
            "Supervisor" => await _userContextService.GetSupervisorUsersAsync(),
            _ => new List<int>()
        };
    
        Console.WriteLine($"🔍 DEBUG - GetUsersByRoleAsync retorna: [{string.Join(", ", result)}]");
        return result;
    }
}