using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Models;

public class PdfGeneratorService : IPdfGeneratorService
{
    public async Task<byte[]> GeneratePdfAsync<T>(IEnumerable<T> data, ExportOptions options)
    {
        return await Task.Run(() =>
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();
            
            // Title
            if (!string.IsNullOrEmpty(options.Title))
            {
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var titleParagraph = new Paragraph(options.Title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(titleParagraph);
            }
            
            // Data table
            var dataList = data.ToList();
            if (dataList.Any())
            {
                var columns = options.Columns.Where(c => c.IsVisible).OrderBy(c => c.Order).ToList();
                var table = new PdfPTable(columns.Count)
                {
                    WidthPercentage = 100
                };
                
                // Headers
                foreach (var column in columns)
                {
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    var cell = new PdfPCell(new Phrase(column.DisplayName, headerFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }
                
                // Data rows
                foreach (var item in dataList)
                {
                    foreach (var column in columns)
                    {
                        var value = GetPropertyValue(item, column.PropertyName);
                        var formattedValue = FormatValue(value, column)?.ToString() ?? "";
                        
                        var cell = new PdfPCell(new Phrase(formattedValue, FontFactory.GetFont(FontFactory.HELVETICA, 9)))
                        {
                            Padding = 3
                        };
                        table.AddCell(cell);
                    }
                }
                
                document.Add(table);
            }
            
            document.Close();
            return memoryStream.ToArray();
        });
    }
    
    public async Task<byte[]> GenerateReportPdfAsync<T>(IEnumerable<T> data, string title, ExportOptions options)
    {
        options.Title = title;
        return await GeneratePdfAsync(data, options);
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
}