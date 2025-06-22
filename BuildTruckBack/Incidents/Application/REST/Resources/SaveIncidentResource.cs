using System;

namespace BuildTruckBack.Incidents.Application.REST.Resources;

public record SaveIncidentResource(
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
    string? Image,
    string Notes);