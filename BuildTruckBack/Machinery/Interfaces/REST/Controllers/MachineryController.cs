using BuildTruckBack.Machinery.Application.Internal.OutboundServices;
using BuildTruckBack.Machinery.Interfaces.REST.Resources;
using BuildTruckBack.Machinery.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BuildTruckBack.Machinery.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "manager,supervisor")]
public class MachineryController(IMachineryFacade machineryFacade) : ControllerBase
{
    [HttpGet("project/{projectId}")]
    [SwaggerOperation(Summary = "Get all machinery for a project", Description = "Returns a list of machinery registered for the specified project.")]
    public async Task<IActionResult> GetAllMachineryByProjectId(string projectId)
    {
        var machinery = await machineryFacade.GetAllMachineryAsync(projectId);
        var resources = machinery.Select(MachineryResourceAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get machinery by ID", Description = "Returns the details of a specific machinery.")]
    public async Task<IActionResult> GetMachineryById(int id)
    {
        var machinery = await machineryFacade.GetMachineryByIdAsync(id);
        if (machinery == null)
            return NotFound();
        var resource = MachineryResourceAssembler.ToResourceFromEntity(machinery);
        return Ok(resource);
    }

    [HttpPost]
    [Authorize(Roles = "supervisor")]
    [SwaggerOperation(Summary = "Create new machinery", Description = "Registers a new machinery for a project.")]
    public async Task<IActionResult> CreateMachinery([FromBody] SaveMachineryResource resource)
    {
        var command = MachineryResourceAssembler.ToCommandFromSaveResource(resource);
        var createdMachinery = await machineryFacade.CreateMachineryAsync(command);
        if (createdMachinery == null)
            return BadRequest();
        var createdResource = MachineryResourceAssembler.ToResourceFromEntity(createdMachinery);
        return CreatedAtAction(nameof(GetMachineryById), new { id = createdMachinery.Id }, createdResource);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "supervisor")]
    [SwaggerOperation(Summary = "Update machinery", Description = "Updates the details of an existing machinery.")]
    public async Task<IActionResult> UpdateMachinery(int id, [FromBody] UpdateMachineryResource resource)
    {
        var command = MachineryResourceAssembler.ToCommandFromUpdateResource(resource, id);
        var updatedMachinery = await machineryFacade.UpdateMachineryAsync(command);
        if (updatedMachinery == null)
            return NotFound();
        var updatedResource = MachineryResourceAssembler.ToResourceFromEntity(updatedMachinery);
        return Ok(updatedResource);
    }

    [HttpGet("{id}/download")]
    [SwaggerOperation(Summary = "Download machinery details", Description = "Generates a JSON file with machinery details.")]
    public async Task<IActionResult> DownloadMachineryDetails(int id)
    {
        var machinery = await machineryFacade.GetMachineryByIdAsync(id);
        if (machinery == null)
            return NotFound();

        var resource = MachineryResourceAssembler.ToResourceFromEntity(machinery);
        var json = System.Text.Json.JsonSerializer.Serialize(resource, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"machinery_{id}.json");
    }
}