// Materials/Interfaces/REST/Resources/CreateMaterialResource.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Materials.Interfaces.REST.Resources
{
    public record CreateMaterialResource(
        [Required] int ProjectId,
        [Required][StringLength(100)] string Name,
        [Required][StringLength(50)] string Type,
        [Required][StringLength(20)] string Unit,
        [Required][Range(0, 999999.99)] decimal MinimumStock,
        [Required][StringLength(100)] string Provider
    );
}