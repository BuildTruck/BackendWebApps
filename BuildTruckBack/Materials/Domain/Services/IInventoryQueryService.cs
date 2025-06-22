// Materials/Domain/Services/IInventoryQueryService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildTruckBack.Materials.Domain.Model.Aggregates;
using BuildTruckBack.Materials.Domain.Model.Queries;

namespace BuildTruckBack.Materials.Domain.Services
{
    /// <summary>
    /// Service interface for querying consolidated inventory by project
    /// </summary>
    public interface IInventoryQueryService
    {
        /// <summary>
        /// Gets the current inventory summary for a given project
        /// </summary>
        /// <param name="query">Query with project ID</param>
        /// <returns>List of materials with updated stock and prices</returns>
        Task<List<Material>> Handle(GetInventoryByProjectQuery query);
    }
}