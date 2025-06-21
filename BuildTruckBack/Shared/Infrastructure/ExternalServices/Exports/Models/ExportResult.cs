namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;

public class ExportResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public byte[]? FileContent { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public int RecordCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
}