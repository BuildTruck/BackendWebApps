using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Configuration;
public class UniversalExportService : IUniversalExportService
{
    private readonly Dictionary<string, EntityExportHandler> _handlers = new();
    private readonly IExcelGeneratorService _excelGenerator;
    private readonly IPdfGeneratorService _pdfGenerator;
    private readonly ExportSettings _settings;
    private readonly ILogger<UniversalExportService> _logger;
    
    public UniversalExportService(
        IExcelGeneratorService excelGenerator,
        IPdfGeneratorService pdfGenerator,
        IOptions<ExportSettings> settings,
        ILogger<UniversalExportService> logger)
    {
        _excelGenerator = excelGenerator;
        _pdfGenerator = pdfGenerator;
        _settings = settings.Value;
        _logger = logger;
    }
    
    public void RegisterHandler(EntityExportHandler handler)
    {
        _handlers[handler.EntityType.ToLower()] = handler;
        _logger.LogInformation("Registered export handler for entity type: {EntityType}", handler.EntityType);
    }
    
    public async Task<ExportResult> ExportEntityAsync(ExportRequest request)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting export for entity: {EntityType}, project: {ProjectId}, format: {Format}", 
                request.EntityType, request.ProjectId, request.Format);
            
            // Validate request
            var validationResult = ValidateRequest(request);
            if (!validationResult.Success)
            {
                return validationResult;
            }
            
            // Get handler
            var handler = GetHandler(request.EntityType);
            if (handler == null)
            {
                return new ExportResult
                {
                    Success = false,
                    ErrorMessage = $"No handler registered for entity type: {request.EntityType}"
                };
            }
            
            // Get data
            var data = await handler.GetDataAsync(request.ProjectId, request.Filters);
            var dataList = data.ToList();
            
            if (dataList.Count > _settings.MaxRecordsPerExport)
            {
                return new ExportResult
                {
                    Success = false,
                    ErrorMessage = $"Too many records ({dataList.Count}). Maximum allowed: {_settings.MaxRecordsPerExport}"
                };
            }
            
            // Prepare options
            var options = PrepareExportOptions(request, handler);
            
            // Generate file
            byte[] fileContent;
            string contentType;
            
            switch (request.Format.ToLower())
            {
                case "excel":
                    fileContent = await _excelGenerator.GenerateExcelAsync(dataList, options);
                    contentType = _settings.SupportedFormats["excel"];
                    break;
                    
                case "pdf":
                    fileContent = await _pdfGenerator.GeneratePdfAsync(dataList, options);
                    contentType = _settings.SupportedFormats["pdf"];
                    break;
                    
                default:
                    return new ExportResult
                    {
                        Success = false,
                        ErrorMessage = $"Unsupported format: {request.Format}"
                    };
            }
            
            // Validate file size
            if (fileContent.Length > _settings.MaxFileSizeMB * 1024 * 1024)
            {
                return new ExportResult
                {
                    Success = false,
                    ErrorMessage = $"Generated file too large ({fileContent.Length / (1024 * 1024)}MB). Maximum: {_settings.MaxFileSizeMB}MB"
                };
            }
            
            var fileName = handler.GetFileName(request.Format, request.Filters);
            
            stopwatch.Stop();
            
            _logger.LogInformation("Export completed successfully. Entity: {EntityType}, Records: {RecordCount}, Size: {FileSize}KB, Time: {ProcessingTime}ms",
                request.EntityType, dataList.Count, fileContent.Length / 1024, stopwatch.ElapsedMilliseconds);
            
            return new ExportResult
            {
                Success = true,
                FileName = fileName,
                FileContent = fileContent,
                ContentType = contentType,
                FileSizeBytes = fileContent.Length,
                RecordCount = dataList.Count,
                ProcessingTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error during export. Entity: {EntityType}, Project: {ProjectId}", 
                request.EntityType, request.ProjectId);
            
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"Internal error during export: {ex.Message}",
                ProcessingTime = stopwatch.Elapsed
            };
        }
    }
    
    public async Task<List<string>> GetSupportedEntityTypesAsync()
    {
        return await Task.FromResult(_handlers.Keys.ToList());
    }
    
    public async Task<List<ExportColumn>> GetEntityColumnsAsync(string entityType)
    {
        var handler = GetHandler(entityType);
        return await Task.FromResult(handler?.GetColumns() ?? new List<ExportColumn>());
    }
    
    private EntityExportHandler? GetHandler(string entityType)
    {
        _handlers.TryGetValue(entityType.ToLower(), out var handler);
        return handler;
    }
    
    private ExportResult ValidateRequest(ExportRequest request)
    {
        if (string.IsNullOrEmpty(request.EntityType))
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = "EntityType is required"
            };
        }
        
        if (request.ProjectId <= 0)
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = "Valid ProjectId is required"
            };
        }
        
        if (!_settings.SupportedFormats.ContainsKey(request.Format.ToLower()))
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = $"Unsupported format: {request.Format}. Supported: {string.Join(", ", _settings.SupportedFormats.Keys)}"
            };
        }
        
        return new ExportResult { Success = true };
    }
    
    private ExportOptions PrepareExportOptions(ExportRequest request, EntityExportHandler handler)
    {
        var options = request.Options ?? handler.GetDefaultOptions();
        
        // Apply default settings if not specified
        if (string.IsNullOrEmpty(options.Title))
        {
            options.Title = $"{request.EntityType} Export Report";
        }
        
        if (string.IsNullOrEmpty(options.Subtitle))
        {
            options.Subtitle = $"Project: {request.ProjectId} | Generated: {DateTime.Now:dd/MM/yyyy HH:mm}";
        }
        
        // Apply filters to subtitle
        if (request.Filters.Any())
        {
            var filterText = string.Join(", ", request.Filters.Select(f => $"{f.Key}: {f.Value}"));
            options.Subtitle += $" | Filters: {filterText}";
        }
        
        return options;
    }
}
