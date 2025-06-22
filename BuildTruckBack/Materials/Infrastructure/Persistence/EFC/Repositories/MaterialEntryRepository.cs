using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Materials.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildTruckBack.Materials.Infrastructure.Persistence.EFC.Repositories
{
    /// <summary>
    /// EF Core implementation of MaterialEntry repository
    /// </summary>
    public class MaterialEntryRepository : BaseRepository<MaterialEntry>, IMaterialEntryRepository
    {
        public MaterialEntryRepository(AppDbContext context) : base(context) { }

        public async Task<List<MaterialEntry>> GetByProjectIdAsync(int projectId)
        {
            return await Context.Set<MaterialEntry>()
                .Where(e => e.ProjectId == projectId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();
        }

        public new async Task<MaterialEntry?> GetByIdAsync(int entryId)
        {
            return await Context.Set<MaterialEntry>()
                .FirstOrDefaultAsync(e => e.Id == entryId);
        }

        public async Task<List<MaterialEntry>> GetByMaterialIdAsync(int materialId)
        {
            return await Context.Set<MaterialEntry>()
                .Where(e => e.MaterialId == materialId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedDate)
                .ToListAsync();
        }
    }
}