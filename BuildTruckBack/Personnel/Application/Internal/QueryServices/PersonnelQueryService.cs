using BuildTruckBack.Personnel.Domain.Model.Aggregates;
using BuildTruckBack.Personnel.Domain.Repositories;
using BuildTruckBack.Personnel.Domain.Services;

namespace BuildTruckBack.Personnel.Application.Internal.QueryServices;

public class PersonnelQueryService : IPersonnelQueryService
{
    private readonly IPersonnelRepository _personnelRepository;

    public PersonnelQueryService(IPersonnelRepository personnelRepository)
    {
        _personnelRepository = personnelRepository;
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetPersonnelByProjectAsync(int projectId)
    {
        return await _personnelRepository.FindByProjectIdAsync(projectId);
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetPersonnelWithAttendanceAsync(
        int projectId, 
        int year, 
        int month, 
        bool includeAttendance = true)
    {
        if (!includeAttendance)
        {
            return await _personnelRepository.FindByProjectIdAsync(projectId);
        }

        var personnel = await _personnelRepository.FindByProjectIdWithAttendanceAsync(projectId, year, month);
        
        // Calculate attendance totals for each personnel
        var personnelList = personnel.ToList();
        foreach (var person in personnelList)
        {
            // Initialize month attendance if not exists
            person.InitializeMonthAttendance(year, month);
            
            // Auto-mark Sundays
            person.AutoMarkSundays(year, month);
            
            // Calculate monthly totals
            person.CalculateMonthlyTotals(year, month);
        }

        return personnelList;
    }

    public async Task<Domain.Model.Aggregates.Personnel?> GetPersonnelByIdAsync(int personnelId)
    {
        return await _personnelRepository.FindByIdAsync(personnelId);
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> GetActivePersonnelByProjectAsync(int projectId)
    {
        return await _personnelRepository.FindActiveByProjectIdAsync(projectId);
    }

    public async Task<IEnumerable<string>> GetDepartmentsByProjectAsync(int projectId)
    {
        return await _personnelRepository.GetDepartmentsByProjectIdAsync(projectId);
    }

    public async Task<bool> ValidateDocumentNumberAsync(
        string documentNumber, 
        int projectId, 
        int? excludePersonnelId = null)
    {
        return await _personnelRepository.ExistsByDocumentNumberAsync(
            documentNumber, 
            projectId, 
            excludePersonnelId);
    }

    public async Task<bool> ValidateEmailAsync(
        string email, 
        int projectId, 
        int? excludePersonnelId = null)
    {
        if (string.IsNullOrEmpty(email))
            return false;
            
        return await _personnelRepository.ExistsByEmailAsync(
            email, 
            projectId, 
            excludePersonnelId);
    }
}