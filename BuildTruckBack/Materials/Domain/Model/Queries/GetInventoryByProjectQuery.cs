using System;

namespace BuildTruckBack.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving inventory summary by project
    /// </summary>
    /// <param name="ProjectId">Project ID</param>
    public record GetInventoryByProjectQuery(int ProjectId);
}