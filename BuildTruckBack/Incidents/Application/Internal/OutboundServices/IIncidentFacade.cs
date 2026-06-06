namespace BuildTruckBack.Incidents.Application.Internal.OutboundServices;

public interface IIncidentFacade
{
    Task<IEnumerable<IncidentDto>> GetByProjectIdAsync(int projectId);
    Task<IncidentDto?> GetByIdAsync(int incidentId);
    Task<IncidentDto> CreateAsync(CreateIncidentDto dto);
    Task<IncidentDto> UpdateAsync(int id, UpdateIncidentDto dto);
    Task<bool> DeleteAsync(int incidentId);
}

public record IncidentDto(
    int Id,
    int ProjectId,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    string Location,
    string? Notes,
    int ReportedBy,
    DateTime ReportedDate,
    DateTime CreatedAt
);

public record CreateIncidentDto(
    int ProjectId,
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    string Location,
    string? Notes
);

public record UpdateIncidentDto(
    string Title,
    string Description,
    string IncidentType,
    string Severity,
    string Status,
    string Location,
    string? Notes
);
