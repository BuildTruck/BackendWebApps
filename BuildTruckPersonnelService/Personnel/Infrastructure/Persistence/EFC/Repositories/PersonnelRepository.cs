using BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;
using BuildTruckPersonnelService.Personnel.Domain.Model.ValueObjects;
using BuildTruckPersonnelService.Personnel.Domain.Repositories;
using BuildTruckPersonnelService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckPersonnelService.Personnel.Infrastructure.Persistence.EFC.Repositories;

public class PersonnelRepository : BaseRepository<Personnel>, IPersonnelRepository
{
    public PersonnelRepository(PersonnelServiceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Personnel>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<Personnel>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<IEnumerable<Personnel>> FindByProjectIdWithAttendanceAsync(int projectId, int year, int month)
    {
        return await Context.Set<Personnel>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<Personnel?> FindByDocumentNumberAsync(string documentNumber, int projectId)
    {
        return await Context.Set<Personnel>()
            .FirstOrDefaultAsync(p =>
                p.DocumentNumber == documentNumber &&
                p.ProjectId == projectId &&
                !p.IsDeleted);
    }

    public async Task<Personnel?> FindByEmailAsync(string email, int projectId)
    {
        return await Context.Set<Personnel>()
            .FirstOrDefaultAsync(p =>
                p.Email == email &&
                p.ProjectId == projectId &&
                !p.IsDeleted);
    }

    public async Task<bool> ExistsByDocumentNumberAsync(string documentNumber, int projectId, int? excludePersonnelId = null)
    {
        var query = Context.Set<Personnel>()
            .Where(p =>
                p.DocumentNumber == documentNumber &&
                p.ProjectId == projectId &&
                !p.IsDeleted);

        if (excludePersonnelId.HasValue)
            query = query.Where(p => p.Id != excludePersonnelId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email, int projectId, int? excludePersonnelId = null)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var query = Context.Set<Personnel>()
            .Where(p =>
                p.Email == email &&
                p.ProjectId == projectId &&
                !p.IsDeleted);

        if (excludePersonnelId.HasValue)
            query = query.Where(p => p.Id != excludePersonnelId.Value);

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Personnel>> FindActiveByProjectIdAsync(int projectId)
    {
        return await Context.Set<Personnel>()
            .Where(p =>
                p.ProjectId == projectId &&
                !p.IsDeleted &&
                p.Status == PersonnelStatus.ACTIVE)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId)
    {
        return await Context.Set<Personnel>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .Select(p => p.Department)
            .Distinct()
            .Where(d => !string.IsNullOrEmpty(d))
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<bool> UpdateAttendanceBatchAsync(IEnumerable<Personnel> personnelList)
    {
        try
        {
            foreach (var personnel in personnelList)
                Context.Set<Personnel>().Update(personnel);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public new async Task<Personnel?> FindByIdAsync(int id)
    {
        return await Context.Set<Personnel>()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public new async Task<IEnumerable<Personnel>> ListAsync()
    {
        return await Context.Set<Personnel>()
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }
}
