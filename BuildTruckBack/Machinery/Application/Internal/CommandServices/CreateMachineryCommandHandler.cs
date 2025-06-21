using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Projects.Application.Internal.OutboundServices;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckBack.Machinery.Application.Internal.CommandServices;

public class CreateMachineryCommandHandler
{
    private readonly IMachineryRepository _machineryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectFacade _projectFacade;
    private readonly ICloudinaryImageService _cloudinaryService;

    public CreateMachineryCommandHandler(
        IMachineryRepository machineryRepository,
        IUnitOfWork unitOfWork,
        IProjectFacade projectFacade,
        ICloudinaryImageService cloudinaryService)
    {
        _machineryRepository = machineryRepository;
        _unitOfWork = unitOfWork;
        _projectFacade = projectFacade;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(CreateMachineryCommand command, IFormFile? imageFile = null)
    {
        if (!await _projectFacade.ExistsByIdAsync(command.ProjectId))
            throw new ArgumentException("Invalid project ID");
        
        var existing = await _machineryRepository.FindByLicensePlateAsync(
            command.LicensePlate, command.ProjectId);
        
        if (existing != null)
            throw new ArgumentException("License plate must be unique per project");
        
        var machinery = new Domain.Model.Aggregates.Machinery
        {
            ProjectId = command.ProjectId,
            Name = command.Name,
            LicensePlate = command.LicensePlate,
            MachineryType = command.MachineryType,
            Status = command.Status.ToString(),
            Provider = command.Provider,
            Description = command.Description,
            PersonnelId = command.PersonnelId,
            RegisterDate = command.RegisterDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        if (imageFile != null)
        {
            using var stream = new MemoryStream();
            await imageFile.CopyToAsync(stream);
            machinery.ImageUrl = await _cloudinaryService.UploadImageAsync(
                stream.ToArray(), 
                imageFile.FileName, 
                "buildtruck/machinery");
        }
        
        await _machineryRepository.AddAsync(machinery);
        await _unitOfWork.CompleteAsync();
        return machinery;
    }
}