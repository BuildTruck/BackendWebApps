namespace BuildTruckBack.Incidents.Domain.Commands;

public record UpdateIncidentCommand(
    int Id,
    int? ProjectId,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    string Location,
    string? ReportedBy,
    string? AssignedTo,
    DateTime OccurredAt,
    DateTime? ResolvedAt,
    string? Image,
    string Notes);