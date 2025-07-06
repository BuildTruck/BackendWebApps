namespace BuildTruckBack.Stats.Infrastructure.ACL;

using BuildTruckBack.Stats.Application.ACL.Services;
using BuildTruckBack.Stats.Domain.Model.ValueObjects;
using BuildTruckBack.Incidents.Application.Internal;
using Microsoft.Extensions.Logging;

/// <summary>
/// ACL Service implementation for Incidents bounded context
/// </summary>
public class IncidentContextService : IIncidentContextService
{
    private readonly IIncidentFacade _incidentFacade;
    private readonly ILogger<IncidentContextService> _logger;

    public IncidentContextService(
        IIncidentFacade incidentFacade,
        ILogger<IncidentContextService> logger)
    {
        _incidentFacade = incidentFacade ?? throw new ArgumentNullException(nameof(incidentFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncidentMetrics> GetIncidentMetricsAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            _logger.LogDebug("Getting incident metrics for {Count} projects", projectIds.Count);

            if (!projectIds.Any())
            {
                return IncidentMetrics.FromCounts(0, 0, 0);
            }

            var allIncidents = new List<BuildTruckBack.Incidents.Domain.Aggregates.Incident>();

            // Get incidents for each project
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    
                    // Filter by period
                    var filteredIncidents = projectIncidents.Where(i => 
                        period.Contains(i.OccurredAt));

                    allIncidents.AddRange(filteredIncidents);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                    // Continue with other projects
                }
            }

            var totalIncidents = allIncidents.Count;
            var criticalIncidents = allIncidents.Count(i => 
                i.Severity.ToString() == "Alto" || i.Severity.ToString() == "Critical");
            var openIncidents = allIncidents.Count(i => 
                i.Status.ToString() == "Reportado" || 
                i.Status.ToString() == "En Proceso" ||
                i.Status.ToString() == "Open");
            var resolvedIncidents = allIncidents.Count(i => 
                i.Status.ToString() == "Resuelto" || 
                i.Status.ToString() == "Resolved");

            // Group by severity
            var incidentsBySeverity = allIncidents
                .GroupBy(i => i.Severity.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by type
            var incidentsByType = allIncidents
                .Where(i => !string.IsNullOrEmpty(i.IncidentType))
                .GroupBy(i => i.IncidentType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by status
            var incidentsByStatus = allIncidents
                .GroupBy(i => i.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate average resolution time
            var resolvedWithTime = allIncidents
                .Where(i => i.ResolvedAt.HasValue && (
                    i.Status.ToString() == "Resuelto" || 
                    i.Status.ToString() == "Resolved"));

            var averageResolutionTime = resolvedWithTime.Any()
                ? (decimal)resolvedWithTime.Average(i => (i.ResolvedAt!.Value - i.OccurredAt).TotalHours)
                : 0m;

            var metrics = new IncidentMetrics(
                totalIncidents,
                criticalIncidents,
                openIncidents,
                resolvedIncidents,
                incidentsBySeverity,
                incidentsByType,
                incidentsByStatus,
                Math.Round(averageResolutionTime, 2)
            );

            _logger.LogDebug("Incident metrics calculated: {Total} total, {Critical} critical, {Open} open", 
                totalIncidents, criticalIncidents, openIncidents);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incident metrics for projects");
            return IncidentMetrics.FromCounts(0, 0, 0);
        }
    }

    public async Task<Dictionary<string, int>> GetIncidentsBySeverityAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var allIncidents = new List<BuildTruckBack.Incidents.Domain.Aggregates.Incident>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var filteredIncidents = projectIncidents.Where(i => period.Contains(i.OccurredAt));
                    allIncidents.AddRange(filteredIncidents);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return allIncidents
                .GroupBy(i => i.Severity.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by severity for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetIncidentsByTypeAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var allIncidents = new List<BuildTruckBack.Incidents.Domain.Aggregates.Incident>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var filteredIncidents = projectIncidents.Where(i => period.Contains(i.OccurredAt));
                    allIncidents.AddRange(filteredIncidents);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return allIncidents
                .Where(i => !string.IsNullOrEmpty(i.IncidentType))
                .GroupBy(i => i.IncidentType)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by type for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<Dictionary<string, int>> GetIncidentsByStatusAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return new Dictionary<string, int>();

            var allIncidents = new List<BuildTruckBack.Incidents.Domain.Aggregates.Incident>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var filteredIncidents = projectIncidents.Where(i => period.Contains(i.OccurredAt));
                    allIncidents.AddRange(filteredIncidents);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return allIncidents
                .GroupBy(i => i.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting incidents by status for projects");
            return new Dictionary<string, int>();
        }
    }

    public async Task<int> GetTotalIncidentsCountAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var totalCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var filteredIncidents = projectIncidents.Where(i => period.Contains(i.OccurredAt));
                    totalCount += filteredIncidents.Count();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return totalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total incidents count for projects");
            return 0;
        }
    }

    public async Task<int> GetCriticalIncidentsCountAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var criticalCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var criticalIncidents = projectIncidents.Where(i => 
                        (i.Severity.ToString() == "Alto" || i.Severity.ToString() == "Critical") &&
                        period.Contains(i.OccurredAt));
                    criticalCount += criticalIncidents.Count();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return criticalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical incidents count for projects");
            return 0;
        }
    }

    public async Task<int> GetOpenIncidentsCountAsync(List<int> projectIds)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var openCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var openIncidents = projectIncidents.Where(i => 
                        i.Status.ToString() == "Reportado" || 
                        i.Status.ToString() == "En Proceso" ||
                        i.Status.ToString() == "Open");
                    openCount += openIncidents.Count();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return openCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting open incidents count for projects");
            return 0;
        }
    }

    public async Task<int> GetResolvedIncidentsCountAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0;

            var resolvedCount = 0;
            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var resolvedIncidents = projectIncidents.Where(i => 
                        (i.Status.ToString() == "Resuelto" || i.Status.ToString() == "Resolved") &&
                        period.Contains(i.OccurredAt));
                    resolvedCount += resolvedIncidents.Count();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            return resolvedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resolved incidents count for projects");
            return 0;
        }
    }

    public async Task<decimal> GetAverageResolutionTimeAsync(List<int> projectIds, StatsPeriod period)
    {
        try
        {
            if (!projectIds.Any()) return 0m;

            var allResolvedIncidents = new List<BuildTruckBack.Incidents.Domain.Aggregates.Incident>();

            foreach (var projectId in projectIds)
            {
                try
                {
                    var projectIncidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
                    var resolvedIncidents = projectIncidents.Where(i => 
                        (i.Status.ToString() == "Resuelto" || i.Status.ToString() == "Resolved") &&
                        i.ResolvedAt.HasValue &&
                        period.Contains(i.OccurredAt));
                    allResolvedIncidents.AddRange(resolvedIncidents);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting incidents for project {ProjectId}", projectId);
                }
            }

            if (!allResolvedIncidents.Any()) return 0m;

            var averageHours = allResolvedIncidents
                .Average(i => (i.ResolvedAt!.Value - i.OccurredAt).TotalHours);

            return Math.Round((decimal)averageHours, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average resolution time for projects");
            return 0m;
        }
    }
}