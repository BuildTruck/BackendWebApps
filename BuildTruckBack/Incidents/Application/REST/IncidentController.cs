using System.Collections.Generic;
                                using System.Threading.Tasks;
                                using Microsoft.AspNetCore.Mvc;
                                using Microsoft.AspNetCore.Authorization;
                                using BuildTruckBack.Incidents.Application.Internal;
                                using BuildTruckBack.Incidents.Application.REST.Resources;
                                using BuildTruckBack.Incidents.Domain.Commands;
                                using BuildTruckBack.Incidents.Application.REST.Transform;
                                using System.IO;
                                using System.Linq;
                                
                                namespace BuildTruckBack.Incidents.Application.REST
                                {
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
                                        [Consumes("multipart/form-data")]
                                        public async Task<IActionResult> CreateIncident([FromForm] CreateIncidentDto dto)
                                        {
                                            string? tempFilePath = null;
                                            string? imageName = null;
                                            if (dto.Image != null)
                                            {
                                                var ext = Path.GetExtension(dto.Image.FileName);
                                                tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);
                                                using (var stream = System.IO.File.Create(tempFilePath))
                                                {
                                                    await dto.Image.CopyToAsync(stream);
                                                }
                                                imageName = dto.Image.FileName;
                                            }
                                
                                            var command = new CreateIncidentCommand(
                                                dto.ProjectId,
                                                dto.Title,
                                                dto.Description,
                                                dto.IncidentType,
                                                dto.Severity,
                                                dto.Status,
                                                dto.Location,
                                                dto.ReportedBy,
                                                dto.AssignedTo,
                                                dto.OccurredAt,
                                                imageName,      // Nombre del archivo
                                                dto.Notes,
                                                tempFilePath    // Ruta temporal
                                            );
                                            var incidentId = await _incidentFacade.CreateIncidentAsync(command);
                                
                                            if (tempFilePath != null)
                                                System.IO.File.Delete(tempFilePath);
                                
                                            return CreatedAtAction(nameof(GetIncidentById), new { id = incidentId }, new { id = incidentId });
                                        }
                                
                                        [HttpPut("{id}")]
                                        [Authorize(Roles = "Supervisor")]
                                        [Consumes("multipart/form-data")]
                                        public async Task<IActionResult> UpdateIncident(int id, [FromForm] UpdateIncidentDto dto)
                                        {
                                            if (id != dto.Id) return BadRequest("ID mismatch.");
                                        
                                            string? tempFilePath = null;
                                            string? imageName = null;
                                            if (dto.Image != null)
                                            {
                                                var ext = Path.GetExtension(dto.Image.FileName);
                                                tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);
                                                using (var stream = System.IO.File.Create(tempFilePath))
                                                {
                                                    await dto.Image.CopyToAsync(stream);
                                                }
                                                imageName = dto.Image.FileName;
                                            }
                                        
                                            var command = new UpdateIncidentCommand(
                                                dto.Id,
                                                dto.ProjectId,
                                                dto.Title,
                                                dto.Description,
                                                dto.IncidentType,
                                                dto.Severity,
                                                dto.Status,
                                                dto.Location,
                                                dto.ReportedBy,
                                                dto.AssignedTo,
                                                dto.OccurredAt,
                                                dto.ResolvedAt,
                                                imageName,
                                                dto.Notes,
                                                tempFilePath
                                            );
                                            await _incidentFacade.UpdateIncidentAsync(command);
                                        
                                            if (tempFilePath != null)
                                                System.IO.File.Delete(tempFilePath);
                                        
                                            return Ok();
                                        }
                                        
                                        [HttpDelete("{id}")]
                                        [Authorize(Roles = "Supervisor")]
                                        public async Task<IActionResult> DeleteIncident(int id)
                                        {
                                            await _incidentFacade.DeleteIncidentAsync(id);
                                            return NoContent();
                                        }
                                    }
                                }