using System;

namespace BuildTruckBack.Materials.Domain.Model.Queries
{
    /// <summary>
    /// Query for retrieving a material entry by ID
    /// </summary>
    /// <param name="EntryId">Material entry ID</param>
    public record GetMaterialEntryByIdQuery(int EntryId);
}