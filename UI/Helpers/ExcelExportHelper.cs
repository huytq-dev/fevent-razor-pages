using ClosedXML.Excel;

namespace UI.Helpers;

public static class ExcelExportHelper
{
    public static byte[] BuildWorkbook(
        IEnumerable<string> headers,
        IEnumerable<IEnumerable<string?>> rows,
        string worksheetName = "Report")
    {
        using var workbook = new XLWorkbook();
        var safeWorksheetName = string.IsNullOrWhiteSpace(worksheetName) ? "Report" : worksheetName.Trim();
        var worksheet = workbook.Worksheets.Add(safeWorksheetName.Length > 31 ? safeWorksheetName[..31] : safeWorksheetName);

        var headerList = headers.ToList();
        for (var col = 0; col < headerList.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = headerList[col];
        }

        var headerRange = worksheet.Range(1, 1, 1, Math.Max(1, headerList.Count));
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F766E");
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        var rowIndex = 2;
        foreach (var row in rows)
        {
            var rowValues = row.ToList();
            for (var col = 0; col < headerList.Count; col++)
            {
                worksheet.Cell(rowIndex, col + 1).Value = rowValues.ElementAtOrDefault(col) ?? string.Empty;
            }

            rowIndex++;
        }

        var lastRow = Math.Max(1, rowIndex - 1);
        var tableRange = worksheet.Range(1, 1, lastRow, Math.Max(1, headerList.Count));
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        worksheet.SheetView.FreezeRows(1);
        worksheet.RangeUsed()?.SetAutoFilter();
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}