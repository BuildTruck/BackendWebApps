using BuildTruckBack.Personnel.Domain.Model.ValueObjects;

namespace BuildTruckBack.Personnel.Application.Internal.OutboundServices;

/// <summary>
/// Service for external integrations and business logic
/// </summary>
public class ExternalPersonnelService
{
    public static List<string> GetAvailableBanks()
    {
        return Enum.GetNames<BankType>().ToList();
    }

    public static List<string> GetPersonnelTypes()
    {
        return Enum.GetNames<PersonnelType>().ToList();
    }

    public static List<string> GetPersonnelStatuses()
    {
        return Enum.GetNames<PersonnelStatus>().ToList();
    }

    public static List<string> GetAttendanceStatuses()
    {
        return Enum.GetNames<AttendanceStatus>()
            .Where(status => status != nameof(AttendanceStatus.Empty))
            .ToList();
    }

    public static decimal CalculateDailyRate(decimal monthlyAmount)
    {
        return monthlyAmount / 30;
    }

    public static decimal CalculateProportionalAmount(decimal monthlyAmount, int workedDays)
    {
        var dailyRate = CalculateDailyRate(monthlyAmount);
        return dailyRate * workedDays;
    }

    public static bool IsValidDocumentNumber(string documentNumber)
    {
        // Basic validation for Peruvian documents (DNI: 8 digits, CE: varies)
        return !string.IsNullOrEmpty(documentNumber) && 
               documentNumber.Length >= 7 && 
               documentNumber.Length <= 12 &&
               documentNumber.All(char.IsDigit);
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static List<int> GenerateMonthDays(int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        return Enumerable.Range(1, daysInMonth).ToList();
    }

    public static Dictionary<string, object> GetMonthStatistics(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnel,
        int year,
        int month)
    {
        var stats = new Dictionary<string, object>
        {
            ["totalPersonnel"] = personnel.Count(),
            ["activePersonnel"] = personnel.Count(p => p.IsActive()),
            ["totalWorkedDays"] = personnel.Sum(p => p.WorkedDays),
            ["totalAbsences"] = personnel.Sum(p => p.Absences),
            ["totalAmount"] = personnel.Sum(p => p.TotalAmount),
            ["averageAttendance"] = personnel.Any() ? 
                personnel.Average(p => p.WorkedDays + p.CompensatoryDays) : 0
        };

        return stats;
    }
}