using BuildTruckBack.Personnel.Interfaces.REST.Resources;

namespace BuildTruckBack.Personnel.Interfaces.REST.Transform;

public static class PersonnelResourceFromEntityAssembler
{
    public static PersonnelResource ToResourceFromEntity(Domain.Model.Aggregates.Personnel personnel)
    {
        return new PersonnelResource(
            personnel.Id,
            personnel.ProjectId,
            personnel.Name,
            personnel.Lastname,
            personnel.GetFullName(),
            personnel.DocumentNumber,
            personnel.Position,
            personnel.Department,
            personnel.PersonnelType.ToString(),
            personnel.Status.ToString(),
            personnel.MonthlyAmount,
            personnel.TotalAmount,
            personnel.Discount,
            personnel.Bank,
            personnel.AccountNumber,
            personnel.StartDate,
            personnel.EndDate,
            personnel.Phone,
            personnel.Email,
            personnel.AvatarUrl,
            personnel.WorkedDays,
            personnel.CompensatoryDays,
            personnel.UnpaidLeave,
            personnel.Absences,
            personnel.Sundays,
            personnel.TotalDays,
            personnel.CreatedDate,
            personnel.UpdatedDate
        );
    }

    public static IEnumerable<PersonnelResource> ToResourceFromEntity(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnel)
    {
        return personnel.Select(ToResourceFromEntity);
    }
}