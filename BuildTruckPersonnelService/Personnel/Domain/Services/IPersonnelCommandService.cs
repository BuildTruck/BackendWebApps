using BuildTruckPersonnelService.Personnel.Domain.Model.Aggregates;
using BuildTruckPersonnelService.Personnel.Domain.Model.Commands;

namespace BuildTruckPersonnelService.Personnel.Domain.Services;

public interface IPersonnelCommandService
{
    Task<Personnel?> Handle(CreatePersonnelCommand command);
    Task<Personnel?> Handle(UpdatePersonnelCommand command);
    Task<bool> Handle(UpdateAttendanceCommand command);
    Task<bool> Handle(DeletePersonnelCommand command);
}
