using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildTruckBack.Machinery.Domain.Model.ValueObjects;
using BuildTruckBack.Projects.Domain.Model.Aggregates;

namespace BuildTruckBack.Machinery.Domain.Model.Aggregates;

public class Machinery
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string MachineryType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "active";
    
    [MaxLength(100)]
    public string Provider { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public int? PersonnelId { get; set; }
    
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime RegisterDate { get; set; } = DateTime.UtcNow.Date;

    // Domain methods
    public bool IsActive() => Status == "active";
    public bool IsInMaintenance() => Status == "maintenance";
}