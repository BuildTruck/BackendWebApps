using System;

namespace BuildTruckBack.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving materials by project
    /// </summary>
    /// <param name="ProjectId">Project ID</param>
    public record GetMaterialsByProjectQuery(int ProjectId);
}