using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Materials.Domain.Repositories
{
    public interface IMaterialEntryRepository : IBaseRepository<MaterialEntry>
    {
        Task<List<MaterialEntry>> GetByProjectIdAsync(int projectId);
        Task<MaterialEntry?> GetByIdAsync(int entryId);
        Task<List<MaterialEntry>> GetByMaterialIdAsync(int materialId);
    }
}