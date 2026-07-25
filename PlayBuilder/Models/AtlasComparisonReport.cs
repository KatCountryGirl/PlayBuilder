namespace PlayBuilder.Models;

public sealed class AtlasComparisonReport
{
    public string LegacyEngineName { get; init; } = "Legacy";
    public string AtlasEngineName { get; init; } = "Atlas";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public List<AtlasComparisonRow> Rows { get; init; } = [];

    public int ComparedGroupCount => Rows.Count;
    public int AgreementCount => Rows.Count(row => row.EnginesAgree);
    public int DifferenceCount => Rows.Count - AgreementCount;
    public int EnglishOnlyExcludedCount => Rows.Count(row =>
        row.LegacyWinner is null &&
        row.AtlasWinner is null &&
        string.IsNullOrWhiteSpace(row.AtlasDecidingRule));
    public double AgreementPercentage => ComparedGroupCount == 0
        ? 0
        : AgreementCount * 100d / ComparedGroupCount;
}

public sealed class AtlasComparisonRow
{
    public string Title { get; init; } = string.Empty;
    public string ComparedVariants { get; init; } = string.Empty;
    public string? LegacyWinner { get; init; }
    public string? AtlasWinner { get; init; }
    public bool EnginesAgree { get; init; }
    public string AtlasDecidingRule { get; init; } = string.Empty;
    public string LegacyExplanation { get; init; } = string.Empty;
}
