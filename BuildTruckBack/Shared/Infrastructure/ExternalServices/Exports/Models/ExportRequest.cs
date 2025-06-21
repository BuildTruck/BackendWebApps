namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;

public class ExportRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string Format { get; set; } = "excel";
    public Dictionary<string, string> Filters { get; set; } = new();
    public ExportOptions Options { get; set; } = new();
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}