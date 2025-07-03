using BuildTruckBack.Personnel.Interfaces.REST.Resources;

namespace BuildTruckBack.Personnel.Interfaces.REST.Transform;

public static class PersonnelResourceFromEntityAssembler
{
    /// <summary>
    /// Convert Personnel entity to PersonnelResource with optional attendance data
    /// </summary>
    /// <param name="personnel">Personnel entity</param>
    /// <param name="includeAttendanceData">Whether to include monthly attendance data</param>
    /// <returns>PersonnelResource</returns>
    public static PersonnelResource ToResourceFromEntity(
        Domain.Model.Aggregates.Personnel personnel, 
        bool includeAttendanceData = false)
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
            
            // 🆕 NUEVO: Include attendance data only if requested
            includeAttendanceData ? personnel.MonthlyAttendance : null,
            
            personnel.CreatedDate,
            personnel.UpdatedDate
        );
    }

    /// <summary>
    /// Convert Personnel entity to PersonnelResource (backward compatibility)
    /// </summary>
    /// <param name="personnel">Personnel entity</param>
    /// <returns>PersonnelResource without attendance data</returns>
    public static PersonnelResource ToResourceFromEntity(Domain.Model.Aggregates.Personnel personnel)
    {
        return ToResourceFromEntity(personnel, false);
    }

    /// <summary>
    /// Convert collection of Personnel entities to PersonnelResource collection with optional attendance data
    /// </summary>
    /// <param name="personnel">Collection of Personnel entities</param>
    /// <param name="includeAttendanceData">Whether to include monthly attendance data</param>
    /// <returns>Collection of PersonnelResource</returns>
    public static IEnumerable<PersonnelResource> ToResourceFromEntity(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnel,
        bool includeAttendanceData = false)
    {
        return personnel.Select(p => ToResourceFromEntity(p, includeAttendanceData));
    }

    /// <summary>
    /// Convert collection of Personnel entities to PersonnelResource collection (backward compatibility)
    /// </summary>
    /// <param name="personnel">Collection of Personnel entities</param>
    /// <returns>Collection of PersonnelResource without attendance data</returns>
    public static IEnumerable<PersonnelResource> ToResourceFromEntity(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnel)
    {
        return ToResourceFromEntity(personnel, false);
    }
}