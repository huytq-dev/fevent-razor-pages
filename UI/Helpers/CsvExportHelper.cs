using System.Text;

namespace UI.Helpers;

public static class CsvExportHelper
{
    public static string BuildCsv(IEnumerable<string> headers, IEnumerable<IEnumerable<string?>> rows)
    {
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(",", headers.Select(Escape)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(Escape)));
        }

        return sb.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        var normalized = value.Replace("\r", " ").Replace("\n", " ").Replace("\"", "\"\"");
        return $"\"{normalized}\"";
    }
}
