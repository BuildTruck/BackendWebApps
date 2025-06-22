using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Materials.Domain.Repositories
{
    public interface IMaterialRepository : IBaseRepository<Material>
    {
        Task<List<Material>> GetByProjectIdAsync(int projectId);
        Task<Material?> GetByIdAsync(int materialId);
    }
}