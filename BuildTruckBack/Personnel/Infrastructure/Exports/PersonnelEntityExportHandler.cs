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
        try
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
                Console.WriteLine($"Personnel with attendance count: {personnelWithAttendance.Count()}");
                return personnelWithAttendance.Cast<object>();
            }

            // Si solo quiere personal activo
            if (filters != null && 
                filters.ContainsKey("activeOnly") && 
                bool.TryParse(filters["activeOnly"], out bool activeOnly) && 
                activeOnly)
            {
                var activePersonnel = await _personnelQueryService.GetActivePersonnelByProjectAsync(projectId);
                Console.WriteLine($"Active personnel count: {activePersonnel.Count()}");
                return activePersonnel.Cast<object>();
            }

            // Por defecto: todo el personal del proyecto
            var personnel = await _personnelQueryService.GetPersonnelByProjectAsync(projectId);
            Console.WriteLine($"Total personnel count for project {projectId}: {personnel.Count()}");
            return personnel.Cast<object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting personnel data: {ex.Message}");
            throw;
        }
    }

    public override List<ExportColumn> GetColumns()
    {
        return new List<ExportColumn>
        {
            // Basic Information
            new() { PropertyName = "Id", DisplayName = "ID", DataType = "number", Order = 0, IsVisible = true, Width = 60 },
            new() { PropertyName = "Name", DisplayName = "Name", DataType = "string", Order = 1, IsVisible = true, Width = 120 },
            new() { PropertyName = "Lastname", DisplayName = "Last Name", DataType = "string", Order = 2, IsVisible = true, Width = 120 },
            new() { PropertyName = "DocumentNumber", DisplayName = "Document Number", DataType = "string", Order = 3, IsVisible = true, Width = 100 },
            new() { PropertyName = "Position", DisplayName = "Position", DataType = "string", Order = 4, IsVisible = true, Width = 150 },
            new() { PropertyName = "Department", DisplayName = "Department", DataType = "string", Order = 5, IsVisible = true, Width = 120 },
            new() { PropertyName = "PersonnelType", DisplayName = "Personnel Type", DataType = "string", Order = 6, IsVisible = true, Width = 100 },
            new() { PropertyName = "Status", DisplayName = "Status", DataType = "string", Order = 7, IsVisible = true, Width = 80 },
            
            // Financial Information
            new() { PropertyName = "MonthlyAmount", DisplayName = "Monthly Amount", DataType = "currency", Order = 8, IsVisible = true, Width = 120, Format = "S/. #,##0.00", Alignment = "right" },
            new() { PropertyName = "TotalAmount", DisplayName = "Total Amount", DataType = "currency", Order = 9, IsVisible = true, Width = 120, Format = "S/. #,##0.00", Alignment = "right" },
            new() { PropertyName = "Discount", DisplayName = "Discount", DataType = "currency", Order = 10, IsVisible = true, Width = 100, Format = "S/. #,##0.00", Alignment = "right" },
            new() { PropertyName = "Bank", DisplayName = "Bank", DataType = "string", Order = 11, IsVisible = true, Width = 100 },
            new() { PropertyName = "AccountNumber", DisplayName = "Account Number", DataType = "string", Order = 12, IsVisible = true, Width = 140 },
            
            // Contract Information
            new() { PropertyName = "StartDate", DisplayName = "Start Date", DataType = "date", Order = 13, IsVisible = true, Width = 100, Format = "dd/MM/yyyy", Alignment = "center" },
            new() { PropertyName = "EndDate", DisplayName = "End Date", DataType = "date", Order = 14, IsVisible = true, Width = 100, Format = "dd/MM/yyyy", Alignment = "center" },
            
            // Contact Information
            new() { PropertyName = "Phone", DisplayName = "Phone", DataType = "string", Order = 15, IsVisible = true, Width = 120 },
            new() { PropertyName = "Email", DisplayName = "Email", DataType = "string", Order = 16, IsVisible = true, Width = 200 },
            
            // Attendance Information
            new() { PropertyName = "WorkedDays", DisplayName = "Worked Days", DataType = "number", Order = 17, IsVisible = true, Width = 90, Alignment = "center" },
            new() { PropertyName = "CompensatoryDays", DisplayName = "Compensatory Days", DataType = "number", Order = 18, IsVisible = true, Width = 120, Alignment = "center" },
            new() { PropertyName = "UnpaidLeave", DisplayName = "Unpaid Leave", DataType = "number", Order = 19, IsVisible = true, Width = 100, Alignment = "center" },
            new() { PropertyName = "Absences", DisplayName = "Absences", DataType = "number", Order = 20, IsVisible = true, Width = 80, Alignment = "center" },
            new() { PropertyName = "Sundays", DisplayName = "Sundays", DataType = "number", Order = 21, IsVisible = true, Width = 80, Alignment = "center" },
            new() { PropertyName = "TotalDays", DisplayName = "Total Days", DataType = "number", Order = 22, IsVisible = true, Width = 90, Alignment = "center" }
        };
    }

    public override ExportOptions GetDefaultOptions()
    {
        return new ExportOptions
        {
            Title = "Personnel Export Report",
            Subtitle = $"Generated on {DateTime.Now:dd/MM/yyyy HH:mm}",
            IncludeHeader = true,
            IncludeFooter = true,
            AutoFitColumns = true,
            Columns = GetColumns()
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
            var monthName = new DateTime(int.Parse(filters["year"]), int.Parse(filters["month"]), 1).ToString("MMMM_yyyy");
            baseFileName = $"Personnel_Attendance_{monthName}";
        }
        else if (filters != null && 
                 filters.ContainsKey("activeOnly") && 
                 bool.TryParse(filters["activeOnly"], out bool activeOnly) && 
                 activeOnly)
        {
            baseFileName = "Personnel_Active_Only";
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