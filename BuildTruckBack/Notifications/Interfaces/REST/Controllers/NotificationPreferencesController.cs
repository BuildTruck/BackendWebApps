using BuildTruckBack.Notifications.Application.Internal.OutboundServices;
using BuildTruckBack.Notifications.Interfaces.REST.Resources;
using BuildTruckBack.Notifications.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization; 
namespace BuildTruckBack.Notifications.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/notification-preferences")]
[Authorize]  // ← AGREGAR ESTA LÍNEA
[Produces("application/json")]  // ← OPCIONAL PERO RECOMENDADO
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationFacade _notificationFacade;

    public NotificationPreferencesController(INotificationFacade notificationFacade)
    {
        _notificationFacade = notificationFacade;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationPreferenceResource>>> GetPreferences()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        try
        {
            var preferences = await _notificationFacade.GetUserPreferencesAsync(userId);
            
            if (!preferences.Any())
            {
                await _notificationFacade.CreateDefaultPreferencesAsync(userId);
                preferences = await _notificationFacade.GetUserPreferencesAsync(userId);
            }

            var resources = NotificationPreferenceResourceAssembler.ToResourceFromEntity(preferences);
            return Ok(resources);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error retrieving notification preferences" });
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdatePreferences([FromBody] List<UpdatePreferenceResource> resources)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            return Unauthorized();

        if (!resources.Any())
            return BadRequest(new { message = "At least one preference must be provided" });

        try
        {
            foreach (var resource in resources)
            {
                var command = NotificationPreferenceResourceAssembler.ToCommandFromResource(userId, resource);
                await _notificationFacade.UpdatePreferenceAsync(
                    command.UserId,
                    command.Context,
                    command.InAppEnabled,
                    command.EmailEnabled,
                    command.MinimumPriority
                );
            }

            return Ok(new { message = "Preferences updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Error updating notification preferences" });
        }
    }
}