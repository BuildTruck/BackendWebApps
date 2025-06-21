namespace BuildTruckBack.Personnel.Domain.Model.ValueObjects;

public record MonthlyAttendance
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string AttendanceString { get; init; } = string.Empty;
    
    public MonthlyAttendance(int year, int month, string attendanceString)
    {
        if (year < 1900 || year > 2100)
            throw new ArgumentException("Invalid year", nameof(year));
            
        if (month < 1 || month > 12)
            throw new ArgumentException("Invalid month", nameof(month));
            
        Year = year;
        Month = month;
        AttendanceString = attendanceString ?? string.Empty;
    }
    
    public string GetMonthKey() => $"{Year}-{Month:D2}";
    
    public AttendanceStatus[] GetDailyAttendance()
    {
        if (string.IsNullOrEmpty(AttendanceString))
            return Array.Empty<AttendanceStatus>();
            
        return AttendanceString
            .Split('|')
            .Select(s => Enum.TryParse<AttendanceStatus>(s, out var status) ? status : AttendanceStatus.Empty)
            .ToArray();
    }
    
    public static MonthlyAttendance Empty(int year, int month) => new(year, month, string.Empty);
}