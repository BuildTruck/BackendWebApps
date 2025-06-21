using BuildTruckBack.Configurations.Application.Internal.OutboundServices;
using BuildTruckBack.Configurations.Interfaces.REST.Resources;
using BuildTruckBack.Configurations.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BuildTruckBack.Configurations.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ConfigurationsController(IConfigurationFacade configurationFacade) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get user configuration", Description = "Retrieves the configuration settings for the authenticated user.")]
    public async Task<IActionResult> GetConfiguration()
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        var configurationInfo = await configurationFacade.GetConfigurationByUserIdAsync(userId);
        if (configurationInfo == null)
            return NotFound($"Configuration for user {userId} not found.");

        var resource = ConfigurationResourceAssembler.ToResourceFromInfo(configurationInfo);
        return Ok(resource);
    }

    [HttpPut]
    [SwaggerOperation(Summary = "Update user configuration", Description = "Updates or creates the configuration settings for the authenticated user.")]
    public async Task<IActionResult> UpdateConfiguration([FromBody] UpdateConfigurationResource resource)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
        var existingConfig = await configurationFacade.GetConfigurationByUserIdAsync(userId);
        ConfigurationInfo? updatedConfigurationInfo;

        try
        {
            if (existingConfig == null)
            {
                var createCommand = ConfigurationResourceAssembler.ToCreateCommandFromResource(resource, userId);
                updatedConfigurationInfo = await configurationFacade.CreateConfigurationAsync(createCommand);
            }
            else
            {
                var updateCommand = ConfigurationResourceAssembler.ToUpdateCommandFromResource(resource, userId);
                updatedConfigurationInfo = await configurationFacade.UpdateConfigurationAsync(updateCommand);
            }

            if (updatedConfigurationInfo == null)
                return BadRequest("Failed to update configuration.");

            var updatedResource = ConfigurationResourceAssembler.ToResourceFromInfo(updatedConfigurationInfo);
            return Ok(updatedResource);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}