namespace BuildTruckBack.Configurations.Domain.Model.Commands;

public record CreateConfigurationCommand
{
    public int UserId { get; init; }
    public string Theme { get; init; } = "auto";
    public string Plan { get; init; } = "basic";
    public bool NotificationsEnable { get; init; } = true;
    public bool EmailNotifications { get; init; } = false;
}