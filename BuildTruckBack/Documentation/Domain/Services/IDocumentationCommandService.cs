using BuildTruckBack.Documentation.Domain.Model.Aggregates;
using BuildTruckBack.Documentation.Domain.Model.Commands;

namespace BuildTruckBack.Documentation.Domain.Services;

public interface IDocumentationCommandService
{
    Task<Documentation.Domain.Model.Aggregates.Documentation?> Handle(CreateOrUpdateDocumentationCommand command);
    
    Task<bool> Handle(DeleteDocumentationCommand command);
}