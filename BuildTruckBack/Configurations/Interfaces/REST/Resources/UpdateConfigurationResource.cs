using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Configurations.Interfaces.REST.Resources;

public class UpdateConfigurationResource
{
    [Required]
    [MaxLength(10)]
    public string Theme { get; set; } = "auto";

    [Required]
    [MaxLength(20)]
    public string Plan { get; set; } = "basic";

    [Required]
    public string NotificationsEnable { get; set; } = "true";

    [Required]
    public string EmailNotifications { get; set; } = "false";
}