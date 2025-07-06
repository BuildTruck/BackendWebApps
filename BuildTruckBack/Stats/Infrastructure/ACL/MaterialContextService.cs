namespace BuildTruckBack.Stats.Infrastructure.ACL;

using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Materials.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// ACL Service implementation for Materials bounded context
/// </summary>
public class MaterialContextService : IMaterialContextService
{
    private readonly IMaterialFacade _materialFacade;
    private readonly ILogger<MaterialContextService> _logger;

    public MaterialContextService(
        IMaterialFacade materialFacade,
        ILogger<MaterialContextService> logger)
    {
        _materialFacade = materialFacade ?? throw new ArgumentNullException(nameof(materialFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MaterialMetrics> GetMaterialMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            _logger.LogDebug("Getting material metrics for {Count} projects", projectIds.Count);

            if (!projectIds.Any())
            {
                return MaterialMetrics.FromCounts(0, 0, 0, 0m);
            }

            var allMaterials = new List<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>();
            decimal totalCost = 0m;
            decimal totalUsageCost = 0m;

            // Get materials for each project
            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                allMaterials.AddRange(projectMaterials);

                // Get project material cost
                var projectCost = await _materialFacade.GetProjectMaterialCostAsync(projectId);
                totalCost += projectCost;
            }

            var totalMaterials = allMaterials.Count;

            // Calculate stock status using Stock.Value instead of calling GetMaterialStockAsync
            var materialsInStock = allMaterials.Count(m => m.Stock.Value > 0);
            var materialsLowStock = allMaterials.Count(m => 
                m.Stock.Value > 0 && m.Stock.Value <= m.MinimumStock.Value);
            var materialsOutOfStock = allMaterials.Count(m => m.Stock.Value <= 0);

            // Group by category (using Type.Value as category)
            var materialsByCategory = allMaterials
                .GroupBy(m => m.Type.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var costsByCategory = allMaterials
                .GroupBy(m => m.Type.Value)
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Price.Value));

            var metrics = new MaterialMetrics(
                totalMaterials,
                materialsInStock,
                materialsLowStock,
                materialsOutOfStock,
                totalCost,
                totalUsageCost,
                materialsByCategory,
                costsByCategory
            );

            _logger.LogDebug("Material metrics calculated: {Total} total, {InStock} in stock, cost: {Cost}", 
                totalMaterials, materialsInStock, totalCost);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material metrics for projects");
            return MaterialMetrics.FromCounts(0, 0, 0, 0m);
        }
    }

    public async Task<Dictionary<string, int>> GetMaterialsByCategoryAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var allMaterials = new List<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>();

            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                allMaterials.AddRange(projectMaterials);
            }

            return allMaterials
                .GroupBy(m => m.Type.Value)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials by category for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<decimal> GetTotalMaterialCostAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0m;

            decimal totalCost = 0m;
            foreach (var projectId in projectIds)
            {
                var projectCost = await _materialFacade.GetProjectMaterialCostAsync(projectId);
                totalCost += projectCost;
            }

            return totalCost;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total material cost for projects");
            return 0m;
        }
    }

    public async Task<decimal> GetTotalUsageCostAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            // This would require a new method in MaterialFacade to get usage costs
            // For now, return 0 as placeholder
            _logger.LogDebug("Getting total usage cost for {Count} projects (placeholder)", projectIds.Count);
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total usage cost for projects");
            return 0m;
        }
    }

    public async Task<int> GetMaterialsInStockCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var allMaterials = new List<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>();

            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                allMaterials.AddRange(projectMaterials);
            }

            return allMaterials.Count(m => m.Stock.Value > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials in stock count for projects");
            return 0;
        }
    }

    public async Task<int> GetMaterialsLowStockCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var allMaterials = new List<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>();

            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                allMaterials.AddRange(projectMaterials);
            }

            return allMaterials.Count(m => 
                m.Stock.Value > 0 && m.Stock.Value <= m.MinimumStock.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials low stock count for projects");
            return 0;
        }
    }

    public async Task<int> GetMaterialsOutOfStockCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var allMaterials = new List<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>();

            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                allMaterials.AddRange(projectMaterials);
            }

            return allMaterials.Count(m => m.Stock.Value <= 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials out of stock count for projects");
            return 0;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostsByCategoryAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, decimal>();

            var allMaterials = new List<BuildTruckBack.Materials.Domain.Model.Aggregates.Material>();

            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                allMaterials.AddRange(projectMaterials);
            }

            return allMaterials
                .GroupBy(m => m.Type.Value)
                .ToDictionary(g => g.Key, g => g.Sum(m => m.Price.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting costs by category for projects");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<int> GetTotalMaterialsCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var totalCount = 0;
            foreach (var projectId in projectIds)
            {
                var projectMaterials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
                totalCount += projectMaterials.Count;
            }

            return totalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total materials count for projects");
            return 0;
        }
    }
}