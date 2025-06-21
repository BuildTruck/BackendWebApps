using BuildTruckBack.Configurations.Domain.Model.Aggregates;

namespace BuildTruckBack.Configurations.Domain.Model.Commands;

public interface IConfigurationCommandHandler
{
    Task<Configuration?> Handle(CreateConfigurationCommand command);
    Task<Configuration?> Handle(UpdateConfigurationCommand command);
}