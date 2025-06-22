using System;
using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Materials.Interfaces.REST.Resources
{
    public record UpdateMaterialUsageResource(
        [Required] DateTime Date,
        [Required][Range(0.01, 999999.99)] decimal Quantity,
        [Required][StringLength(100)] string Area,
        [Required][StringLength(50)] string UsageType,
        [Required][StringLength(100)] string Worker,
        [StringLength(500)] string? Observations
    );
}