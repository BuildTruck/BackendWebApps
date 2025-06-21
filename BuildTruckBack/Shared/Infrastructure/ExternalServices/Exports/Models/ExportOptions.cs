namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;

public class ExportOptions
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public bool IncludeHeader { get; set; } = true;
    public bool IncludeFooter { get; set; } = true;
    public bool AutoFitColumns { get; set; } = true;
    public string Orientation { get; set; } = "portrait"; // portrait, landscape
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    public List<ExportColumn> Columns { get; set; } = new();
}