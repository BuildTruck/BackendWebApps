namespace BuildTruckBack.Stats.Infrastructure.ACL;

using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Machinery.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// ACL Service implementation for Machinery bounded context
/// </summary>
public class MachineryContextService : IMachineryContextService
{
    private readonly IMachineryFacade _machineryFacade;
    private readonly ILogger<MachineryContextService> _logger;

    public MachineryContextService(
        IMachineryFacade machineryFacade,
        ILogger<MachineryContextService> logger)
    {
        _machineryFacade = machineryFacade ?? throw new ArgumentNullException(nameof(machineryFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MachineryMetrics> GetMachineryMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            _logger.LogDebug("Getting machinery metrics for {Count} projects", projectIds.Count);

            if (!projectIds.Any())
            {
                return MachineryMetrics.FromCounts(0, 0, 0);
            }

            var allMachinery = new List<BuildTruckBack.Machinery.Domain.Model.Aggregates.Machinery>();

            // Get machinery for each project
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    allMachinery.AddRange(projectMachinery);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            var totalMachinery = allMachinery.Count;
            var activeMachinery = allMachinery.Count(m => IsActiveStatus(m.Status));
            var inMaintenanceMachinery = allMachinery.Count(m => IsMaintenanceStatus(m.Status));
            var inactiveMachinery = allMachinery.Count(m => IsInactiveStatus(m.Status));

            // Group by status
            var machineryByStatus = allMachinery
                .Where(m => !string.IsNullOrEmpty(m.Status))
                .GroupBy(m => m.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by type
            var machineryByType = allMachinery
                .Where(m => !string.IsNullOrEmpty(m.MachineryType))
                .GroupBy(m => m.MachineryType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by project
            var machineryByProject = allMachinery
                .GroupBy(m => m.ProjectId)
                .ToDictionary(g => $"Project {g.Key}", g => g.Count());

            // Calculate overall availability rate
            var overallAvailabilityRate = totalMachinery > 0
                ? Math.Round((decimal)activeMachinery / totalMachinery * 100, 2)
                : 0m;

            var metrics = new MachineryMetrics(
                totalMachinery,
                activeMachinery,
                inMaintenanceMachinery,
                inactiveMachinery,
                machineryByStatus,
                machineryByType,
                machineryByProject,
                overallAvailabilityRate,
                0m // averageMaintenanceTime - would need additional data
            );

            _logger.LogDebug("Machinery metrics calculated: {Total} total, {Active} active, {Maintenance} in maintenance", 
                totalMachinery, activeMachinery, inMaintenanceMachinery);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery metrics for projects");
            return MachineryMetrics.FromCounts(0, 0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetMachineryByStatusAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var allMachinery = new List<BuildTruckBack.Machinery.Domain.Model.Aggregates.Machinery>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    allMachinery.AddRange(projectMachinery);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return allMachinery
                .Where(m => !string.IsNullOrEmpty(m.Status))
                .GroupBy(m => m.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery by status for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetMachineryByTypeAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var allMachinery = new List<BuildTruckBack.Machinery.Domain.Model.Aggregates.Machinery>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    allMachinery.AddRange(projectMachinery);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return allMachinery
                .Where(m => !string.IsNullOrEmpty(m.MachineryType))
                .GroupBy(m => m.MachineryType)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery by type for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<int> GetTotalMachineryCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var totalCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    totalCount += projectMachinery.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return totalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total machinery count for projects");
            return 0;
        }
    }

    public async Task<int> GetActiveMachineryCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var activeCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    activeCount += projectMachinery.Count(m => IsActiveStatus(m.Status));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return activeCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active machinery count for projects");
            return 0;
        }
    }

    public async Task<int> GetMachineryInMaintenanceCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var maintenanceCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    maintenanceCount += projectMachinery.Count(m => IsMaintenanceStatus(m.Status));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return maintenanceCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery in maintenance count for projects");
            return 0;
        }
    }

    public async Task<int> GetInactiveMachineryCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var inactiveCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    inactiveCount += projectMachinery.Count(m => IsInactiveStatus(m.Status));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return inactiveCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inactive machinery count for projects");
            return 0;
        }
    }

    public async Task<decimal> GetOverallAvailabilityRateAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0m;

            var totalCount = 0;
            var activeCount = 0;

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    totalCount += projectMachinery.Count;
                    activeCount += projectMachinery.Count(m => IsActiveStatus(m.Status));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return totalCount > 0 
                ? Math.Round((decimal)activeCount / totalCount * 100, 2)
                : 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overall availability rate for projects");
            return 0m;
        }
    }

    public async Task<decimal> GetAverageMaintenanceTimeAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            // This would require additional data about maintenance schedules/history
            // For now, return a placeholder value
            _logger.LogDebug("Getting average maintenance time for {Count} projects (placeholder)", projectIds.Count);
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average maintenance time for projects");
            return 0m;
        }
    }

    public async Task<Dictionary<string, int>> GetMachineryByProjectAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var machineryByProject = new Dictionary<string, int>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectMachinery = await _machineryFacade.GetMachineryByProjectAsync(projectId);
                    machineryByProject[$"Project {projectId}"] = projectMachinery.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting machinery for project {ProjectId}", projectId);
                }
            }

            return machineryByProject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting machinery by project for projects");
            return new Dictionary<string, int>();
        }
    }

    // Helper methods to determine machinery status
    private static bool IsActiveStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        
        var normalizedStatus = status.ToLowerInvariant();
        return normalizedStatus == "active" || 
               normalizedStatus == "activo" || 
               normalizedStatus == "operational" ||
               normalizedStatus == "operacional";
    }

    private static bool IsMaintenanceStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return false;
        
        var normalizedStatus = status.ToLowerInvariant();
        return normalizedStatus == "maintenance" || 
               normalizedStatus == "mantenimiento" || 
               normalizedStatus == "repair" ||
               normalizedStatus == "reparacion";
    }

    private static bool IsInactiveStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return true; // Default to inactive if no status
        
        var normalizedStatus = status.ToLowerInvariant();
        return normalizedStatus == "inactive" || 
               normalizedStatus == "inactivo" || 
               normalizedStatus == "offline" ||
               normalizedStatus == "down" ||
               normalizedStatus == "broken" ||
               normalizedStatus == "fuera_de_servicio";
    }
}