using System;
using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Materials.Interfaces.REST.Resources
{
    public record UpdateMaterialEntryResource(
        [Required] DateTime Date,
        [Required][Range(0.01, 999999.99)] decimal Quantity,
        [Required][StringLength(100)] string Provider,
        [Required][StringLength(11)] string Ruc,
        [Required][StringLength(50)] string Payment,
        [Required][StringLength(50)] string DocumentType,
        [Required][StringLength(50)] string DocumentNumber,
        [Required][Range(0.01, 9999999.99)] decimal UnitCost,
        [StringLength(500)] string? Observations
    );
}