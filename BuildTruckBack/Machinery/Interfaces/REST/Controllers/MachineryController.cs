using BuildTruckBack.Machinery.Application.Internal.CommandServices;
using BuildTruckBack.Machinery.Application.Internal.QueryServices;
using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Model.Queries;
using BuildTruckBack.Machinery.Interfaces.REST.Resources;
using BuildTruckBack.Machinery.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace BuildTruckBack.Machinery.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/projects/{projectId}/machinery")]
public class MachineryController : ControllerBase
{
    private readonly CreateMachineryCommandHandler _createCommandHandler;
    private readonly UpdateMachineryCommandHandler _updateCommandHandler;
    private readonly DeleteMachineryCommandHandler _deleteCommandHandler;
    private readonly GetMachineryByIdQueryHandler _getByIdQueryHandler;
    private readonly GetMachineryByProjectQueryHandler _getByProjectQueryHandler;
    private readonly GetActiveMachineryQueryHandler _getActiveQueryHandler;

    public MachineryController(
        CreateMachineryCommandHandler createCommandHandler,
        UpdateMachineryCommandHandler updateCommandHandler,
        DeleteMachineryCommandHandler deleteCommandHandler,
        GetMachineryByIdQueryHandler getByIdQueryHandler,
        GetMachineryByProjectQueryHandler getByProjectQueryHandler,
        GetActiveMachineryQueryHandler getActiveQueryHandler)
    {
        _createCommandHandler = createCommandHandler;
        _updateCommandHandler = updateCommandHandler;
        _deleteCommandHandler = deleteCommandHandler;
        _getByIdQueryHandler = getByIdQueryHandler;
        _getByProjectQueryHandler = getByProjectQueryHandler;
        _getActiveQueryHandler = getActiveQueryHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMachinery(int projectId)
    {
        var query = new GetMachineryByProjectQuery(projectId);
        var machinery = await _getByProjectQueryHandler.Handle(query);
        var resources = machinery.Select(m => m.ToResource());
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMachineryById(int projectId, int id)
    {
        var query = new GetMachineryByIdQuery(id);
        var machinery = await _getByIdQueryHandler.Handle(query);
        if (machinery == null || machinery.ProjectId != projectId)
            return NotFound();
        
        return Ok(machinery.ToResource());
    }

    [HttpPost]
    public async Task<IActionResult> CreateMachinery(
        int projectId, 
        [FromForm] CreateMachineryResource resource,
        IFormFile? imageFile)
    {
        var command = new CreateMachineryCommand(
            projectId,
            resource.Name,
            resource.LicensePlate,
            resource.MachineryType,
            resource.Status,
            resource.Provider,
            resource.Description,
            resource.PersonnelId,
            resource.RegisterDate);
        
        var machinery = await _createCommandHandler.Handle(command, imageFile);
        return CreatedAtAction(
            nameof(GetMachineryById), 
            new { projectId, id = machinery.Id }, 
            machinery.ToResource());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMachinery(
        int projectId, 
        int id, 
        [FromForm] MachineryResource resource,
        IFormFile? imageFile)
    {
        if (id != resource.Id || projectId != resource.ProjectId)
            return BadRequest("ID mismatch");
        
        var command = new UpdateMachineryCommand(
            id,
            projectId,
            resource.Name,
            resource.LicensePlate,
            resource.MachineryType,
            resource.Status,
            resource.Provider,
            resource.Description,
            resource.PersonnelId);
        
        var machinery = await _updateCommandHandler.Handle(command, imageFile);
        return Ok(machinery.ToResource());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMachinery(int projectId, int id)
    {
        var command = new DeleteMachineryCommand(id);
        await _deleteCommandHandler.Handle(command);
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveMachinery(int projectId)
    {
        var query = new GetActiveMachineryQuery(projectId);
        var machinery = await _getActiveQueryHandler.Handle(query);
        return Ok(machinery.Select(m => m.ToResource()));
    }
}