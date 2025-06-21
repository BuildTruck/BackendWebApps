using BuildTruckBack.Configurations.Domain.Model.Aggregates;
using BuildTruckBack.Configurations.Domain.Model.Commands;
using BuildTruckBack.Configurations.Domain.Model.ValueObjects;
using BuildTruckBack.Configurations.Domain.Repositories;
using BuildTruckBack.Projects.Application.ACL.Services;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Users.Application.ACL.Services;

namespace BuildTruckBack.Configurations.Application.Internal.CommandServices;

public class ConfigurationCommandService(
    IConfigurationRepository configurationRepository,
    IUserContextService userContextService,
    IUnitOfWork unitOfWork) : IConfigurationCommandHandler
{
    private static readonly string[] ValidPlans = ["basic", "premium", "enterprise"];

    public async Task<Configuration?> Handle(CreateConfigurationCommand command)
    {
        // Validate User
        var user = await userContextService.FindByIdAsync(command.UserId);
        if (user == null)
            throw new Exception($"User with ID {command.UserId} not found.");

        // Validate Theme
        if (!ConfigurationThemes.IsValid(command.Theme))
            throw new Exception($"Invalid theme: {command.Theme}");

        // Validate Plan
        if (!ValidPlans.Contains(command.Plan.ToLowerInvariant()))
            throw new Exception($"Invalid plan: {command.Plan}");

        // Check if configuration exists
        var existingConfig = await configurationRepository.FindByUserIdAsync(command.UserId);
        if (existingConfig != null)
            throw new Exception($"Configuration for user {command.UserId} already exists.");

        var configuration = new Configuration
        {
            UserId = command.UserId,
            Theme = command.Theme.ToLowerInvariant(),
            Plan = command.Plan.ToLowerInvariant(),
            NotificationsEnable = command.NotificationsEnable,
            EmailNotifications = command.EmailNotifications
        };

        await configurationRepository.AddAsync(configuration);
        await unitOfWork.CompleteAsync();
        return configuration;
    }

    public async Task<Configuration?> Handle(UpdateConfigurationCommand command)
    {
        // Validate User
        var user = await userContextService.FindByIdAsync(command.UserId);
        if (user == null)
            throw new Exception($"User with ID {command.UserId} not found.");

        var configuration = await configurationRepository.FindByUserIdAsync(command.UserId);
        if (configuration == null)
            return null;

        // Validate Theme
        if (!ConfigurationThemes.IsValid(command.Theme))
            throw new Exception($"Invalid theme: {command.Theme}");

        // Validate Plan
        if (!ValidPlans.Contains(command.Plan.ToLowerInvariant()))
            throw new Exception($"Invalid plan: {command.Plan}");

        // Update fields
        configuration.Theme = command.Theme.ToLowerInvariant();
        configuration.Plan = command.Plan.ToLowerInvariant();
        configuration.NotificationsEnable = command.NotificationsEnable;
        configuration.EmailNotifications = command.EmailNotifications;

        configurationRepository.Update(configuration);
        await unitOfWork.CompleteAsync();
        return configuration;
    }
}