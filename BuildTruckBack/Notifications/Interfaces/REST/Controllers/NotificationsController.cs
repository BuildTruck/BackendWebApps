using BuildTruckBack.Notifications.Application.Internal.OutboundServices;
using BuildTruckBack.Notifications.Interfaces.REST.Resources;
using BuildTruckBack.Notifications.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Domain.Repositories;
using BuildTruckBack.Notifications.Interfaces.ACL;
using Microsoft.AspNetCore.Authorization;
namespace BuildTruckBack.Notifications.Interfaces.REST.Controllers;
using BuildTruckBack.Notifications.Application.ACL.Services; 

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]  // ← AGREGAR ESTA LÍNEA
[Produces("application/json")]  // ← OPCIONAL PERO RECOMENDADO
public class NotificationsController : ControllerBase
{
    private readonly INotificationFacade _notificationFacade;

    public NotificationsController(INotificationFacade notificationFacade)
    {
        _notificationFacade = notificationFacade;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResource>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] bool? unread = null,
        [FromQuery] string? context = null,
        [FromQuery] int? projectId = null)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var contextFilter = !string.IsNullOrWhiteSpace(context) 
                ? BuildTruckBack.Notifications.Domain.Model.ValueObjects.NotificationContext.FromString(context) 
                : null;

            var notifications = await _notificationFacade.GetUserNotificationsAsync(
                userId, page, size, unread, contextFilter, projectId);

            var resources = NotificationResourceAssembler.ToResourceFromEntity(notifications);
            return Ok(resources);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error retrieving notifications" });
        }
    }

    [HttpPost("mark-read")]
    public async Task<ActionResult> MarkAsRead([FromBody] MarkAsReadResource resource)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        if (!resource.IsValid)
            return BadRequest(new { message = "Must provide either NotificationId or NotificationIds" });

        try
        {
            if (resource.IsSingle && resource.NotificationId.HasValue)
            {
                await _notificationFacade.MarkAsReadAsync(resource.NotificationId.Value, userId);
            }
            else if (resource.IsBulk && resource.NotificationIds != null)
            {
                await _notificationFacade.MarkAllAsReadAsync(resource.NotificationIds, userId);
            }

            return Ok(new { message = "Notifications marked as read successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error marking notifications as read" });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<NotificationSummaryResource>> GetSummary()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var summary = await _notificationFacade.GetNotificationSummaryAsync(userId);
            var resource = NotificationResourceAssembler.ToSummaryResource(summary);
            return Ok(resource);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error retrieving notification summary" });
        }
    }

    [HttpDelete]
    public async Task<ActionResult> CleanOldNotifications([FromBody] CleanNotificationsResource? resource = null)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var daysOld = resource?.DaysOld ?? 30;
            await _notificationFacade.CleanOldNotificationsAsync(userId, daysOld);
            return Ok(new { message = $"Old notifications (older than {daysOld} days) cleaned successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error cleaning old notifications" });
        }
    }
    [HttpPost("test-force-digest")]
    [Authorize(Roles = "Admin")]  // Solo admins pueden forzar resúmenes
    public async Task<ActionResult> TestForceDigest([FromQuery] int? targetUserId = null)
    {
        try
        {
            // Si no se especifica usuario, usar el actual
            var userId = targetUserId ?? GetCurrentUserId();
            
            if (userId == 0)
                return Unauthorized();

            var userService = HttpContext.RequestServices.GetRequiredService<IUserContextService>();
            var emailService = HttpContext.RequestServices.GetRequiredService<ISharedEmailService>();
            var notificationRepository = HttpContext.RequestServices.GetRequiredService<INotificationRepository>();

            Console.WriteLine($"🔍 Buscando notificaciones para usuario {userId}");

            // Obtener TODAS las notificaciones del usuario
            var allNotifications = await notificationRepository.FindByUserIdWithFiltersAsync(
                userId, 1, 50, null, null, null, null);

            var notificationsList = allNotifications.ToList();
            Console.WriteLine($"🔍 Encontradas {notificationsList.Count} notificaciones");

            if (notificationsList.Any())
            {
                var userEmail = await userService.GetUserEmailAsync(userId);
                var userName = await userService.GetUserNameAsync(userId);

                Console.WriteLine($"🔍 Usuario: {userName} - Email: {userEmail}");

                if (!string.IsNullOrEmpty(userEmail))
                {
                    await emailService.SendDigestEmailAsync(
                        userEmail, 
                        userName, 
                        notificationsList, 
                        DateTime.Now
                    );

                    return Ok(new { 
                        success = true, 
                        message = $"✅ Resumen forzado enviado a {userEmail}",
                        notificationCount = notificationsList.Count,
                        userName,
                        userEmail,
                        userId,
                        notifications = notificationsList.Select(n => new {
                            n.Id,
                            title = n.Content.Title,
                            message = n.Content.Message,
                            context = n.Context.Value,
                            priority = n.Priority.Value,
                            createdAt = n.CreatedDate,
                            isRead = n.IsRead
                        }).ToList()
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Email del usuario está vacío",
                        userName,
                        userId
                    });
                }
            }

            return Ok(new { 
                success = false, 
                message = "No hay notificaciones para este usuario",
                userId,
                userName = await userService.GetUserNameAsync(userId)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                success = false, 
                message = $"Error: {ex.Message}",
                details = ex.InnerException?.Message
            });
        }
    }
    
}