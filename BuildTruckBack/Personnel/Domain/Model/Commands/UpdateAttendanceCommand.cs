using BuildTruckBack.Personnel.Domain.Model.ValueObjects;

namespace BuildTruckBack.Personnel.Domain.Model.Commands;

public record UpdateAttendanceCommand(
    List<PersonnelAttendanceUpdate> PersonnelAttendances
);

public record PersonnelAttendanceUpdate(
    int PersonnelId,
    int Year,
    int Month,
    Dictionary<int, AttendanceStatus> DailyAttendance // Key: day (1-31), Value: status
);