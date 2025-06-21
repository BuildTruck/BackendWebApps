using BuildTruckBack.Personnel.Domain.Model.Commands;
using BuildTruckBack.Personnel.Domain.Model.ValueObjects;
using BuildTruckBack.Personnel.Interfaces.REST.Resources;

namespace BuildTruckBack.Personnel.Interfaces.REST.Transform;

public static class AttendanceResourceFromEntityAssembler
{
    public static UpdateAttendanceCommand ToCommandFromResource(UpdateAttendanceResource resource)
    {
        var personnelAttendances = resource.PersonnelAttendances.Select(ToPersonnelAttendanceUpdate).ToList();
        return new UpdateAttendanceCommand(personnelAttendances);
    }

    private static PersonnelAttendanceUpdate ToPersonnelAttendanceUpdate(PersonnelAttendanceResource resource)
    {
        var dailyAttendance = new Dictionary<int, AttendanceStatus>();

        foreach (var (day, statusStr) in resource.DailyAttendance)
        {
            if (Enum.TryParse<AttendanceStatus>(statusStr, out var status))
            {
                dailyAttendance[day] = status;
            }
            else
            {
                throw new ArgumentException($"Invalid attendance status: {statusStr} for day {day}");
            }
        }

        return new PersonnelAttendanceUpdate(
            resource.PersonnelId,
            resource.Year,
            resource.Month,
            dailyAttendance
        );
    }
}