using System.ComponentModel.DataAnnotations;

namespace BuildTruckBack.Personnel.Interfaces.REST.Resources;

/// <summary>
/// Resource for updating attendance for multiple personnel
/// </summary>
public class UpdateAttendanceResource
{
    [Required]
    public List<PersonnelAttendanceResource> PersonnelAttendances { get; init; } = new();

    /// <summary>
    /// Validation method for business rules
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (!PersonnelAttendances.Any())
        {
            errors.Add("At least one personnel attendance is required");
            return errors;
        }

        foreach (var attendance in PersonnelAttendances)
        {
            var attendanceErrors = attendance.GetValidationErrors();
            errors.AddRange(attendanceErrors);
        }

        // Check for duplicate personnel IDs
        var duplicateIds = PersonnelAttendances
            .GroupBy(p => p.PersonnelId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicateId in duplicateIds)
        {
            errors.Add($"Duplicate personnel ID: {duplicateId}");
        }

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;
}