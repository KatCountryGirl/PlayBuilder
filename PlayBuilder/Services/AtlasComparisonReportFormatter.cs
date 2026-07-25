using System.Text;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public enum AtlasComparisonFilter
{
    All,
    AgreementsOnly,
    DisagreementsOnly
}

public static class AtlasComparisonReportFormatter
{
    public static IReadOnlyList<AtlasComparisonRow> FilterRows(
        AtlasComparisonReport report,
        AtlasComparisonFilter filter,
        string? searchText)
    {
        ArgumentNullException.ThrowIfNull(report);

        var rows = report.Rows.AsEnumerable();
        rows = filter switch
        {
            AtlasComparisonFilter.AgreementsOnly => rows.Where(row => row.EnginesAgree),
            AtlasComparisonFilter.DisagreementsOnly => rows.Where(row => !row.EnginesAgree),
            _ => rows
        };

        var search = searchText?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            rows = rows.Where(row =>
                Contains(row.Title, search) ||
                Contains(row.LegacyWinner, search) ||
                Contains(row.AtlasWinner, search) ||
                Contains(row.ComparedVariants, search));
        }

        return rows
            .OrderBy(row => row.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string ToCsv(IEnumerable<AtlasComparisonRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var builder = new StringBuilder();
        builder.AppendLine("Title,Legacy winner,Atlas winner,Agreement,Atlas deciding rule,Legacy explanation");

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',',
            [
                Escape(row.Title),
                Escape(row.LegacyWinner),
                Escape(row.AtlasWinner),
                Escape(row.EnginesAgree ? "Agree" : "Disagree"),
                Escape(row.AtlasDecidingRule),
                Escape(row.LegacyExplanation)
            ]));
        }

        return builder.ToString();
    }

    private static bool Contains(string? value, string searchText) =>
        value?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true;

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var mustQuote = value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\r') ||
            value.Contains('\n');

        var escaped = value.Replace("\"", "\"\"");
        return mustQuote ? $"\"{escaped}\"" : escaped;
    }
}
