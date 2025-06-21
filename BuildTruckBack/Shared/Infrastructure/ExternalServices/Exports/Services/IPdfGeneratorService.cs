namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePdfAsync<T>(
        IEnumerable<T> data, 
        ExportOptions options);
    
    Task<byte[]> GenerateReportPdfAsync<T>(
        IEnumerable<T> data, 
        string title, 
        ExportOptions options);
}