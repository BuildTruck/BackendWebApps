namespace BuildTruckBack.Personnel.Application.Internal.OutboundServices;

public interface IPersonnelFacade
{
    Task<IEnumerable<PersonnelInfo>> GetPersonnelByProjectAsync(int projectId);
    Task<IEnumerable<PersonnelInfo>> GetPersonnelByProjectsAsync(List<int> projectIds);
    Task<int> GetActivePersonnelCountAsync(List<int> projectIds);
    Task<int> GetTotalPersonnelCountAsync(List<int> projectIds);
    Task<Dictionary<string, int>> GetPersonnelByTypeAsync(List<int> projectIds);
    Task<decimal> GetTotalSalaryAmountAsync(List<int> projectIds);
    Task<decimal> GetAverageAttendanceRateAsync(List<int> projectIds);
    Task<Dictionary<string, object>> GetPersonnelStatisticsAsync(List<int> projectIds, int year, int month);
}