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
    /// EF Core implementation of MaterialUsage repository
    /// </summary>
    public class MaterialUsageRepository : BaseRepository<MaterialUsage>, IMaterialUsageRepository
    {
        public MaterialUsageRepository(AppDbContext context) : base(context) { }

        public async Task<List<MaterialUsage>> GetByProjectIdAsync(int projectId)
        {
            return await Context.Set<MaterialUsage>()
                .Where(u => u.ProjectId == projectId)
                .OrderByDescending(u => u.Date)
                .ThenByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public new async Task<MaterialUsage?> GetByIdAsync(int usageId)
        {
            return await Context.Set<MaterialUsage>()
                .FirstOrDefaultAsync(u => u.Id == usageId);
        }

        public async Task<List<MaterialUsage>> GetByMaterialIdAsync(int materialId)
        {
            return await Context.Set<MaterialUsage>()
                .Where(u => u.MaterialId == materialId)
                .OrderByDescending(u => u.Date)
                .ThenByDescending(u => u.CreatedDate)
                .ToListAsync();
        }
    }
}