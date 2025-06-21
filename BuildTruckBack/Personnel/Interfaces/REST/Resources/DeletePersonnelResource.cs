namespace BuildTruckBack.Personnel.Interfaces.REST.Resources;

/// <summary>
/// Resource for delete personnel operation response
/// </summary>
public record DeletePersonnelResource(
    bool Success,
    string Message
);