using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Personnel.Interfaces.REST.Resources;

/// <summary>
/// Resource for individual personnel attendance data
/// </summary>
public class PersonnelAttendanceResource
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Personnel ID must be greater than 0")]
    public int PersonnelId { get; init; }

    [Required]
    [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100")]
    public int Year { get; init; }

    [Required]
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12")]
    public int Month { get; init; }

    [Required]
    public Dictionary<int, string> DailyAttendance { get; init; } = new(); // Key: day (1-31), Value: status (X, F, P, DD, PD)

    /// <summary>
    /// Validation method for business rules
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Validate daily attendance
        if (!DailyAttendance.Any())
        {
            errors.Add("At least one day of attendance is required");
            return errors;
        }

        var daysInMonth = DateTime.DaysInMonth(Year, Month);
        var validStatuses = new[] { "X", "F", "P", "DD", "PD", "" }; // Empty string for no attendance

        foreach (var (day, status) in DailyAttendance)
        {
            if (day < 1 || day > daysInMonth)
            {
                errors.Add($"Invalid day {day} for month {Month}/{Year}");
            }

            if (!validStatuses.Contains(status))
            {
                errors.Add($"Invalid attendance status '{status}' for day {day}. Valid statuses: X, F, P, DD, PD");
            }
        }

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;
}