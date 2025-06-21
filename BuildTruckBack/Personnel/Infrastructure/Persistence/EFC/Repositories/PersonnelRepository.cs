using BuildTruckBack.Personnel.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Personnel.Infrastructure.Persistence.EFC.Repositories;

public class PersonnelRepository : BaseRepository<Domain.Model.Aggregates.Personnel>, IPersonnelRepository
{
    public PersonnelRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> FindByProjectIdAsync(int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> FindByProjectIdWithAttendanceAsync(
        int projectId, 
        int year, 
        int month)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<Domain.Model.Aggregates.Personnel?> FindByDocumentNumberAsync(
        string documentNumber, 
        int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .FirstOrDefaultAsync(p => 
                p.DocumentNumber == documentNumber && 
                p.ProjectId == projectId && 
                !p.IsDeleted);
    }

    public async Task<Domain.Model.Aggregates.Personnel?> FindByEmailAsync(
        string email, 
        int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .FirstOrDefaultAsync(p => 
                p.Email == email && 
                p.ProjectId == projectId && 
                !p.IsDeleted);
    }

    public async Task<bool> ExistsByDocumentNumberAsync(
        string documentNumber, 
        int projectId, 
        int? excludePersonnelId = null)
    {
        var query = Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => 
                p.DocumentNumber == documentNumber && 
                p.ProjectId == projectId && 
                !p.IsDeleted);

        if (excludePersonnelId.HasValue)
        {
            query = query.Where(p => p.Id != excludePersonnelId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmailAsync(
        string email, 
        int projectId, 
        int? excludePersonnelId = null)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        var query = Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => 
                p.Email == email && 
                p.ProjectId == projectId && 
                !p.IsDeleted);

        if (excludePersonnelId.HasValue)
        {
            query = query.Where(p => p.Id != excludePersonnelId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> FindActiveByProjectIdAsync(int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => 
                p.ProjectId == projectId && 
                !p.IsDeleted && 
                p.Status == Domain.Model.ValueObjects.PersonnelStatus.ACTIVE)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Lastname)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetDepartmentsByProjectIdAsync(int projectId)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted)
            .Select(p => p.Department)
            .Distinct()
            .Where(d => !string.IsNullOrEmpty(d))
            .OrderBy(d => d)
            .ToListAsync();
    }

    public async Task<bool> UpdateAttendanceBatchAsync(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnelList)
    {
        try
        {
            var personnelArray = personnelList.ToArray();
            
            foreach (var personnel in personnelArray)
            {
                Context.Set<Domain.Model.Aggregates.Personnel>().Update(personnel);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Removí los override - implementa estos métodos si los necesitas específicamente
    // o usa los métodos heredados de la clase base
    public new async Task<Domain.Model.Aggregates.Personnel?> FindByIdAsync(int id)
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public new async Task<IEnumerable<Domain.Model.Aggregates.Personnel>> ListAsync()
    {
        return await Context.Set<Domain.Model.Aggregates.Personnel>()
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }
}