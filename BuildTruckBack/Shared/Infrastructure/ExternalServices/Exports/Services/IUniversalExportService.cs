namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
public interface IUniversalExportService
{
    Task<ExportResult> ExportEntityAsync(ExportRequest request);
    
    void RegisterHandler(EntityExportHandler handler);
    
    Task<List<string>> GetSupportedEntityTypesAsync();
    
    Task<List<ExportColumn>> GetEntityColumnsAsync(string entityType);
}