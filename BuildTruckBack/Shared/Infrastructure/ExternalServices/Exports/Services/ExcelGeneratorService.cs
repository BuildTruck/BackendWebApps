using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Configuration;

namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;

public class ExcelGeneratorService : IExcelGeneratorService
{
    public async Task<byte[]> GenerateExcelAsync<T>(IEnumerable<T> data, ExportOptions options)
    {
        return await Task.Run(() =>
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Data");
            
            var dataList = data.ToList();
            if (!dataList.Any()) return package.GetAsByteArray();
            
            var columns = options.Columns.Any() ? options.Columns : GetDefaultColumns<T>();
            
            // Header
            if (options.IncludeHeader)
            {
                AddTitle(worksheet, options);
                AddColumnHeaders(worksheet, columns, options);
            }
            
            // Data
            AddDataRows(worksheet, dataList, columns, options);
            
            // Formatting
            ApplyFormatting(worksheet, columns, options);
            
            return package.GetAsByteArray();
        });
    }
    
    public async Task<byte[]> GenerateExcelWithMultipleSheetsAsync(
        Dictionary<string, object> sheetsData, 
        ExportOptions options)
    {
        return await Task.Run(() =>
        {
            using var package = new ExcelPackage();
            
            foreach (var sheet in sheetsData)
            {
                var worksheet = package.Workbook.Worksheets.Add(sheet.Key);
                // Process each sheet data...
                // Implementation similar to GenerateExcelAsync
            }
            
            return package.GetAsByteArray();
        });
    }
    
    private List<ExportColumn> GetDefaultColumns<T>()
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select((prop, index) => new ExportColumn
            {
                PropertyName = prop.Name,
                DisplayName = prop.Name,
                DataType = GetDataType(prop.PropertyType),
                Order = index
            }).ToList();
    }
    
    private void AddTitle(ExcelWorksheet worksheet, ExportOptions options)
    {
        if (!string.IsNullOrEmpty(options.Title))
        {
            worksheet.Cells[1, 1].Value = options.Title;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
        }
        
        if (!string.IsNullOrEmpty(options.Subtitle))
        {
            worksheet.Cells[2, 1].Value = options.Subtitle;
            worksheet.Cells[2, 1].Style.Font.Size = 12;
        }
    }
    
    private void AddColumnHeaders(ExcelWorksheet worksheet, List<ExportColumn> columns, ExportOptions options)
    {
        var headerRow = string.IsNullOrEmpty(options.Title) ? 1 : 3;
        var visibleColumns = columns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
        
        for (int i = 0; i < visibleColumns.Count; i++)
        {
            var cell = worksheet.Cells[headerRow, i + 1];
            cell.Value = visibleColumns[i].DisplayName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }
    }
    
    private void AddDataRows<T>(ExcelWorksheet worksheet, List<T> data, List<ExportColumn> columns, ExportOptions options)
    {
        var startRow = string.IsNullOrEmpty(options.Title) ? 2 : 4;
        var visibleColumns = columns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
        
        for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
        {
            var item = data[rowIndex];
            
            for (int colIndex = 0; colIndex < visibleColumns.Count; colIndex++)
            {
                var column = visibleColumns[colIndex];
                var value = GetPropertyValue(item, column.PropertyName);
                var cell = worksheet.Cells[startRow + rowIndex, colIndex + 1];
                
                cell.Value = FormatValue(value, column);
                ApplyCellFormatting(cell, column);
            }
        }
    }
    
    private void ApplyFormatting(ExcelWorksheet worksheet, List<ExportColumn> columns, ExportOptions options)
    {
        if (options.AutoFitColumns)
        {
            worksheet.Cells.AutoFitColumns();
        }
        
        // Apply column widths
        var visibleColumns = columns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
        for (int i = 0; i < visibleColumns.Count; i++)
        {
            if (visibleColumns[i].Width > 0)
            {
                worksheet.Column(i + 1).Width = visibleColumns[i].Width / 7.0; // Excel units
            }
        }
    }
    
    private object? GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    }
    
    private object? FormatValue(object? value, ExportColumn column)
    {
        if (value == null) return string.Empty;
        
        return column.DataType switch
        {
            "date" => value is DateTime date ? date.ToString(column.Format ?? "dd/MM/yyyy") : value,
            "currency" => value is decimal currency ? currency.ToString(column.Format ?? "C") : value,
            "number" => value is decimal number ? number.ToString(column.Format ?? "N2") : value,
            _ => value
        };
    }
    
    private void ApplyCellFormatting(ExcelRange cell, ExportColumn column)
    {
        cell.Style.HorizontalAlignment = column.Alignment switch
        {
            "center" => ExcelHorizontalAlignment.Center,
            "right" => ExcelHorizontalAlignment.Right,
            _ => ExcelHorizontalAlignment.Left
        };
    }
    
    private string GetDataType(Type type)
    {
        if (type == typeof(DateTime) || type == typeof(DateTime?)) return "date";
        if (type == typeof(decimal) || type == typeof(decimal?)) return "currency";
        if (type == typeof(int) || type == typeof(double) || type == typeof(float)) return "number";
        if (type == typeof(bool) || type == typeof(bool?)) return "boolean";
        return "string";
    }
}