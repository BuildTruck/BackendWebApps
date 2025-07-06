namespace BuildTruckBack.Stats.Application.ACL.Services;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// ACL Service interface for Incidents bounded context
/// </summary>
public interface IIncidentContextService
{
    /// <summary>
    /// Get incident metrics for projects within a period
    /// </summary>
    Task<IncidentMetrics> GetIncidentMetricsAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get incidents count by severity for projects within period
    /// </summary>
    Task<Dictionary<string, int>> GetIncidentsBySeverityAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get incidents count by type for projects within period
    /// </summary>
    Task<Dictionary<string, int>> GetIncidentsByTypeAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get incidents count by status for projects within period
    /// </summary>
    Task<Dictionary<string, int>> GetIncidentsByStatusAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get total incidents count for projects within period
    /// </summary>
    Task<int> GetTotalIncidentsCountAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get critical incidents count for projects within period
    /// </summary>
    Task<int> GetCriticalIncidentsCountAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get open incidents count for projects
    /// </summary>
    Task<int> GetOpenIncidentsCountAsync(List<int> projectIds);

    /// <summary>
    /// Get resolved incidents count for projects within period
    /// </summary>
    Task<int> GetResolvedIncidentsCountAsync(List<int> projectIds, StatsPeriod period);

    /// <summary>
    /// Get average resolution time in hours for projects within period
    /// </summary>
    Task<decimal> GetAverageResolutionTimeAsync(List<int> projectIds, StatsPeriod period);
}