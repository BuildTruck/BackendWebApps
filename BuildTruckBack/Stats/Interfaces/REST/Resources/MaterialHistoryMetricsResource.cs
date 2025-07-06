namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing material metrics in history snapshot
/// </summary>
public record MaterialHistoryMetricsResource(
    int TotalMaterials,
    int MaterialsOutOfStock,
    decimal TotalMaterialCost,
    decimal InventoryHealthScore
);