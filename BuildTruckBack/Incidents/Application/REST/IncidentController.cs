using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BuildTruckBack.Incidents.Application.Internal;
using BuildTruckBack.Incidents.Application.REST.Resources;
using BuildTruckBack.Incidents.Domain.Commands;
using BuildTruckBack.Incidents.Application.REST.Transform;

namespace BuildTruckBack.Incidents.Application.REST;

[ApiController]
[Route("api/incidents")]
[Authorize(Roles = "Manager,Supervisor")]
public class IncidentController : ControllerBase
{
    private readonly IIncidentFacade _incidentFacade;

    public IncidentController(IIncidentFacade incidentFacade)
    {
        _incidentFacade = incidentFacade;
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetIncidentsByProjectId(int projectId)
    {
        var incidents = await _incidentFacade.GetIncidentsByProjectIdAsync(projectId);
        var resources = incidents.Select(IncidentResourceAssembler.ToResource);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIncidentById(int id)
    {
        var incident = await _incidentFacade.GetIncidentByIdAsync(id);
        if (incident == null) return NotFound();
        return Ok(IncidentResourceAssembler.ToResource(incident));
    }

    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    public async Task<IActionResult> CreateIncident([FromBody] SaveIncidentResource resource)
    {
        var command = new CreateIncidentCommand(
            resource.ProjectId,
            resource.Title,
            resource.Description,
            resource.IncidentType,
            resource.Severity,
            resource.Status,
            resource.Location,
            resource.ReportedBy,
            resource.AssignedTo,
            resource.OccurredAt,
            resource.Image,
            resource.Notes);
        var incidentId = await _incidentFacade.CreateIncidentAsync(command);
        return CreatedAtAction(nameof(GetIncidentById), new { id = incidentId }, new { id = incidentId });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Supervisor")]
    public async Task<IActionResult> UpdateIncident(int id, [FromBody] UpdateIncidentResource resource)
    {
        if (id != resource.Id) return BadRequest("ID mismatch.");
        var command = new UpdateIncidentCommand(
            resource.Id,
            resource.ProjectId,
            resource.Title,
            resource.Description,
            resource.IncidentType,
            resource.Severity,
            resource.Status,
            resource.Location,
            resource.ReportedBy,
            resource.AssignedTo,
            resource.OccurredAt,
            resource.ResolvedAt,
            resource.Image,
            resource.Notes);
        await _incidentFacade.UpdateIncidentAsync(command);
        return Ok();
    }
}