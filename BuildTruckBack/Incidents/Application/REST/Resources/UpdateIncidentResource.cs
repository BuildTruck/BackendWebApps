using System;

namespace BuildTruckBack.Incidents.Application.REST.Resources;

public record UpdateIncidentResource(
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