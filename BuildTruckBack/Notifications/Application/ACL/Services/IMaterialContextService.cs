namespace BuildTruckBack.Notifications.Application.ACL.Services;

public interface IMaterialContextService
{
    Task<bool> MaterialExistsAsync(int materialId);
    Task<string> GetMaterialNameAsync(int materialId);
    Task<int> GetMaterialProjectIdAsync(int materialId);
    Task<bool> MaterialBelongsToProjectAsync(int materialId, int projectId);
    Task<decimal> GetMaterialStockAsync(int materialId);
    Task<decimal> GetMaterialMinimumStockAsync(int materialId);
    Task<bool> IsMaterialLowStockAsync(int materialId);
    Task<IEnumerable<int>> GetLowStockMaterialsAsync(int projectId);
}