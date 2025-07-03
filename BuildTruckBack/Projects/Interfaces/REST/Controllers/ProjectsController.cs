using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BuildTruckBack.Projects.Domain.Services;
using BuildTruckBack.Projects.Domain.Model.Commands;
using BuildTruckBack.Projects.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Projects.Interfaces.REST.Resources;
using BuildTruckBack.Projects.Interfaces.REST.Transform;
using System.Net;

namespace BuildTruckBack.Projects.Interfaces.REST.Controllers;

/// <summary>
/// REST Controller for Project operations
/// </summary>
/// <remarks>
/// Handles the 4 main project endpoints with proper authorization and validation
/// </remarks>
[ApiController]
[Route("api/v1/projects")]
[Authorize] // Require authentication for all endpoints
public class ProjectsController : ControllerBase
{
    private readonly IProjectCommandService _projectCommandService;
    private readonly ProjectRepository _projectRepository;
    private readonly ProjectResourceAssembler _resourceAssembler;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IProjectCommandService projectCommandService,
        ProjectRepository projectRepository,
        ProjectResourceAssembler resourceAssembler,
        ILogger<ProjectsController> logger)
    {
        _projectCommandService = projectCommandService;
        _projectRepository = projectRepository;
        _resourceAssembler = resourceAssembler;
        _logger = logger;
    }

    /// <summary>
    /// Create a new project
    /// POST /api/v1/projects/create
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ProjectResource), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<ProjectResource>> CreateProject([FromForm] CreateProjectResource resource)
    {
        try
        {
            _logger.LogInformation("🏗️ Creating new project: {ProjectName}", resource.Name);

            // 1. Validate resource
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                                      .Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            // 2. Additional business validation
            var businessErrors = resource.GetValidationErrors();
            if (businessErrors.Any())
            {
                return BadRequest($"Business validation failed: {string.Join(", ", businessErrors)}");
            }

            // 3. Transform to command
            var command = ProjectResourceAssembler.ToCommandFromResource(resource);

            // 4. Execute command
            var project = await _projectCommandService.Handle(command);
            if (project == null)
            {
                return BadRequest("Failed to create project");
            }

            // 5. Transform to resource
            var projectResource = await _resourceAssembler.ToResourceFromEntityAsync(project);

            _logger.LogInformation("✅ Project created successfully: {ProjectId} - {ProjectName}", 
                project.Id, project.ProjectName);

            return CreatedAtAction(nameof(GetProjectsByManager), 
                new { id = project.ManagerId }, projectResource);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for project creation: {ProjectName}", resource.Name);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for project creation: {ProjectName}", resource.Name);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "❌ Unauthorized project creation attempt: {ProjectName}", resource.Name);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error creating project: {ProjectName}", resource.Name);
            return StatusCode(500, "An internal error occurred while creating the project");
        }
    }

    /// <summary>
    /// Get projects by manager ID
    /// GET /api/v1/projects/by-manager/{id}
    /// </summary>
    [HttpGet("by-manager/{id}")]
    [ProducesResponseType(typeof(List<ProjectSummaryResource>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<List<ProjectSummaryResource>>> GetProjectsByManager(int id)
    {
        try
        {
            _logger.LogInformation("🔍 Getting projects for manager: {ManagerId}", id);

            // 1. Validate manager ID
            if (id <= 0)
            {
                return BadRequest("Manager ID must be greater than 0");
            }

            // 2. Get projects from repository
            var projects = await _projectRepository.FindByManagerIdAsync(id);

            // 3. Transform to summary resources
            var summaryTasks = projects.Select(_resourceAssembler.ToSummaryResourceFromEntityAsync);
            var projectSummaries = await Task.WhenAll(summaryTasks);

            _logger.LogInformation("✅ Found {ProjectCount} projects for manager: {ManagerId}", 
                projects.Count, id);

            return Ok(projectSummaries.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting projects for manager: {ManagerId}", id);
            return StatusCode(500, "An internal error occurred while retrieving projects");
        }
    }

    /// <summary>
    /// Update an existing project
    /// PUT /api/v1/projects/{id}/update
    /// </summary>
    [HttpPut("{id}/update")]
    [ProducesResponseType(typeof(ProjectResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<ProjectResource>> UpdateProject(int id, [FromForm] UpdateProjectResource resource)
    {
        try
        {
            _logger.LogInformation("🔧 Updating project: {ProjectId}", id);

            // 1. Validate project ID
            if (id <= 0)
            {
                return BadRequest("Project ID must be greater than 0");
            }

            // 2. Validate resource
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value?.Errors ?? [])
                                      .Select(x => x.ErrorMessage);
                return BadRequest($"Validation failed: {string.Join(", ", errors)}");
            }

            // 3. Check if project exists
            var existingProject = await _projectRepository.FindByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            // 4. Additional business validation
            var businessErrors = resource.GetValidationErrors();
            if (businessErrors.Any())
            {
                return BadRequest($"Business validation failed: {string.Join(", ", businessErrors)}");
            }

            // 5. Check if there are changes to apply
            if (!resource.HasChanges())
            {
                return BadRequest("No changes specified in update request");
            }

            // 6. Transform to command
            var command = ProjectResourceAssembler.ToCommandFromResource(id, resource);

            // 7. Execute command
            var updatedProject = await _projectCommandService.Handle(command);
            if (updatedProject == null)
            {
                return BadRequest("Failed to update project");
            }

            // 8. Transform to resource
            var projectResource = await _resourceAssembler.ToResourceFromEntityAsync(updatedProject);

            _logger.LogInformation("✅ Project updated successfully: {ProjectId} - {ProjectName}", 
                updatedProject.Id, updatedProject.ProjectName);

            return Ok(projectResource);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for project update: {ProjectId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for project update: {ProjectId}", id);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "❌ Unauthorized project update attempt: {ProjectId}", id);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error updating project: {ProjectId}", id);
            return StatusCode(500, "An internal error occurred while updating the project");
        }
    }

    /// <summary>
    /// Delete a project
    /// DELETE /api/v1/projects/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult> DeleteProject(int id, [FromQuery] bool force = false, [FromQuery] string? reason = null)
    {
        try
        {
            _logger.LogInformation("🗑️ Deleting project: {ProjectId} (Force: {Force})", id, force);

            // 1. Validate project ID
            if (id <= 0)
            {
                return BadRequest("Project ID must be greater than 0");
            }

            // 2. Check if project exists
            var existingProject = await _projectRepository.FindByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            // 3. Get current user ID (from JWT token)
            var currentUserIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("user_id")?.Value;
            if (!int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                return BadRequest("Invalid user ID in token");
            }

            // 4. Create delete command
            var command = new DeleteProjectCommand(
                projectId: id,
                requestedByUserId: currentUserId,
                forceDelete: force,
                reason: reason
            );

            // 5. Validate command
            var commandErrors = command.GetValidationErrors();
            if (commandErrors.Any())
            {
                return BadRequest($"Command validation failed: {string.Join(", ", commandErrors)}");
            }

            // 6. Execute delete command
            var success = await _projectCommandService.Handle(command);
            if (!success)
            {
                return BadRequest("Failed to delete project");
            }

            _logger.LogInformation("✅ Project deleted successfully: {ProjectId}", id);

            return Ok(new { 
                message = "Project deleted successfully", 
                projectId = id,
                deletedAt = DateTimeOffset.UtcNow,
                reason = command.GetNormalizedReason()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "❌ Invalid request for project deletion: {ProjectId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "❌ Business rule violation for project deletion: {ProjectId}", id);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "❌ Unauthorized project deletion attempt: {ProjectId}", id);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Internal error deleting project: {ProjectId}", id);
            return StatusCode(500, "An internal error occurred while deleting the project");
        }
    }
    /// <summary>
    /// Get project by supervisor ID
    /// GET /api/v1/projects/by-supervisor/{supervisorId}
    /// </summary>
    [HttpGet("by-supervisor/{supervisorId}")]
    [ProducesResponseType(typeof(ProjectResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
    public async Task<ActionResult<ProjectResource>> GetProjectBySupervisor(int supervisorId)
    {
        try
        {
            _logger.LogInformation("🔍 Getting project for supervisor: {SupervisorId}", supervisorId);

            // 1. Validate supervisor ID
            if (supervisorId <= 0)
            {
                return BadRequest("Supervisor ID must be greater than 0");
            }

            // 2. Security: Verify the requesting user
            var currentUserIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("user_id")?.Value;
            if (int.TryParse(currentUserIdClaim, out var currentUserId))
            {
                // Allow if it's the same supervisor or if it's an admin/manager
                var currentUserRole = User.FindFirst("role")?.Value;
                if (currentUserId != supervisorId && 
                    currentUserRole != "Admin" && 
                    currentUserRole != "Manager")
                {
                    return Forbid("You can only access your own project information");
                }
            }

            // 3. Find projects by supervisor ID
            var projects = await _projectRepository.FindBySupervisorIdAsync(supervisorId);
            
            if (!projects.Any())
            {
                return NotFound($"No project found for supervisor {supervisorId}");
            }

            // 4. Return the most recent project (supervisors usually have one active project)
            var currentProject = projects.First();

            // 5. Transform to resource
            var projectResource = await _resourceAssembler.ToResourceFromEntityAsync(currentProject);

            _logger.LogInformation("✅ Project found for supervisor {SupervisorId}: {ProjectId} - {ProjectName}", 
                supervisorId, currentProject.Id, currentProject.ProjectName);

            return Ok(projectResource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting project for supervisor: {SupervisorId}", supervisorId);
            return StatusCode(500, "An internal error occurred while retrieving project");
        }
    }
    
    /*
    /// <summary>
    /// Get project by ID (additional endpoint for single project details)
    /// GET /api/v1/projects/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ProjectResource>> GetProjectById(int id)
    {
        try
        {
            _logger.LogInformation("🔍 Getting project details: {ProjectId}", id);

            // 1. Validate project ID
            if (id <= 0)
            {
                return BadRequest("Project ID must be greater than 0");
            }

            // 2. Get project from repository
            var project = await _projectRepository.FindByIdAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found");
            }

            // 3. Transform to resource
            var projectResource = await _resourceAssembler.ToResourceFromEntityAsync(project);

            _logger.LogInformation("✅ Project details retrieved: {ProjectId} - {ProjectName}", 
                project.Id, project.ProjectName);

            return Ok(projectResource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting project details: {ProjectId}", id);
            return StatusCode(500, "An internal error occurred while retrieving project details");
        }
    }

    /// <summary>
    /// Search projects with pagination (additional endpoint for advanced queries)
    /// GET /api/v1/projects/search
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ProjectPagedResource), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ProjectPagedResource>> SearchProjects(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? managerId = null,
        [FromQuery] int? supervisorId = null,
        [FromQuery] string? state = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("🔍 Searching projects with term: {SearchTerm}", searchTerm);

            // 1. Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            // 2. Search projects
            var projects = await _projectRepository.SearchAsync(searchTerm, managerId, supervisorId, state);
            var totalCount = projects.Count;

            // 3. Apply pagination
            var paginatedProjects = projects
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. Transform to paged resource
            var pagedResource = await _resourceAssembler.ToPagedResourceFromEntitiesAsync(
                paginatedProjects, totalCount, pageNumber, pageSize);

            _logger.LogInformation("✅ Found {TotalCount} projects, returning page {PageNumber}", 
                totalCount, pageNumber);

            return Ok(pagedResource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error searching projects");
            return StatusCode(500, "An internal error occurred while searching projects");
        }
    }
    */
}