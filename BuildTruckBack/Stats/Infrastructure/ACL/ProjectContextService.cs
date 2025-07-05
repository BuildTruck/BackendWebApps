namespace BuildTruckBack.Stats.Infrastructure.ACL;

using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Projects.Application.Internal.OutboundServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// ACL Service implementation for Projects bounded context
/// </summary>
public class ProjectContextService : IProjectContextService
{
    private readonly IProjectFacade _projectFacade;
    private readonly ILogger<ProjectContextService> _logger;

    public ProjectContextService(
        IProjectFacade projectFacade,
        ILogger<ProjectContextService> logger)
    {
        _projectFacade = projectFacade ?? throw new ArgumentNullException(nameof(projectFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProjectMetrics> GetProjectMetricsAsync(int managerId, StatsPeriod period)
    {
        try
        {
            _logger.LogDebug("Getting project metrics for manager {ManagerId} in period {Period}", managerId, period);

            // Get all projects for the manager
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);

            // Filter by period if dates are available
            var filteredProjects = projects.Where(p => 
                !p.StartDate.HasValue || 
                period.Contains(p.StartDate.Value)).ToList();

            var totalProjects = filteredProjects.Count;
            var activeProjects = filteredProjects.Count(p => IsActiveStatus(p.State));
            var completedProjects = filteredProjects.Count(p => IsCompletedStatus(p.State));
            var plannedProjects = filteredProjects.Count(p => IsPlannedStatus(p.State));

            // Calculate overdue projects
            var overdueProjects = filteredProjects.Count(p => 
                p.StartDate.HasValue && 
                p.StartDate.Value < DateTime.Now && 
                !IsCompletedStatus(p.State));

            // Build projects by status breakdown
            var projectsByStatus = filteredProjects
                .Where(p => !string.IsNullOrEmpty(p.State))
                .GroupBy(p => p.State)
                .ToDictionary(g => g.Key, g => g.Count());

            var metrics = new ProjectMetrics(
                totalProjects,
                activeProjects,
                completedProjects,
                plannedProjects,
                overdueProjects,
                projectsByStatus
            );

            _logger.LogDebug("Project metrics calculated: {Total} total, {Active} active, {Completed} completed", 
                totalProjects, activeProjects, completedProjects);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project metrics for manager {ManagerId}", managerId);
            
            // Return empty metrics on error
            return ProjectMetrics.FromCounts(0, 0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetProjectsByStatusAsync(int managerId, StatsPeriod period)
    {
        try
        {
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);

            var filteredProjects = projects.Where(p => 
                !p.StartDate.HasValue || 
                period.Contains(p.StartDate.Value));

            return filteredProjects
                .Where(p => !string.IsNullOrEmpty(p.State))
                .GroupBy(p => p.State)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects by status for manager {ManagerId}", managerId);
            return new Dictionary<string, int>();
        }
    }

    public async Task<bool> ManagerHasProjectsAsync(int managerId)
    {
        try
        {
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);
            return projects.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if manager {ManagerId} has projects", managerId);
            return false;
        }
    }

    public async Task<int> GetActiveProjectsCountAsync(int managerId)
    {
        try
        {
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);
            return projects.Count(p => IsActiveStatus(p.State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active projects count for manager {ManagerId}", managerId);
            return 0;
        }
    }

    public async Task<int> GetCompletedProjectsCountAsync(int managerId, StatsPeriod period)
    {
        try
        {
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);

            var completedInPeriod = projects.Where(p => 
                IsCompletedStatus(p.State) &&
                (!p.StartDate.HasValue || 
                 period.Contains(p.StartDate.Value)));

            return completedInPeriod.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed projects count for manager {ManagerId}", managerId);
            return 0;
        }
    }

    public async Task<int> GetOverdueProjectsCountAsync(int managerId)
    {
        try
        {
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);
            var now = DateTime.Now;

            return projects.Count(p => 
                p.StartDate.HasValue && 
                p.StartDate.Value < now && 
                !IsCompletedStatus(p.State));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue projects count for manager {ManagerId}", managerId);
            return 0;
        }
    }

    public async Task<List<int>> GetManagerProjectIdsAsync(int managerId)
    {
        try
        {
            var projects = await _projectFacade.GetProjectsByManagerAsync(managerId);
            return projects.Select(p => p.Id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project IDs for manager {ManagerId}", managerId);
            return new List<int>();
        }
    }

    // Helper methods to determine project status
    private static bool IsActiveStatus(string? state)
    {
        if (string.IsNullOrEmpty(state)) return false;
        
        var normalizedState = state.ToLowerInvariant();
        return normalizedState == "activo" || 
               normalizedState == "active" || 
               normalizedState == "en_progreso" ||
               normalizedState == "in_progress" ||
               normalizedState == "iniciado" ||
               normalizedState == "started";
    }

    private static bool IsCompletedStatus(string? state)
    {
        if (string.IsNullOrEmpty(state)) return false;
        
        var normalizedState = state.ToLowerInvariant();
        return normalizedState == "completado" || 
               normalizedState == "completed" || 
               normalizedState == "finalizado" ||
               normalizedState == "finished" ||
               normalizedState == "terminado" ||
               normalizedState == "done";
    }

    private static bool IsPlannedStatus(string? state)
    {
        if (string.IsNullOrEmpty(state)) return false;
        
        var normalizedState = state.ToLowerInvariant();
        return normalizedState == "planificado" || 
               normalizedState == "planned" || 
               normalizedState == "programado" ||
               normalizedState == "scheduled" ||
               normalizedState == "pendiente" ||
               normalizedState == "pending";
    }
}