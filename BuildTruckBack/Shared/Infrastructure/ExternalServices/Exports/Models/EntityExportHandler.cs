namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;

public abstract class EntityExportHandler
{
    public abstract string EntityType { get; }
    
    public abstract Task<IEnumerable<object>> GetDataAsync(
        int projectId, 
        Dictionary<string, string> filters);
    
    public abstract List<ExportColumn> GetColumns();
    
    public virtual ExportOptions GetDefaultOptions()
    {
        return new ExportOptions
        {
            Title = $"{EntityType} Report",
            IncludeHeader = true,
            IncludeFooter = true,
            AutoFitColumns = true,
            Columns = GetColumns()
        };
    }
    
    public virtual string GetFileName(string format, Dictionary<string, string> filters)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filterSuffix = filters.Any() ? $"_{string.Join("_", filters.Values.Take(2))}" : "";
        return $"{EntityType}_Report_{timestamp}{filterSuffix}.{format}";
    }
}