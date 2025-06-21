using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;

namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportsController : ControllerBase
{
    private readonly IUniversalExportService _exportService;
    private readonly ILogger<ExportsController> _logger;
    
    public ExportsController(
        IUniversalExportService exportService,
        ILogger<ExportsController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }
    
    [HttpGet("{entityType}")]
    public async Task<IActionResult> ExportEntity(
        string entityType,
        [FromQuery] int projectId,
        [FromQuery] string format = "excel",
        [FromQuery] Dictionary<string, string>? filters = null)
    {
        try
        {
            if (projectId <= 0)
            {
                return BadRequest("Valid projectId is required");
            }
            
            var request = new ExportRequest
            {
                EntityType = entityType,
                ProjectId = projectId,
                Format = format,
                Filters = filters ?? new Dictionary<string, string>(),
                RequestedBy = User.Identity?.Name ?? "Anonymous",
                RequestedAt = DateTime.UtcNow
            };
            
            var result = await _exportService.ExportEntityAsync(request);
            
            if (!result.Success)
            {
                _logger.LogWarning("Export failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            
            if (result.FileContent == null || string.IsNullOrEmpty(result.FileName))
            {
                return StatusCode(500, "Failed to generate export file");
            }
            
            return File(
                result.FileContent, 
                result.ContentType ?? "application/octet-stream", 
                result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during export: {EntityType}, Project: {ProjectId}", 
                entityType, projectId);
            return StatusCode(500, "Internal server error during export");
        }
    }
    
}