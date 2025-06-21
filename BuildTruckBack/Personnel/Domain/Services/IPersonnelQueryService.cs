using BuildTruckBack.Personnel.Domain.Model.Aggregates;

namespace BuildTruckBack.Personnel.Domain.Services;

public interface IPersonnelQueryService
{
    Task<IEnumerable<Personnel.Domain.Model.Aggregates.Personnel>> GetPersonnelByProjectAsync(int projectId);
    
    Task<IEnumerable<Personnel.Domain.Model.Aggregates.Personnel>> GetPersonnelWithAttendanceAsync(
        int projectId, 
        int year, 
        int month, 
        bool includeAttendance = true);
    
    Task<Personnel.Domain.Model.Aggregates.Personnel?> GetPersonnelByIdAsync(int personnelId);
    
    Task<IEnumerable<Personnel.Domain.Model.Aggregates.Personnel>> GetActivePersonnelByProjectAsync(int projectId);
    
    Task<IEnumerable<string>> GetDepartmentsByProjectAsync(int projectId);
    
    Task<bool> ValidateDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null);
    
    Task<bool> ValidateEmailAsync(string email, int projectId, int? excludePersonnelId = null);
}