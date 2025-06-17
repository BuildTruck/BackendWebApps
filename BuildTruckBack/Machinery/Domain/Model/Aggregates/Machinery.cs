using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildTruckBack.Machinery.Domain.Model.ValueObjects;
using BuildTruckBack.Projects.Domain.Model.Aggregates;

namespace BuildTruckBack.Machinery.Domain.Model.Aggregates;

public class Machinery
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    public DateTime RegisterDate { get; set; }

    [Required]
    public MachineryStatus Status { get; set; }

    [Required]
    [MaxLength(100)]
    public string Provider { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string ProjectId { get; set; } = string.Empty;

    [ForeignKey("ProjectId")]
    public Project Project { get; set; } = null!;
}