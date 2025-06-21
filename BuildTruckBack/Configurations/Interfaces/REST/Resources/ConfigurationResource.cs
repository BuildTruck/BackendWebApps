namespace BuildTruckBack.Configurations.Interfaces.REST.Resources;

public class ConfigurationResource
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Theme { get; set; } = "auto";
    public string Plan { get; set; } = "basic";
    public string NotificationsEnable { get; set; } = "true";
    public string EmailNotifications { get; set; } = "false";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}