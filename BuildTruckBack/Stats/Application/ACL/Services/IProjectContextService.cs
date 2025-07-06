namespace BuildTruckBack.Stats.Application.ACL.Services;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// ACL Service interface for Projects bounded context
/// </summary>
public interface IProjectContextService
{
    /// <summary>
    /// Get project metrics for a manager within a period
    /// </summary>
    Task<ProjectMetrics> GetProjectMetricsAsync(int managerId, StatsPeriod period);

    /// <summary>
    /// Get project counts by status for a manager
    /// </summary>
    Task<Dictionary<string, int>> GetProjectsByStatusAsync(int managerId, StatsPeriod period);

    /// <summary>
    /// Check if manager has access to projects
    /// </summary>
    Task<bool> ManagerHasProjectsAsync(int managerId);

    /// <summary>
    /// Get active projects count for a manager
    /// </summary>
    Task<int> GetActiveProjectsCountAsync(int managerId);

    /// <summary>
    /// Get completed projects count for a manager within period
    /// </summary>
    Task<int> GetCompletedProjectsCountAsync(int managerId, StatsPeriod period);

    /// <summary>
    /// Get overdue projects count for a manager
    /// </summary>
    Task<int> GetOverdueProjectsCountAsync(int managerId);

    /// <summary>
    /// Get all project IDs managed by a manager
    /// </summary>
    Task<List<int>> GetManagerProjectIdsAsync(int managerId);
}