using System;

namespace BuildTruckBack.Materials.Interfaces.REST.Resources
{
    public record InventoryItemResource(
        int MaterialId,
        string Name,
        string Type,
        string Unit,
        decimal MinimumStock,
        string Provider,
        decimal StockActual,
        decimal UnitPrice,
        decimal Total
    );
}