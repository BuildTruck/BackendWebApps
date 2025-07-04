using BuildTruckBack.Machinery.Domain.Model.Commands;

namespace BuildTruckBack.Machinery.Domain.Services;

public interface IMachineryCommandService
{
    Task<Model.Aggregates.Machinery> Handle(CreateMachineryCommand command, IFormFile? imageFile = null);
    Task<Model.Aggregates.Machinery> Handle(UpdateMachineryCommand command, IFormFile? imageFile = null);
    Task Handle(DeleteMachineryCommand command);
   
}