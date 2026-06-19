// Materials/Application/Internal/QueryServices/MaterialQueryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckMaterialsService.Materials.Domain.Model.Aggregates;
using BuildTruckMaterialsService.Materials.Domain.Model.Queries;
using BuildTruckMaterialsService.Materials.Domain.Repositories;
using BuildTruckMaterialsService.Materials.Domain.Services;

namespace BuildTruckMaterialsService.Materials.Application.Internal.QueryServices
{
    /// <summary>
    /// Application service for querying materials
    /// </summary>
    public class MaterialQueryService : IMaterialQueryService
    {
        private readonly IMaterialRepository _materialRepository;

        public MaterialQueryService(IMaterialRepository materialRepository)
        {
            _materialRepository = materialRepository;
        }

        public async Task<List<Material>> Handle(GetMaterialsByProjectQuery query)
        {
            return await _materialRepository.GetByProjectIdAsync(query.ProjectId);
        }

        public async Task<Material?> Handle(GetMaterialByIdQuery query)
        {
            return await _materialRepository.GetByIdAsync(query.MaterialId);
        }
    }
}