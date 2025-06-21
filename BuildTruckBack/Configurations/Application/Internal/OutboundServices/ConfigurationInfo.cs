namespace BuildTruckBack.Configurations.Application.Internal.OutboundServices;

public record ConfigurationInfo
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Theme { get; init; } = "auto";
    public string Plan { get; init; } = "basic";
    public bool NotificationsEnable { get; init; } = true;
    public bool EmailNotifications { get; init; } = false;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}