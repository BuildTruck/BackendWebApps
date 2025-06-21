using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildTruckBack.Users.Domain.Model.Aggregates;

namespace BuildTruckBack.Configurations.Domain.Model.Aggregates;

public class Configuration
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string Theme { get; set; } = "auto";

    [Required]
    [MaxLength(20)]
    public string Plan { get; set; } = "basic";

    [Required]
    public bool NotificationsEnable { get; set; } = true;

    [Required]
    public bool EmailNotifications { get; set; } = false;

    // Handled by CreatedUpdatedInterceptor
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}