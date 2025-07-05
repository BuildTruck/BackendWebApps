namespace BuildTruckBack.Stats.Application.ACL.Services;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// ACL Service interface for Materials bounded context
/// </summary>
public interface IMaterialContextService
{
    /// <summary>
    /// Get material metrics for projects within a period
    /// </summary>
    Task<MaterialMetrics> GetMaterialMetricsAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get materials count by category for projects
    /// </summary>
    Task<Dictionary<string, int>> GetMaterialsByCategoryAsync(List<int> projectIds);

    /// <summary>
    /// Get total material cost for projects within period
    /// </summary>
    Task<decimal> GetTotalMaterialCostAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get total usage cost for projects within period
    /// </summary>
    Task<decimal> GetTotalUsageCostAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get materials in stock count for projects
    /// </summary>
    Task<int> GetMaterialsInStockCountAsync(List<int> projectIds);

    /// <summary>
    /// Get materials with low stock count for projects
    /// </summary>
    Task<int> GetMaterialsLowStockCountAsync(List<int> projectIds);

    /// <summary>
    /// Get materials out of stock count for projects
    /// </summary>
    Task<int> GetMaterialsOutOfStockCountAsync(List<int> projectIds);

    /// <summary>
    /// Get costs breakdown by category for projects
    /// </summary>
    Task<Dictionary<string, decimal>> GetCostsByCategoryAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get total materials count for projects
    /// </summary>
    Task<int> GetTotalMaterialsCountAsync(List<int> projectIds);
}