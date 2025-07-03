namespace BuildTruckBack.Personnel.Interfaces.REST.Resources;

/// <summary>
/// Resource for personnel data responses
/// </summary>
public record PersonnelResource(
    int Id,
    int ProjectId,
    string Name,
    string Lastname,
    string FullName,
    string DocumentNumber,
    string Position,
    string Department,
    string PersonnelType,
    string Status,
    decimal MonthlyAmount,
    decimal TotalAmount,
    decimal Discount,
    string? Bank,
    string? AccountNumber,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Phone,
    string? Email,
    string? AvatarUrl,
    
    // Attendance fields (calculated)
    int WorkedDays,
    int CompensatoryDays,
    int UnpaidLeave,
    int Absences,
    int Sundays,
    int TotalDays,
    
    // 🆕 NUEVO: Attendance data for frontend
    Dictionary<string, string>? MonthlyAttendanceData,
    
    // Timestamps
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt
);