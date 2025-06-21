namespace BuildTruckBack.Personnel.Domain.Model.ValueObjects;

public enum AttendanceStatus
{
    Empty,  // No attendance marked
    X,      // Worked day
    F,      // Absence
    P,      // Compensatory leave
    DD,     // Sunday
    PD      // Unpaid leave
}