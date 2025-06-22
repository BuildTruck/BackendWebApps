using System;

namespace BuildTruckBack.Materials.Interfaces.REST.Resources
{
    public record MaterialUsageResource(
        int Id,         // Cambiado de Guid a int
        int ProjectId,
        int MaterialId, // Cambiado de Guid a int
        DateTime Date,
        decimal Quantity,
        string Area,
        string UsageType,
        string Worker,
        string Status,
        string? Observations
    );
}