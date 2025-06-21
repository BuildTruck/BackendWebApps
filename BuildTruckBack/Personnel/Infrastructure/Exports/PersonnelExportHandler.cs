using ClosedXML.Excel;
using System.Data;

namespace BuildTruckBack.Personnel.Infrastructure.Exports;

/// <summary>
/// Handler for exporting Personnel data to various formats
/// </summary>
public class PersonnelExportHandler
{
    public byte[] ExportToExcel(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnel, 
        string sheetName = "Personnel",
        bool includeAttendance = false)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Headers
        var headers = new List<string>
        {
            "ID",
            "Name",
            "Last Name",
            "Document Number",
            "Position",
            "Department",
            "Personnel Type",
            "Status",
            "Monthly Amount",
            "Total Amount",
            "Discount",
            "Bank",
            "Account Number",
            "Start Date",
            "End Date",
            "Phone",
            "Email"
        };

        if (includeAttendance)
        {
            headers.AddRange(new[]
            {
                "Worked Days",
                "Compensatory Days",
                "Unpaid Leave",
                "Absences",
                "Sundays",
                "Total Days"
            });
        }

        // Set headers
        for (int i = 0; i < headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Data rows
        var row = 2;
        foreach (var person in personnel.Where(p => !p.IsDeleted))
        {
            var col = 1;
            worksheet.Cell(row, col++).Value = person.Id;
            worksheet.Cell(row, col++).Value = person.Name;
            worksheet.Cell(row, col++).Value = person.Lastname;
            worksheet.Cell(row, col++).Value = person.DocumentNumber;
            worksheet.Cell(row, col++).Value = person.Position;
            worksheet.Cell(row, col++).Value = person.Department;
            worksheet.Cell(row, col++).Value = person.PersonnelType.ToString();
            worksheet.Cell(row, col++).Value = person.Status.ToString();
            worksheet.Cell(row, col++).Value = person.MonthlyAmount;
            worksheet.Cell(row, col++).Value = person.TotalAmount;
            worksheet.Cell(row, col++).Value = person.Discount;
            worksheet.Cell(row, col++).Value = person.Bank;
            worksheet.Cell(row, col++).Value = person.AccountNumber;
            worksheet.Cell(row, col++).Value = person.StartDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, col++).Value = person.EndDate?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, col++).Value = person.Phone;
            worksheet.Cell(row, col++).Value = person.Email;

            if (includeAttendance)
            {
                worksheet.Cell(row, col++).Value = person.WorkedDays;
                worksheet.Cell(row, col++).Value = person.CompensatoryDays;
                worksheet.Cell(row, col++).Value = person.UnpaidLeave;
                worksheet.Cell(row, col++).Value = person.Absences;
                worksheet.Cell(row, col++).Value = person.Sundays;
                worksheet.Cell(row, col++).Value = person.TotalDays;
            }

            row++;
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        // Format currency columns
        var monthlyAmountCol = headers.IndexOf("Monthly Amount") + 1;
        var totalAmountCol = headers.IndexOf("Total Amount") + 1;
        var discountCol = headers.IndexOf("Discount") + 1;

        if (monthlyAmountCol > 0)
            worksheet.Column(monthlyAmountCol).Style.NumberFormat.Format = "S/ #,##0.00";
        if (totalAmountCol > 0)
            worksheet.Column(totalAmountCol).Style.NumberFormat.Format = "S/ #,##0.00";
        if (discountCol > 0)
            worksheet.Column(discountCol).Style.NumberFormat.Format = "S/ #,##0.00";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportAttendanceToExcel(
        IEnumerable<Domain.Model.Aggregates.Personnel> personnel,
        int year,
        int month,
        string sheetName = "Attendance")
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

        // Title
        worksheet.Cell(1, 1).Value = $"Attendance Report - {monthName}";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range(1, 1, 1, daysInMonth + 8).Merge();

        // Headers
        var row = 3;
        var col = 1;

        worksheet.Cell(row, col++).Value = "Name";
        worksheet.Cell(row, col++).Value = "Position";
        worksheet.Cell(row, col++).Value = "Department";

        // Day headers
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            worksheet.Cell(row, col).Value = day;
            worksheet.Cell(row + 1, col).Value = date.ToString("ddd")[..1]; // First letter of day
            
            // Highlight Sundays
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Cell(row + 1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }
            
            col++;
        }

        // Summary headers
        worksheet.Cell(row, col++).Value = "Worked";
        worksheet.Cell(row, col++).Value = "Comp.";
        worksheet.Cell(row, col++).Value = "Absent";
        worksheet.Cell(row, col++).Value = "Unpaid";
        worksheet.Cell(row, col++).Value = "Total";

        // Header styling
        worksheet.Range(row, 1, row + 1, col - 1).Style.Font.Bold = true;
        worksheet.Range(row, 1, row + 1, col - 1).Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data rows
        row += 2;
        foreach (var person in personnel.Where(p => !p.IsDeleted).OrderBy(p => p.Name))
        {
            col = 1;
            worksheet.Cell(row, col++).Value = person.GetFullName();
            worksheet.Cell(row, col++).Value = person.Position;
            worksheet.Cell(row, col++).Value = person.Department;

            // Daily attendance
            for (int day = 1; day <= daysInMonth; day++)
            {
                var status = person.GetDayAttendance(year, month, day);
                var statusStr = status.ToString();
                
                if (status == Domain.Model.ValueObjects.AttendanceStatus.Empty)
                {
                    var date = new DateTime(year, month, day);
                    statusStr = date.DayOfWeek == DayOfWeek.Sunday ? "DD" : "";
                }

                worksheet.Cell(row, col).Value = statusStr;

                // Color coding
                var cellColor = status switch
                {
                    Domain.Model.ValueObjects.AttendanceStatus.X => XLColor.LightGreen,
                    Domain.Model.ValueObjects.AttendanceStatus.F => XLColor.LightPink,
                    Domain.Model.ValueObjects.AttendanceStatus.P => XLColor.LightBlue,
                    Domain.Model.ValueObjects.AttendanceStatus.DD => XLColor.LightGray,
                    Domain.Model.ValueObjects.AttendanceStatus.PD => XLColor.Orange,
                    _ => XLColor.White
                };

                if (cellColor != XLColor.White)
                {
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = cellColor;
                }

                col++;
            }

            // Summary columns
            worksheet.Cell(row, col++).Value = person.WorkedDays;
            worksheet.Cell(row, col++).Value = person.CompensatoryDays;
            worksheet.Cell(row, col++).Value = person.Absences;
            worksheet.Cell(row, col++).Value = person.UnpaidLeave;
            worksheet.Cell(row, col++).Value = person.TotalDays;

            row++;
        }

        // Legend
        row += 2;
        worksheet.Cell(row, 1).Value = "Legend:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;

        var legendItems = new[]
        {
            ("X", "Worked Day", XLColor.LightGreen),
            ("F", "Absence", XLColor.LightPink),
            ("P", "Compensatory", XLColor.LightBlue),
            ("DD", "Sunday", XLColor.LightGray),
            ("PD", "Unpaid Leave", XLColor.Orange)
        };

        foreach (var (code, description, color) in legendItems)
        {
            worksheet.Cell(row, 1).Value = code;
            worksheet.Cell(row, 1).Style.Fill.BackgroundColor = color;
            worksheet.Cell(row, 2).Value = description;
            row++;
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public DataTable ConvertToDataTable(IEnumerable<Domain.Model.Aggregates.Personnel> personnel)
    {
        var table = new DataTable("Personnel");

        // Columns
        table.Columns.Add("ID", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("LastName", typeof(string));
        table.Columns.Add("DocumentNumber", typeof(string));
        table.Columns.Add("Position", typeof(string));
        table.Columns.Add("Department", typeof(string));
        table.Columns.Add("PersonnelType", typeof(string));
        table.Columns.Add("Status", typeof(string));
        table.Columns.Add("MonthlyAmount", typeof(decimal));
        table.Columns.Add("TotalAmount", typeof(decimal));
        table.Columns.Add("Discount", typeof(decimal));
        table.Columns.Add("Bank", typeof(string));
        table.Columns.Add("AccountNumber", typeof(string));
        table.Columns.Add("StartDate", typeof(DateTime));
        table.Columns.Add("EndDate", typeof(DateTime));
        table.Columns.Add("Phone", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("WorkedDays", typeof(int));
        table.Columns.Add("CompensatoryDays", typeof(int));
        table.Columns.Add("Absences", typeof(int));
        table.Columns.Add("UnpaidLeave", typeof(int));
        table.Columns.Add("TotalDays", typeof(int));

        // Rows
        foreach (var person in personnel.Where(p => !p.IsDeleted))
        {
            table.Rows.Add(
                person.Id,
                person.Name,
                person.Lastname,
                person.DocumentNumber,
                person.Position,
                person.Department,
                person.PersonnelType.ToString(),
                person.Status.ToString(),
                person.MonthlyAmount,
                person.TotalAmount,
                person.Discount,
                person.Bank,
                person.AccountNumber,
                person.StartDate,
                person.EndDate,
                person.Phone,
                person.Email,
                person.WorkedDays,
                person.CompensatoryDays,
                person.Absences,
                person.UnpaidLeave,
                person.TotalDays
            );
        }

        return table;
    }

    public byte[] ExportToCsv(IEnumerable<Domain.Model.Aggregates.Personnel> personnel)
    {
        var csv = new System.Text.StringBuilder();
        
        // Headers
        csv.AppendLine("ID,Name,LastName,DocumentNumber,Position,Department,PersonnelType,Status,MonthlyAmount,TotalAmount,Discount,Bank,AccountNumber,StartDate,EndDate,Phone,Email,WorkedDays,CompensatoryDays,Absences,UnpaidLeave,TotalDays");

        // Data
        foreach (var person in personnel.Where(p => !p.IsDeleted))
        {
            csv.AppendLine($"{person.Id}," +
                          $"\"{person.Name}\"," +
                          $"\"{person.Lastname}\"," +
                          $"\"{person.DocumentNumber}\"," +
                          $"\"{person.Position}\"," +
                          $"\"{person.Department}\"," +
                          $"{person.PersonnelType}," +
                          $"{person.Status}," +
                          $"{person.MonthlyAmount}," +
                          $"{person.TotalAmount}," +
                          $"{person.Discount}," +
                          $"\"{person.Bank}\"," +
                          $"\"{person.AccountNumber}\"," +
                          $"{person.StartDate?.ToString("yyyy-MM-dd")}," +
                          $"{person.EndDate?.ToString("yyyy-MM-dd")}," +
                          $"\"{person.Phone}\"," +
                          $"\"{person.Email}\"," +
                          $"{person.WorkedDays}," +
                          $"{person.CompensatoryDays}," +
                          $"{person.Absences}," +
                          $"{person.UnpaidLeave}," +
                          $"{person.TotalDays}");
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }
}