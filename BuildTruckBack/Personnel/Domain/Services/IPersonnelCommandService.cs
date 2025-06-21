using BuildTruckBack.Personnel.Domain.Model.Aggregates;
using BuildTruckBack.Personnel.Domain.Model.Commands;

namespace BuildTruckBack.Personnel.Domain.Services;

public interface IPersonnelCommandService
{
    Task<Personnel.Domain.Model.Aggregates.Personnel?> Handle(CreatePersonnelCommand command);
    
    Task<Personnel.Domain.Model.Aggregates.Personnel?> Handle(UpdatePersonnelCommand command);
    
    Task<bool> Handle(UpdateAttendanceCommand command);
    
    Task<bool> Handle(DeletePersonnelCommand command);
}