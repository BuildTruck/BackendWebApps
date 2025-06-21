namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;

public class ExportColumn
{
    public string PropertyName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = "string"; // string, number, date, currency, boolean
    public string Format { get; set; } = string.Empty;
    public int Width { get; set; } = 120;
    public bool IsVisible { get; set; } = true;
    public int Order { get; set; } = 0;
    public string Alignment { get; set; } = "left"; // left, center, right
}