using BuildTruckBack.Documentation.Domain.Model.Commands;
using BuildTruckBack.Documentation.Interfaces.REST.Resources;

namespace BuildTruckBack.Documentation.Interfaces.REST.Transform;

public static class CreateOrUpdateDocumentationCommandFromResourceAssembler
{
    public static CreateOrUpdateDocumentationCommand ToCommandFromResource(
        CreateOrUpdateDocumentationResource resource, 
        string imagePath, 
        int createdBy)
    {
        return new CreateOrUpdateDocumentationCommand(
            resource.Id,
            resource.ProjectId,
            resource.Title,
            resource.Description,
            imagePath,
            resource.Date,
            createdBy
        );
    }

    public static CreateOrUpdateDocumentationCommand ToCommandFromResourceForUpdate(
        CreateOrUpdateDocumentationResource resource, 
        string? imagePath, 
        int createdBy,
        string existingImagePath)
    {
        // Use new image path if provided, otherwise keep existing
        var finalImagePath = imagePath ?? existingImagePath;

        return new CreateOrUpdateDocumentationCommand(
            resource.Id,
            resource.ProjectId,
            resource.Title,
            resource.Description,
            finalImagePath,
            resource.Date,
            createdBy
        );
    }
}