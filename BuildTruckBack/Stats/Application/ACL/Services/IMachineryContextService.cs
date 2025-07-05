namespace BuildTruckBack.Stats.Application.ACL.Services;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// ACL Service interface for Machinery bounded context
/// </summary>
public interface IMachineryContextService
{
    /// <summary>
    /// Get machinery metrics for projects within a period
    /// </summary>
    Task<MachineryMetrics> GetMachineryMetricsAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get machinery count by status for projects
    /// </summary>
    Task<Dictionary<string, int>> GetMachineryByStatusAsync(List<int> projectIds);

    /// <summary>
    /// Get machinery count by type for projects
    /// </summary>
    Task<Dictionary<string, int>> GetMachineryByTypeAsync(List<int> projectIds);

    /// <summary>
    /// Get total machinery count for projects
    /// </summary>
    Task<int> GetTotalMachineryCountAsync(List<int> projectIds);

    /// <summary>
    /// Get active machinery count for projects
    /// </summary>
    Task<int> GetActiveMachineryCountAsync(List<int> projectIds);

    /// <summary>
    /// Get machinery in maintenance count for projects
    /// </summary>
    Task<int> GetMachineryInMaintenanceCountAsync(List<int> projectIds);

    /// <summary>
    /// Get inactive machinery count for projects
    /// </summary>
    Task<int> GetInactiveMachineryCountAsync(List<int> projectIds);

    /// <summary>
    /// Get overall availability rate for projects
    /// </summary>
    Task<decimal> GetOverallAvailabilityRateAsync(List<int> projectIds);

    /// <summary>
    /// Get average maintenance time in hours for projects within period
    /// </summary>
    Task<decimal> GetAverageMaintenanceTimeAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get machinery breakdown by project
    /// </summary>
    Task<Dictionary<string, int>> GetMachineryByProjectAsync(List<int> projectIds);
}