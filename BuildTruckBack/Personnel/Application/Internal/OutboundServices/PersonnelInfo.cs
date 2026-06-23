namespace BuildTruckBack.Personnel.Application.Internal.OutboundServices;

public record PersonnelInfo(
    int Id,
    int ProjectId,
    string Name,
    string Lastname,
    string PersonnelType,
    string Status,
    decimal TotalAmount,
    int WorkedDays,
    int CompensatoryDays,
    int TotalDays,
    int Absences
)
{
    public string GetFullName() => $"{Name} {Lastname}".Trim();
    public bool IsActive() => Status == "ACTIVE";
}
