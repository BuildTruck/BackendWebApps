using BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;
using BuildTruckShared.Domain.Repositories;

namespace BuildTruckPersonnelService.Personnel.Domain.Repositories;

public interface IPersonnelRepository : IBaseRepository<Personnel>
{
    Task<IEnumerable<Personnel>> FindByProjectIdAsync(int projectId);

    Task<IEnumerable<Personnel>> FindByProjectIdWithAttendanceAsync(int projectId, int year, int month);

    Task<Personnel?> FindByDocumentNumberAsync(string documentNumber, int projectId);

    Task<Personnel?> FindByEmailAsync(string email, int projectId);

    Task<bool> ExistsByDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null);

    Task<bool> ExistsByEmailAsync(string email, int projectId, int? excludePersonnelId = null);

    Task<IEnumerable<Personnel>> FindActiveByProjectIdAsync(int projectId);

    Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId);

    Task<bool> UpdateAttendanceBatchAsync(IEnumerable<Personnel> personnelList);
}
