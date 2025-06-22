using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Materials.Domain.Repositories
{
    public interface IMaterialUsageRepository : IBaseRepository<MaterialUsage>
    {
        Task<List<MaterialUsage>> GetByProjectIdAsync(int projectId);
        Task<MaterialUsage?> GetByIdAsync(int usageId);
        Task<List<MaterialUsage>> GetByMaterialIdAsync(int materialId);
    }
}