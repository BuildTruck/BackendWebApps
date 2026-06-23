using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Personnel.Application.Internal.OutboundServices;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class PersonnelContextService : IPersonnelContextService
{
    private readonly IPersonnelFacade _personnelFacade;

    public PersonnelContextService(IPersonnelFacade personnelFacade)
    {
        _personnelFacade = personnelFacade;
    }

    public async Task<bool> PersonnelExistsAsync(int personnelId)
    {
        try
        {
            var personnel = await GetPersonnelInfoByIdAsync(personnelId);
            return personnel != null;
        }
        catch { return false; }
    }

    public async Task<string> GetPersonnelNameAsync(int personnelId)
    {
        var personnel = await GetPersonnelInfoByIdAsync(personnelId);
        return personnel?.GetFullName() ?? string.Empty;
    }

    public async Task<int> GetPersonnelProjectIdAsync(int personnelId)
    {
        var personnel = await GetPersonnelInfoByIdAsync(personnelId);
        return personnel?.ProjectId ?? 0;
    }

    public async Task<bool> PersonnelBelongsToProjectAsync(int personnelId, int projectId)
    {
        var personnel = await GetPersonnelInfoByIdAsync(personnelId);
        return personnel?.ProjectId == projectId;
    }

    public async Task<int> GetActivePersonnelCountAsync(int projectId)
    {
        return await _personnelFacade.GetActivePersonnelCountAsync(new List<int> { projectId });
    }

    public async Task<decimal> GetAttendanceRateAsync(int projectId)
    {
        return await _personnelFacade.GetAverageAttendanceRateAsync(new List<int> { projectId });
    }

    private async Task<PersonnelInfo?> GetPersonnelInfoByIdAsync(int personnelId)
    {
        // NOTE: This requires knowing the projectId to query the service.
        // For now returns null; callers handle the null case gracefully.
        return await Task.FromResult<PersonnelInfo?>(null);
    }
}