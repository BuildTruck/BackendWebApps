using BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;

namespace BuildTruckPersonnelService.Personnel.Domain.Services;

public interface IPersonnelQueryService
{
    Task<IEnumerable<Personnel>> GetPersonnelByProjectAsync(int projectId);

    Task<IEnumerable<Personnel>> GetPersonnelWithAttendanceAsync(
        int projectId, int year, int month, bool includeAttendance = true);

    Task<Personnel?> GetPersonnelByIdAsync(int personnelId);

    Task<IEnumerable<Personnel>> GetActivePersonnelByProjectAsync(int projectId);

    Task<IEnumerable<string>> GetDepartmentsByProjectAsync(int projectId);

    Task<bool> ValidateDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null);

    Task<bool> ValidateEmailAsync(string email, int projectId, int? excludePersonnelId = null);
}
