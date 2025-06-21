using BuildTruckBack.Machinery.Domain.Model.Aggregates;
using BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace BuildTruckBack.Machinery.Application.Internal.CommandServices;

public class DeleteMachineryCommandHandler
{
    private readonly IMachineryRepository _machineryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudinaryImageService _cloudinaryService;

    public DeleteMachineryCommandHandler(
        IMachineryRepository machineryRepository,
        IUnitOfWork unitOfWork,
        ICloudinaryImageService cloudinaryService)
    {
        _machineryRepository = machineryRepository;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task Handle(DeleteMachineryCommand command)
    {
        var machinery = await _machineryRepository.FindByIdAsync(command.Id);
        if (machinery == null) return;
        
        if (!string.IsNullOrEmpty(machinery.ImageUrl))
        {
            var publicId = _cloudinaryService.ExtractPublicIdFromUrl(machinery.ImageUrl);
            await _cloudinaryService.DeleteImageAsync(publicId);
        }
        
        _machineryRepository.Remove(machinery);
        await _unitOfWork.CompleteAsync();
    }
}