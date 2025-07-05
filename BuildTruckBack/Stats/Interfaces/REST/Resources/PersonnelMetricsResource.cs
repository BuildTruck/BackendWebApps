namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

/// <summary>
/// Resource representing personnel metrics
/// </summary>
public record PersonnelMetricsResource(
    int TotalPersonnel,
    int ActivePersonnel,
    int InactivePersonnel,
    Dictionary<string, int> PersonnelByType,
    decimal TotalSalaryAmount,
    decimal AverageAttendanceRate,
    decimal ActiveRate,
    decimal InactiveRate,
    decimal AverageSalary,
    string DominantPersonnelType,
    bool HasGoodAttendance,
    string AttendanceStatus,
    string PersonnelSummary,
    decimal EfficiencyScore
);