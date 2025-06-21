using BuildTruckBack.Configurations.Application.Internal.OutboundServices;
using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Model.Commands;
using BuildTruckBack.Configurations.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BuildTruckBack.Configurations.Application.Internal.OutboundServices;

public class ConfigurationFacade(
    IConfigurationRepository configurationRepository,
    IConfigurationCommandHandler commandHandler,
    ILogger<ConfigurationFacade> logger) : IConfigurationFacade
{
    public async Task<ConfigurationInfo?> GetConfigurationByUserIdAsync(int userId)
    {
        try
        {
            logger.LogDebug("Getting configuration for user: {UserId}", userId);
            var configuration = await configurationRepository.FindByUserIdAsync(userId);
            if (configuration == null)
            {
                logger.LogDebug("Configuration not found for user: {UserId}", userId);
                return null;
            }

            var configurationInfo = new ConfigurationInfo
            {
                Id = configuration.Id,
                UserId = configuration.UserId,
                Theme = configuration.Theme,
                Plan = configuration.Plan,
                NotificationsEnable = configuration.NotificationsEnable,
                EmailNotifications = configuration.EmailNotifications,
                CreatedAt = configuration.CreatedAt,
                UpdatedAt = configuration.UpdatedAt
            };

            logger.LogDebug("Retrieved configuration for user: {UserId}", userId);
            return configurationInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting configuration for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<ConfigurationInfo?> CreateConfigurationAsync(CreateConfigurationCommand command)
    {
        try
        {
            logger.LogDebug("Creating configuration for user: {UserId}", command.UserId);
            var configuration = await commandHandler.Handle(command);
            if (configuration == null)
            {
                logger.LogDebug("Failed to create configuration for user: {UserId}", command.UserId);
                return null;
            }

            var configurationInfo = new ConfigurationInfo
            {
                Id = configuration.Id,
                UserId = configuration.UserId,
                Theme = configuration.Theme,
                Plan = configuration.Plan,
                NotificationsEnable = configuration.NotificationsEnable,
                EmailNotifications = configuration.EmailNotifications,
                CreatedAt = configuration.CreatedAt,
                UpdatedAt = configuration.UpdatedAt
            };

            logger.LogDebug("Created configuration for user: {UserId}", command.UserId);
            return configurationInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating configuration for user: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<ConfigurationInfo?> UpdateConfigurationAsync(UpdateConfigurationCommand command)
    {
        try
        {
            logger.LogDebug("Updating configuration for user: {UserId}", command.UserId);
            var configuration = await commandHandler.Handle(command);
            if (configuration == null)
            {
                logger.LogDebug("Failed to update configuration for user: {UserId}", command.UserId);
                return null;
            }

            var configurationInfo = new ConfigurationInfo
            {
                Id = configuration.Id,
                UserId = configuration.UserId,
                Theme = configuration.Theme,
                Plan = configuration.Plan,
                NotificationsEnable = configuration.NotificationsEnable,
                EmailNotifications = configuration.EmailNotifications,
                CreatedAt = configuration.CreatedAt,
                UpdatedAt = configuration.UpdatedAt
            };

            logger.LogDebug("Updated configuration for user: {UserId}", command.UserId);
            return configurationInfo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating configuration for user: {UserId}", command.UserId);
            return null;
        }
    }
}