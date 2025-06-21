using BuildTruckBack.Personnel.Domain.Model.Commands;
using BuildTruckBack.Personnel.Domain.Model.ValueObjects;
using BuildTruckBack.Personnel.Domain.Services;
using BuildTruckBack.Personnel.Interfaces.REST.Resources;
using BuildTruckBack.Personnel.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Authorization;
namespace BuildTruckBack.Personnel.Interfaces.REST.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelCommandService _personnelCommandService;
    private readonly IPersonnelQueryService _personnelQueryService;

    public PersonnelController(
        IPersonnelCommandService personnelCommandService,
        IPersonnelQueryService personnelQueryService)
    {
        _personnelCommandService = personnelCommandService;
        _personnelQueryService = personnelQueryService;
    }

    /// <summary>
    /// 1. POST /api/personnel - Crear empleado con foto FormData
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
            // Validate the resource first
            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Create command from resource
            var command = CreatePersonnelCommandFromResourceAssembler.ToCommandFromResource(resource);
            var personnel = await _personnelCommandService.Handle(command);

            if (personnel == null)
                return BadRequest("Failed to create personnel");

            // Handle image upload after creation
            if (resource.ImageFile != null)
            {
                // TODO: Implement Cloudinary upload
                // var imageUrl = await _cloudinaryService.UploadAsync(resource.ImageFile);
                // personnel.UpdateAvatar(imageUrl);
                // await _personnelCommandService.SaveChangesAsync();
            }

            var personnelResource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            return CreatedAtAction(nameof(GetPersonnelById), new { id = personnel.Id }, personnelResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 2. GET /api/personnel?projectId=X&year=Y&month=Z&includeAttendance=true - Obtener con cálculos
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

                personnel = await _personnelQueryService.GetPersonnelWithAttendanceAsync(
                    projectId, targetYear, targetMonth, true);
            }
            else
            {
                personnel = await _personnelQueryService.GetPersonnelByProjectAsync(projectId);
            }

            var personnelList = personnel.ToList();
            var resources = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnelList);
            return Ok(resources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper para obtener por ID (usado por CreatedAtAction)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(PersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetPersonnelById(int id)
    {
        try
        {
            var personnel = await _personnelQueryService.GetPersonnelByIdAsync(id);
            if (personnel == null)
                return NotFound($"Personnel with ID {id} not found");

            var resource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 3. PUT /api/personnel/{id} - Actualizar empleado con foto FormData opcional
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
            // Validate the resource first
            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            // Create command from resource
            var command = UpdatePersonnelCommandFromResourceAssembler.ToCommandFromResource(id, resource);
            var personnel = await _personnelCommandService.Handle(command);
            
            if (personnel == null)
                return NotFound($"Personnel with ID {id} not found");

            // Handle image operations after update
            if (resource.RemoveImage)
            {
                personnel.UpdateAvatar(null);
                // TODO: Also delete from Cloudinary
                // await _cloudinaryService.DeleteAsync(oldImageUrl);
            }
            else if (resource.ImageFile != null)
            {
                // TODO: Implement Cloudinary upload
                // var imageUrl = await _cloudinaryService.UploadAsync(resource.ImageFile);
                // personnel.UpdateAvatar(imageUrl);
            }

            // TODO: Save changes if image was updated
            // await _personnelCommandService.SaveChangesAsync();

            var personnelResource = PersonnelResourceFromEntityAssembler.ToResourceFromEntity(personnel);
            return Ok(personnelResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 4. PUT /api/personnel/attendance - Guardar asistencia masiva de proyecto
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
            // Validate the resource first
            if (!resource.IsValid())
            {
                var errors = resource.GetValidationErrors();
                return BadRequest(new { message = "Validation failed", errors });
            }

            var command = AttendanceResourceFromEntityAssembler.ToCommandFromResource(resource);
            var success = await _personnelCommandService.Handle(command);

            if (success)
            {
                return Ok(new { 
                    success = true, 
                    message = "Attendance updated successfully",
                    updatedPersonnel = resource.PersonnelAttendances.Count
                });
            }

            return BadRequest("Failed to update attendance");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// 5. DELETE /api/personnel/{id} - Soft delete
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Supervisor,Manager,Admin")]
    [ProducesResponseType(typeof(DeletePersonnelResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        try
        {
            var command = new DeletePersonnelCommand(id);
            var success = await _personnelCommandService.Handle(command);

            if (success)
            {
                return Ok(new DeletePersonnelResource(true, "Personnel deleted successfully"));
            }

            return NotFound(new DeletePersonnelResource(false, $"Personnel with ID {id} not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new DeletePersonnelResource(false, $"Internal server error: {ex.Message}"));
        }
    }
}