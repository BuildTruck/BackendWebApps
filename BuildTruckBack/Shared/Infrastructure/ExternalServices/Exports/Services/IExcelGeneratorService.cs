namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
public interface IExcelGeneratorService
{
    Task<byte[]> GenerateExcelAsync<T>(
        IEnumerable<T> data, 
        ExportOptions options);
    
    Task<byte[]> GenerateExcelWithMultipleSheetsAsync(
        Dictionary<string, object> sheetsData, 
        ExportOptions options);
}