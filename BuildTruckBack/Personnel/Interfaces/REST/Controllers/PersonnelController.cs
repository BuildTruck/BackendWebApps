using BuildTruckBack.Personnel.Domain.Model.Commands;
using BuildTruckBack.Personnel.Domain.Model.ValueObjects;
using BuildTruckBack.Personnel.Domain.Services;
using BuildTruckBack.Personnel.Interfaces.REST.Resources;
using BuildTruckBack.Personnel.Interfaces.REST.Transform;
using BuildTruckBack.Personnel.Application.ACL.Services;
using BuildTruckBack.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace BuildTruckBack.Personnel.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize] // Require authentication for all endpoints
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelCommandService _personnelCommandService;
    private readonly IPersonnelQueryService _personnelQueryService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PersonnelController> _logger;

    public PersonnelController(
        IPersonnelCommandService personnelCommandService,
        IPersonnelQueryService personnelQueryService,
        ICloudinaryService cloudinaryService,
        IUnitOfWork unitOfWork,
        ILogger<PersonnelController> logger)
    {
        _personnelCommandService = personnelCommandService;
        _personnelQueryService = personnelQueryService;
        _cloudinaryService = cloudinaryService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Create a new personnel with optional image upload
    /// POST /api/v1/personnel
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreatePersonnel([FromForm] CreatePersonnelResource resource)
    {
        try
        {
            _logger.LogInformation("👤 Creating new personnel: {PersonnelName}", $"{resource.Name} {resource.Lastname}");

            // 1. Validate the resource first
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                                      .Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // 2. Create command from resource
            var command = CreatePersonnelCommandFromResourceAssembler.ToCommandFromResource(resource);
            var personnel = await _personnelCommandService.Handle(command);

            if (personnel == null)
                return BadRequest("Failed to create personnel");

            // 3. Handle image upload after creation
            if (resource.ImageFile != null)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await resource.ImageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    
                    var imageUrl = await _cloudinaryService.UploadPersonnelPhotoAsync(
                        imageBytes, 
                        resource.ImageFile.FileName, 
                        personnel.Id);
                    
                    personnel.UpdateAvatar(imageUrl);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation("📸 Avatar uploaded successfully for personnel: {PersonnelId}", personnel.Id);
                }
                catch (Exception imageEx)
                {
                    _logger.LogWarning(imageEx, "⚠️ Failed to upload avatar for personnel: {PersonnelId}", personnel.Id);
                    // Continue without failing the entire operation
                }
            }

            var personnelResource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            
            _logger.LogInformation("✅ Personnel created successfully: {PersonnelId} - {PersonnelName}", 
                personnel.Id, personnel.GetFullName());

            return CreatedAtAction(nameof(GetPersonnelById), new { id = personnel.Id }, personnelResource);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for personnel creation: {PersonnelName}", $"{resource.Name} {resource.Lastname}");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "❌ Duplicate data for personnel creation: {PersonnelName}", $"{resource.Name} {resource.Lastname}");
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for personnel creation: {PersonnelName}", $"{resource.Name} {resource.Lastname}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error creating personnel: {PersonnelName}", $"{resource.Name} {resource.Lastname}");
            return StatusCode(500, "An internal error occurred while creating the personnel");
        }
    }

    /// <summary>
    /// Get personnel by project with optional attendance data
    /// GET /api/v1/personnel?projectId=X&year=Y&month=Z&includeAttendance=true
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(IEnumerable<PersonnelResource>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetPersonnelByProject(
        [FromQuery] int projectId,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] bool includeAttendance = false)
    {
        try
        {
            _logger.LogInformation("🔍 Getting personnel for project: {ProjectId} (includeAttendance: {IncludeAttendance})", 
                projectId, includeAttendance);

            if (projectId <= 0)
                return BadRequest("Invalid project ID");

            IEnumerable<Domain.Model.Aggregates.Personnel> personnel;

            // If attendance is requested, validate year and month
            if (includeAttendance)
            {
                var currentDate = DateTime.Now;
                var targetYear = year ?? currentDate.Year;
                var targetMonth = month ?? currentDate.Month;

                if (targetYear < 2020 || targetYear > 2100)
                    return BadRequest("Invalid year. Must be between 2020 and 2100");

                if (targetMonth < 1 || targetMonth > 12)
                    return BadRequest("Invalid month. Must be between 1 and 12");

                _logger.LogInformation("📊 Loading personnel with attendance for {Year}/{Month}", targetYear, targetMonth);

                personnel = await _personnelQueryService.GetPersonnelWithAttendanceAsync(
                    projectId, targetYear, targetMonth, true);

                _logger.LogInformation("📊 Retrieved personnel with attendance calculations completed");
            }
            else
            {
                personnel = await _personnelQueryService.GetPersonnelByProjectAsync(projectId);
            }

            var personnelList = personnel.ToList();
            
            // 🆕 MODIFICADO: Pass the includeAttendance flag to the assembler
            var resources = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(
                personnelList, 
                includeAttendance);
            
            _logger.LogInformation("✅ Found {PersonnelCount} personnel for project: {ProjectId} (with attendance data: {WithAttendance})", 
                personnelList.Count, projectId, includeAttendance);

            // 🔍 DEBUG: Log sample attendance data if included
            if (includeAttendance && personnelList.Any())
            {
                var firstPersonnel = personnelList.First();
                _logger.LogInformation("🔍 Sample attendance data for {PersonnelName}: {AttendanceKeys}", 
                    firstPersonnel.GetFullName(), 
                    string.Join(", ", firstPersonnel.MonthlyAttendance.Keys));
            }

            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting personnel for project: {ProjectId}", projectId);
            return StatusCode(500, "An internal error occurred while retrieving personnel");
        }
    }

    /// <summary>
    /// Get personnel by ID
    /// GET /api/v1/personnel/{id}
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetPersonnelById(int id)
    {
        try
        {
            _logger.LogInformation("🔍 Getting personnel details: {PersonnelId}", id);

            if (id <= 0)
                return BadRequest("Personnel ID must be greater than 0");

            var personnel = await _personnelQueryService.GetPersonnelByIdAsync(id);
            if (personnel == null)
                return NotFound($"Personnel with ID {id} not found");

            var resource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            
            _logger.LogInformation("✅ Personnel details retrieved: {PersonnelId} - {PersonnelName}", 
                personnel.Id, personnel.GetFullName());

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting personnel details: {PersonnelId}", id);
            return StatusCode(500, "An internal error occurred while retrieving personnel");
        }
    }

    /// <summary>
    /// Update existing personnel with optional image operations
    /// PUT /api/v1/personnel/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> UpdatePersonnel(int id, [FromForm] UpdatePersonnelResource resource)
    {
        try
        {
            _logger.LogInformation("🔧 Updating personnel: {PersonnelId}", id);

            // 1. Validate personnel ID
            if (id <= 0)
                return BadRequest("Personnel ID must be greater than 0");

            // 2. Validate the resource
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                                      .Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // 3. Check if there are changes to apply
            if (!resource.HasChanges())
            {
                return BadRequest("No changes specified in update request");
            }

            // 4. Create command from resource
            var command = UpdatePersonnelCommandFromResourceAssembler.ToCommandFromResource(id, resource);
            var personnel = await _personnelCommandService.Handle(command);
            
            if (personnel == null)
                return NotFound($"Personnel with ID {id} not found");

            // 5. Handle image operations after update
            bool imageUpdated = false;
            
            if (resource.ImageFile != null)
            {
                try
                {
                    // Nueva imagen - reemplazar la anterior
                    using var memoryStream = new MemoryStream();
                    await resource.ImageFile.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();
                    
                    var newImageUrl = await _cloudinaryService.UploadPersonnelPhotoAsync(
                        imageBytes, 
                        resource.ImageFile.FileName, 
                        personnel.Id);
                    
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(personnel.AvatarUrl))
                    {
                        try
                        {
                            await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl);
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogWarning(deleteEx, "⚠️ Failed to delete old avatar for personnel: {PersonnelId}", personnel.Id);
                        }
                    }
                    
                    personnel.UpdateAvatar(newImageUrl);
                    imageUpdated = true;

                    _logger.LogInformation("📸 Avatar updated successfully for personnel: {PersonnelId}", personnel.Id);
                }
                catch (Exception imageEx)
                {
                    _logger.LogWarning(imageEx, "⚠️ Failed to update avatar for personnel: {PersonnelId}", personnel.Id);
                    // Continue without failing the entire operation
                }
            }
            else if (resource.RemoveImage)
            {
                try
                {
                    // Solo eliminar imagen actual
                    if (!string.IsNullOrEmpty(personnel.AvatarUrl))
                    {
                        await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl);
                        personnel.UpdateAvatar(null);
                        imageUpdated = true;

                        _logger.LogInformation("🗑️ Avatar removed successfully for personnel: {PersonnelId}", personnel.Id);
                    }
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "⚠️ Failed to remove avatar for personnel: {PersonnelId}", personnel.Id);
                }
            }

            // 6. Save changes if image was updated
            if (imageUpdated)
            {
                await _unitOfWork.CompleteAsync();
            }

            var personnelResource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            
            _logger.LogInformation("✅ Personnel updated successfully: {PersonnelId} - {PersonnelName}", 
                personnel.Id, personnel.GetFullName());

            return Ok(personnelResource);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for personnel update: {PersonnelId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "❌ Duplicate data for personnel update: {PersonnelId}", id);
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for personnel update: {PersonnelId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error updating personnel: {PersonnelId}", id);
            return StatusCode(500, "An internal error occurred while updating the personnel");
        }
    }

    /// <summary>
    /// Update attendance for multiple personnel
    /// PUT /api/v1/personnel/attendance
    /// </summary>
    [HttpPut("attendance")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> UpdateAttendance([FromBody] UpdateAttendanceResource resource)
    {
        try
        {
            _logger.LogInformation("📊 Updating attendance for {PersonnelCount} personnel", 
                resource.PersonnelAttendances?.Count ?? 0);

            // 1. Validate the resource first
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                                      .Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // 2. Create command from resource
            var command = AttendanceResourceFromEntityAssembler.ToCommandFromResource(resource);
            var success = await _personnelCommandService.Handle(command);

            if (success)
            {
                _logger.LogInformation("✅ Attendance updated successfully for {PersonnelCount} personnel", 
                    resource.PersonnelAttendances.Count);

                return Ok(new { 
                    success = true, 
                    message = "Attendance updated successfully",
                    updatedPersonnel = resource.PersonnelAttendances.Count,
                    updatedAt = DateTimeOffset.UtcNow
                });
            }

            return BadRequest("Failed to update attendance");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for attendance update");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for attendance update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error updating attendance");
            return StatusCode(500, "An internal error occurred while updating attendance");
        }
    }

    /// <summary>
    /// Delete personnel (soft delete) with image cleanup
    /// DELETE /api/v1/personnel/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(DeletePersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DeletePersonnel(int id, [FromQuery] string? reason = null)
    {
        try
        {
            _logger.LogInformation("🗑️ Deleting personnel: {PersonnelId}", id);

            // 1. Validate personnel ID
            if (id <= 0)
                return BadRequest("Personnel ID must be greater than 0");

            // 2. Get personnel first to check if has avatar
            var personnel = await _personnelQueryService.GetPersonnelByIdAsync(id);
            if (personnel == null)
                return NotFound(new DeletePersonnelResource(false, $"Personnel with ID {id} not found"));

            // 3. Execute soft delete
            var command = new DeletePersonnelCommand(id);
            var success = await _personnelCommandService.Handle(command);

            if (success)
            {
                // 4. Clean up avatar from Cloudinary after successful delete
                if (!string.IsNullOrEmpty(personnel.AvatarUrl))
                {
                    try
                    {
                        await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl);
                        _logger.LogInformation("🗑️ Avatar deleted from Cloudinary for personnel: {PersonnelId}", id);
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogWarning(imageEx, "⚠️ Failed to delete avatar from Cloudinary for personnel: {PersonnelId}", id);
                        // Don't fail the operation if image deletion fails
                    }
                }

                _logger.LogInformation("✅ Personnel deleted successfully: {PersonnelId}", id);
                return Ok(new DeletePersonnelResource(true, "Personnel deleted successfully"));
            }

            return BadRequest(new DeletePersonnelResource(false, "Failed to delete personnel"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for personnel deletion: {PersonnelId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for personnel deletion: {PersonnelId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error deleting personnel: {PersonnelId}", id);
            return StatusCode(500, new DeletePersonnelResource(false, $"Internal server error: {ex.Message}"));
        }
    }
}