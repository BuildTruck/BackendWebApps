using BuildTruckBack.Personnel.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Personnel.Domain.Repositories;

public interface IPersonnelRepository : IBaseRepository<Personnel.Domain.Model.Aggregates.Personnel>
{
    // Queries específicas del dominio Personnel
    Task<IEnumerable<Personnel.Domain.Model.Aggregates.Personnel>> FindByProjectIdAsync(int projectId);
    
    Task<IEnumerable<Personnel.Domain.Model.Aggregates.Personnel>> FindByProjectIdWithAttendanceAsync(
        int projectId, 
        int year, 
        int month);
    
    Task<Personnel.Domain.Model.Aggregates.Personnel?> FindByDocumentNumberAsync(
        string documentNumber, 
        int projectId);
    
    Task<Personnel.Domain.Model.Aggregates.Personnel?> FindByEmailAsync(
        string email, 
        int projectId);
    
    Task<bool> ExistsByDocumentNumberAsync(
        string documentNumber, 
        int projectId, 
        int? excludePersonnelId = null);
    
    Task<bool> ExistsByEmailAsync(
        string email, 
        int projectId, 
        int? excludePersonnelId = null);
    
    Task<IEnumerable<Personnel.Domain.Model.Aggregates.Personnel>> FindActiveByProjectIdAsync(int projectId);
    
    Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId);
    
    // Métodos para asistencia masiva
    Task<bool> UpdateAttendanceBatchAsync(
        IEnumerable<Personnel.Domain.Model.Aggregates.Personnel> personnelList);
}