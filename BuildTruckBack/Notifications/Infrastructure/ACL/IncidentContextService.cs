using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Incidents.Application.Internal;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class IncidentContextService : IIncidentContextService
{
    private readonly IIncidentFacade _incidentFacade;

    public IncidentContextService(IIncidentFacade incidentFacade)
    {
        _incidentFacade = incidentFacade;
    }

    public async Task<bool> IncidentExistsAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident != null;
    }

    public async Task<string> GetIncidentTitleAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.Title ?? string.Empty;
    }

    public async Task<int> GetIncidentProjectIdAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.ProjectId ?? 0;
    }

    public async Task<bool> IncidentBelongsToProjectAsync(int incidentId, int projectId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.ProjectId == projectId;
    }

    public async Task<string> GetIncidentSeverityAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.Severity.ToString() ?? string.Empty;
    }

    public async Task<string> GetIncidentStatusAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.Status.ToString() ?? string.Empty;
    }

    public async Task<string?> GetIncidentAssignedToAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.AssignedTo;
    }

    public async Task<bool> IsIncidentCriticalAsync(int incidentId)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(incidentId);
        return incident?.Severity.ToString() == "Alto" || incident?.Severity.ToString() == "Critico";
    }

    public async Task<int> GetOpenIncidentsCountAsync(int projectId)
    {
        var incidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
        return incidents.Count(i => i.Status.ToString() != "Resuelto");
    }
}