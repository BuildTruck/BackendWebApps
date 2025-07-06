namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IIncidentContextService
{
    Task<bool> IncidentExistsAsync(int incidentId);
    Task<string> GetIncidentTitleAsync(int incidentId);
    Task<int> GetIncidentProjectIdAsync(int incidentId);
    Task<bool> IncidentBelongsToProjectAsync(int incidentId, int projectId);
    Task<string> GetIncidentSeverityAsync(int incidentId);
    Task<string> GetIncidentStatusAsync(int incidentId);
    Task<string?> GetIncidentAssignedToAsync(int incidentId);
    Task<bool> IsIncidentCriticalAsync(int incidentId);
    Task<int> GetOpenIncidentsCountAsync(int projectId);
}