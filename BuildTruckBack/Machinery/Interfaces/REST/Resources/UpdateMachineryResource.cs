using System.ComponentModel.DataAnnotations;
using BuildTruckBack.Machinery.Domain.Model.ValueObjects;

namespace BuildTruckBack.Machinery.Interfaces.REST.Resources;

public class UpdateMachineryResource
{
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
}