using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckBack.Machinery.Application.Internal.CommandServices;

public class UpdateMachineryCommandHandler
{
    private readonly IMachineryRepository _machineryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryImageService _cloudinaryService;

    public UpdateMachineryCommandHandler(
        IMachineryRepository machineryRepository,
        IUnitOfWork unitOfWork,
        ICloudinaryImageService cloudinaryService)
    {
        _machineryRepository = machineryRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Domain.Model.Aggregates.Machinery> Handle(UpdateMachineryCommand command, IFormFile? imageFile = null)
    {
        var machinery = await _machineryRepository.FindByIdAsync(command.Id);
        if (machinery == null)
            throw new ArgumentException("Machinery not found");
        
        machinery.Name = command.Name;
        machinery.LicensePlate = command.LicensePlate;
        machinery.MachineryType = command.MachineryType;
        machinery.Status = command.Status.ToString();
        machinery.Provider = command.Provider;
        machinery.Description = command.Description;
        machinery.PersonnelId = command.PersonnelId;
        machinery.UpdatedAt = DateTime.UtcNow;
        
        if (imageFile != null)
        {
            if (!string.IsNullOrEmpty(machinery.ImageUrl))
            {
                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(machinery.ImageUrl);
                await _cloudinaryService.DeleteImageAsync(publicId);
            }
            
            using var stream = new MemoryStream();
            await imageFile.CopyToAsync(stream);
            machinery.ImageUrl = await _cloudinaryService.UploadImageAsync(
                stream.ToArray(), 
                imageFile.FileName, 
                "buildtruck/machinery");
        }
        
        _machineryRepository.Update(machinery);
        await _unitOfWork.CompleteAsync();
        return machinery;
    }
}