namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Configuration;

public class ExportSettings
{
    public int MaxRecordsPerExport { get; set; } = 10000;
    public int MaxFileSizeMB { get; set; } = 50;
    public string DefaultDateFormat { get; set; } = "dd/MM/yyyy";
    public string DefaultCurrencyFormat { get; set; } = "S/. #,##0.00";
    public string TempFilePath { get; set; } = "temp/exports/";
    public int FileRetentionHours { get; set; } = 24;
    public Dictionary<string, string> SupportedFormats { get; set; } = new()
    {
        { "excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { "pdf", "application/pdf" },
        { "csv", "text/csv" }
    };
}