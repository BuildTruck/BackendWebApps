using BuildTruckBack.Incidents.Application.REST.Resources;
using BuildTruckBack.Incidents.Domain.Aggregates;

namespace BuildTruckBack.Incidents.Application.REST.Transform;

public class IncidentResourceAssembler
{
    public static IncidentResource ToResource(Incident incident)
    {
        return new IncidentResource(
            incident.Id,
            incident.ProjectId,
            incident.Title,
            incident.Description,
            incident.IncidentType,
            incident.Severity,
            incident.Status,
            incident.Location,
            incident.ReportedBy,
            incident.AssignedTo,
            incident.OccurredAt,
            incident.ResolvedAt,
            incident.Image,
            incident.Notes,
            incident.UpdatedAt,
            incident.RegisterDate);
    }
}