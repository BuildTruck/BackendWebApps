namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing material metrics
/// </summary>
public record MaterialMetricsResource(
    int TotalMaterials,
    int MaterialsInStock,
    int MaterialsLowStock,
    int MaterialsOutOfStock,
    decimal TotalMaterialCost,
    decimal TotalUsageCost,
    Dictionary<string, int> MaterialsByCategory,
    Dictionary<string, decimal> CostsByCategory,
    decimal AverageUsageRate,
    decimal StockRate,
    decimal LowStockRate,
    decimal OutOfStockRate,
    decimal CostEfficiencyRate,
    decimal AverageMaterialCost,
    string StockStatus,
    bool NeedsRestocking,
    string MostUsedCategory,
    string LargestCategory,
    decimal InventoryHealthScore,
    string MaterialSummary,
    string CostSummary,
    List<string> StockAlerts
);