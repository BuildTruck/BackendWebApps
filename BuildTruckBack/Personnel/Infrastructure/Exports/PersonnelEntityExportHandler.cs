// Archivo: Personnel/Infrastructure/Exports/PersonnelEntityExportHandler.cs

using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
using BuildTruckBack.Personnel.Domain.Services;

namespace BuildTruckBack.Personnel.Infrastructure.Exports;

public class PersonnelEntityExportHandler : EntityExportHandler
{
    private readonly IPersonnelQueryService _personnelQueryService;

    public PersonnelEntityExportHandler(IPersonnelQueryService personnelQueryService)
    {
        _personnelQueryService = personnelQueryService;
    }

    public override string EntityType => "personnel";

    public override async Task<IEnumerable<object>> GetDataAsync(int projectId, Dictionary<string, string> filters)
    {
        // Si incluye asistencia con año y mes específicos
        if (filters != null && 
            filters.ContainsKey("includeAttendance") && 
            bool.TryParse(filters["includeAttendance"], out bool includeAttendance) && 
            includeAttendance &&
            filters.ContainsKey("year") && 
            filters.ContainsKey("month") &&
            int.TryParse(filters["year"], out int year) &&
            int.TryParse(filters["month"], out int month))
        {
            var personnelWithAttendance = await _personnelQueryService.GetPersonnelWithAttendanceAsync(
                projectId, year, month, true);
            return personnelWithAttendance.Cast<object>();
        }

        // Si solo quiere personal activo
        if (filters != null && 
            filters.ContainsKey("activeOnly") && 
            bool.TryParse(filters["activeOnly"], out bool activeOnly) && 
            activeOnly)
        {
            var activePersonnel = await _personnelQueryService.GetActivePersonnelByProjectAsync(projectId);
            return activePersonnel.Cast<object>();
        }

        // Por defecto: todo el personal del proyecto
        var personnel = await _personnelQueryService.GetPersonnelByProjectAsync(projectId);
        return personnel.Cast<object>();
    }

    public override List<ExportColumn> GetColumns()
    {
        return new List<ExportColumn>
        {
            new() { PropertyName = "Id", DisplayName = "ID", DataType = "number", Order = 0, IsVisible = true },
            new() { PropertyName = "Name", DisplayName = "Name", DataType = "string", Order = 1, IsVisible = true },
            new() { PropertyName = "Lastname", DisplayName = "Last Name", DataType = "string", Order = 2, IsVisible = true },
            new() { PropertyName = "DocumentNumber", DisplayName = "Document", DataType = "string", Order = 3, IsVisible = true },
            new() { PropertyName = "Position", DisplayName = "Position", DataType = "string", Order = 4, IsVisible = true },
            new() { PropertyName = "Department", DisplayName = "Department", DataType = "string", Order = 5, IsVisible = true },
            new() { PropertyName = "PersonnelType", DisplayName = "Type", DataType = "string", Order = 6, IsVisible = true },
            new() { PropertyName = "Status", DisplayName = "Status", DataType = "string", Order = 7, IsVisible = true },
            new() { PropertyName = "MonthlyAmount", DisplayName = "Monthly Amount", DataType = "currency", Order = 8, IsVisible = true, Format = "S/. #,##0.00" },
            new() { PropertyName = "TotalAmount", DisplayName = "Total Amount", DataType = "currency", Order = 9, IsVisible = true, Format = "S/. #,##0.00" },
            new() { PropertyName = "Discount", DisplayName = "Discount", DataType = "currency", Order = 10, IsVisible = true, Format = "S/. #,##0.00" },
            new() { PropertyName = "Bank", DisplayName = "Bank", DataType = "string", Order = 11, IsVisible = true },
            new() { PropertyName = "AccountNumber", DisplayName = "Account Number", DataType = "string", Order = 12, IsVisible = true },
            new() { PropertyName = "StartDate", DisplayName = "Start Date", DataType = "date", Order = 13, IsVisible = true, Format = "dd/MM/yyyy" },
            new() { PropertyName = "EndDate", DisplayName = "End Date", DataType = "date", Order = 14, IsVisible = true, Format = "dd/MM/yyyy" },
            new() { PropertyName = "Phone", DisplayName = "Phone", DataType = "string", Order = 15, IsVisible = true },
            new() { PropertyName = "Email", DisplayName = "Email", DataType = "string", Order = 16, IsVisible = true },
            new() { PropertyName = "WorkedDays", DisplayName = "Worked Days", DataType = "number", Order = 17, IsVisible = true },
            new() { PropertyName = "CompensatoryDays", DisplayName = "Compensatory Days", DataType = "number", Order = 18, IsVisible = true },
            new() { PropertyName = "Absences", DisplayName = "Absences", DataType = "number", Order = 19, IsVisible = true },
            new() { PropertyName = "UnpaidLeave", DisplayName = "Unpaid Leave", DataType = "number", Order = 20, IsVisible = true },
            new() { PropertyName = "Sundays", DisplayName = "Sundays", DataType = "number", Order = 21, IsVisible = true },
            new() { PropertyName = "TotalDays", DisplayName = "Total Days", DataType = "number", Order = 22, IsVisible = true }
        };
    }

    public override string GetFileName(string format, Dictionary<string, string> filters)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var baseFileName = "Personnel_Report";
        
        // Si incluye asistencia, cambiar el nombre
        if (filters != null && 
            filters.ContainsKey("includeAttendance") && 
            bool.TryParse(filters["includeAttendance"], out bool includeAttendance) && 
            includeAttendance &&
            filters.ContainsKey("year") && 
            filters.ContainsKey("month"))
        {
            baseFileName = $"Personnel_Attendance_{filters["year"]}_{filters["month"]}";
        }
        else if (filters != null && 
                 filters.ContainsKey("activeOnly") && 
                 bool.TryParse(filters["activeOnly"], out bool activeOnly) && 
                 activeOnly)
        {
            baseFileName = "Personnel_Active";
        }
        
        var extension = format.ToLower() switch
        {
            "excel" => "xlsx",
            "pdf" => "pdf", 
            "csv" => "csv",
            _ => "xlsx"
        };
        
        return $"{baseFileName}_{timestamp}.{extension}";
    }
}