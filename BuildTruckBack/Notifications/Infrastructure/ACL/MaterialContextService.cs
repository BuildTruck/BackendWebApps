using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Materials.Application.Internal.OutboundServices;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class MaterialContextService : IMaterialContextService
{
    private readonly IMaterialFacade _materialFacade;

    public MaterialContextService(IMaterialFacade materialFacade)
    {
        _materialFacade = materialFacade;
    }

    public async Task<bool> MaterialExistsAsync(int materialId)
    {
        try
        {
            var material = await GetMaterialByIdAsync(materialId);
            return material != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetMaterialNameAsync(int materialId)
    {
        var material = await GetMaterialByIdAsync(materialId);
        return material?.Name.Value ?? string.Empty;
    }

    public async Task<int> GetMaterialProjectIdAsync(int materialId)
    {
        var material = await GetMaterialByIdAsync(materialId);
        return material?.ProjectId ?? 0;
    }

    public async Task<bool> MaterialBelongsToProjectAsync(int materialId, int projectId)
    {
        return await _materialFacade.ValidateMaterialExistsInProjectAsync(materialId, projectId);
    }

    public async Task<decimal> GetMaterialStockAsync(int materialId)
    {
        return await _materialFacade.GetMaterialStockAsync(materialId);
    }

    public async Task<decimal> GetMaterialMinimumStockAsync(int materialId)
    {
        var material = await GetMaterialByIdAsync(materialId);
        return material?.MinimumStock.Value ?? 0;
    }

    public async Task<bool> IsMaterialLowStockAsync(int materialId)
    {
        var currentStock = await GetMaterialStockAsync(materialId);
        var minimumStock = await GetMaterialMinimumStockAsync(materialId);
        return currentStock <= minimumStock;
    }

    public async Task<IEnumerable<int>> GetLowStockMaterialsAsync(int projectId)
    {
        var materials = await _materialFacade.GetMaterialsByProjectAsync(projectId);
        var lowStockMaterials = new List<int>();

        foreach (var material in materials)
        {
            if (await IsMaterialLowStockAsync(material.Id))
            {
                lowStockMaterials.Add(material.Id);
            }
        }

        return lowStockMaterials;
    }

    private async Task<BuildTruckBack.Materials.Domain.Model.Aggregates.Material?> GetMaterialByIdAsync(int materialId)
    {
        try
        {
            var allMaterials = await _materialFacade.GetMaterialsByProjectAsync(0);
            return allMaterials.FirstOrDefault(m => m.Id == materialId);
        }
        catch
        {
            return null;
        }
    }
}