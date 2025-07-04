using System;
using System.Threading.Tasks;
using BuildTruckBack.Incidents.Domain.Aggregates;
using BuildTruckBack.Incidents.Domain.Commands;
using BuildTruckBack.Incidents.Domain.Repositories;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Incidents.Application.ACL.Services;
using BuildTruckBack.Incidents.Domain.ValueObjects;

namespace BuildTruckBack.Incidents.Application.Internal.CommandServices;

public class IncidentCommandService : IIncidentCommandHandler
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinaryService;

    public IncidentCommandService(
        IIncidentRepository incidentRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICloudinaryService cloudinaryService)
    {
        _incidentRepository = incidentRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<int> HandleAsync(CreateIncidentCommand command)
    {
        if (command.ProjectId.HasValue && !await _projectRepository.ExistsAsync(command.ProjectId.Value))
            throw new Exception("Project does not exist.");
        if (command.ReportedBy != null && !await _userRepository.ExistsAsync(command.ReportedBy))
            throw new Exception("ReportedBy user does not exist.");
        if (command.AssignedTo != null && !await _userRepository.ExistsAsync(command.AssignedTo))
            throw new Exception("AssignedTo user does not exist.");

        var incident = new Incident
        {
            ProjectId = command.ProjectId,
            Title = command.Title,
            Description = command.Description,
            IncidentType = command.IncidentType,
            Severity = Enum.Parse<IncidentSeverity>(command.Severity, true),
            Status = Enum.Parse<IncidentStatus>(command.Status, true),
            Location = command.Location,
            ReportedBy = command.ReportedBy,
            AssignedTo = command.AssignedTo,
            OccurredAt = command.OccurredAt,
            Image = command.Image != null ? await _cloudinaryService.UploadImageAsync(command.Image) : null,
            Notes = command.Notes,
            RegisterDate = command.OccurredAt,
            UpdatedAt = DateTime.UtcNow.AddHours(-5)
        };

        await _incidentRepository.AddAsync(incident);
        await _unitOfWork.CompleteAsync();
        return incident.Id;
    }

    public async Task HandleAsync(UpdateIncidentCommand command)
    {
        var incident = await _incidentRepository.FindByIdAsync(command.Id)
            ?? throw new Exception("Incident not found.");

        if (command.ProjectId.HasValue && !await _projectRepository.ExistsAsync(command.ProjectId.Value))
            throw new Exception("Project does not exist.");
        if (command.ReportedBy != null && !await _userRepository.ExistsAsync(command.ReportedBy))
            throw new Exception("ReportedBy user does not exist.");
        if (command.AssignedTo != null && !await _userRepository.ExistsAsync(command.AssignedTo))
            throw new Exception("AssignedTo user does not exist.");

        incident.ProjectId = command.ProjectId;
        incident.Title = command.Title;
        incident.Description = command.Description;
        incident.IncidentType = command.IncidentType;
        incident.Severity = Enum.Parse<IncidentSeverity>(command.Severity, true);
        incident.Status = Enum.Parse<IncidentStatus>(command.Status, true);
        incident.Location = command.Location;
        incident.ReportedBy = command.ReportedBy;
        incident.AssignedTo = command.AssignedTo;
        incident.OccurredAt = command.OccurredAt;
        incident.ResolvedAt = command.ResolvedAt;
        incident.Image = command.Image != null ? await _cloudinaryService.UploadImageAsync(command.Image) : incident.Image;
        incident.Notes = command.Notes;
        incident.UpdatedAt = DateTime.UtcNow.AddHours(-5);

        _incidentRepository.Update(incident);
        await _unitOfWork.CompleteAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var incident = await _incidentRepository.FindByIdAsync(id);
        if (incident == null)
            throw new Exception($"No se encontró el incidente con Id {id}");
    
        // Elimina la imagen de Cloudinary si existe
        if (!string.IsNullOrEmpty(incident.Image))
            await _cloudinaryService.DeleteIncidentImageAsync(incident.Image);
    
        _incidentRepository.Remove(incident);
        await _unitOfWork.CompleteAsync();
    }
}