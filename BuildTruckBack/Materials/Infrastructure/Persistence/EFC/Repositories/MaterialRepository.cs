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
    /// EF Core implementation of Material repository
    /// </summary>
    public class MaterialRepository : BaseRepository<Material>, IMaterialRepository
    {
        public MaterialRepository(AppDbContext context) : base(context) { }

        public async Task<List<Material>> GetByProjectIdAsync(int projectId)
        {
            return await Context.Set<Material>()
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.Name.Value)
                .ToListAsync();
        }

        public new async Task<Material?> GetByIdAsync(int materialId)
        {
            return await Context.Set<Material>()
                .FirstOrDefaultAsync(m => m.Id == materialId);
        }
    }
}