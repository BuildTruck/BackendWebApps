namespace BuildTruckBack.Machinery.Domain.Model.Commands;
using BuildTruckBack.Machinery.Domain.Model.Aggregates;

public interface IMachineryCommandHandler
{
    Task<Machinery?> Handle(CreateMachineryCommand command);
    Task<Machinery?> Handle(UpdateMachineryCommand command);
}