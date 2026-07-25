using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class AtlasComparisonReportFormatterTests
{
    [Fact]
    public void ReportSummary_CalculatesTotalsAndEnglishOnlyExclusions()
    {
        var report = CreateReport(
            Row("Agree", "A (USA).zip", "A (USA).zip", true, "Language priority", "legacy"),
            Row("Disagree", "B (USA).zip", "B (Japan).zip", false, "Dump quality", "legacy"),
            Row("Excluded", null, null, true, string.Empty, string.Empty));

        Assert.Equal(3, report.ComparedGroupCount);
        Assert.Equal(2, report.AgreementCount);
        Assert.Equal(1, report.DifferenceCount);
        Assert.Equal(1, report.EnglishOnlyExcludedCount);
        Assert.Equal(66.667, report.AgreementPercentage, precision: 3);
    }

    [Fact]
    public void FilterRows_ReturnsAgreementsOnly()
    {
        var report = CreateReport(
            Row("Agree", "A.zip", "A.zip", true),
            Row("Disagree", "B.zip", "C.zip", false));

        var rows = AtlasComparisonReportFormatter.FilterRows(
            report,
            AtlasComparisonFilter.AgreementsOnly,
            null);

        var row = Assert.Single(rows);
        Assert.Equal("Agree", row.Title);
    }

    [Fact]
    public void FilterRows_ReturnsDisagreementsOnly()
    {
        var report = CreateReport(
            Row("Agree", "A.zip", "A.zip", true),
            Row("Disagree", "B.zip", "C.zip", false));

        var rows = AtlasComparisonReportFormatter.FilterRows(
            report,
            AtlasComparisonFilter.DisagreementsOnly,
            null);

        var row = Assert.Single(rows);
        Assert.Equal("Disagree", row.Title);
    }

    [Fact]
    public void FilterRows_SearchesTitleWinnerAndComparedFilenames()
    {
        var report = CreateReport(
            Row("Alpha Game", "Alpha (USA).zip", "Alpha (USA).zip", true, variants: "Alpha (USA).zip | Alpha (Europe).zip"),
            Row("Beta Game", "Beta (USA).zip", "Beta (Japan).zip", false, variants: "Beta (USA).zip | Beta Prototype.zip"));

        var titleRows = AtlasComparisonReportFormatter.FilterRows(report, AtlasComparisonFilter.All, "alpha");
        var filenameRows = AtlasComparisonReportFormatter.FilterRows(report, AtlasComparisonFilter.All, "Prototype");

        Assert.Equal("Alpha Game", Assert.Single(titleRows).Title);
        Assert.Equal("Beta Game", Assert.Single(filenameRows).Title);
    }

    [Fact]
    public void ToCsv_UsesRequiredColumnOrderAndEscapesValues()
    {
        var rows = new[]
        {
            Row(
                "Comma, Game",
                "Legacy \"Winner\".zip",
                "Atlas Winner.zip",
                false,
                "Dump quality",
                "Legacy, explanation")
        };

        var csv = AtlasComparisonReportFormatter.ToCsv(rows);

        Assert.StartsWith("Title,Legacy winner,Atlas winner,Agreement,Atlas deciding rule,Legacy explanation", csv);
        Assert.Contains("\"Comma, Game\",\"Legacy \"\"Winner\"\".zip\",Atlas Winner.zip,Disagree,Dump quality,\"Legacy, explanation\"", csv);
    }

    [Fact]
    public void EmptyReport_ReturnsEmptyRowsAndHeaderOnlyCsv()
    {
        var report = CreateReport();

        var rows = AtlasComparisonReportFormatter.FilterRows(report, AtlasComparisonFilter.All, "anything");
        var csv = AtlasComparisonReportFormatter.ToCsv(rows);

        Assert.Empty(rows);
        Assert.Equal(0, report.ComparedGroupCount);
        Assert.Equal(0, report.AgreementPercentage);
        Assert.Equal("Title,Legacy winner,Atlas winner,Agreement,Atlas deciding rule,Legacy explanation\r\n", csv);
    }

    private static AtlasComparisonReport CreateReport(params AtlasComparisonRow[] rows)
    {
        var report = new AtlasComparisonReport();
        report.Rows.AddRange(rows);
        return report;
    }

    private static AtlasComparisonRow Row(
        string title,
        string? legacyWinner,
        string? atlasWinner,
        bool agrees,
        string decidingRule = "Language priority",
        string legacyExplanation = "Legacy reason",
        string variants = "")
    {
        return new AtlasComparisonRow
        {
            Title = title,
            ComparedVariants = variants,
            LegacyWinner = legacyWinner,
            AtlasWinner = atlasWinner,
            EnginesAgree = agrees,
            AtlasDecidingRule = decidingRule,
            LegacyExplanation = legacyExplanation
        };
    }
}
