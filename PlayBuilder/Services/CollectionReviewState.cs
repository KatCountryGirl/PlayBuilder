using PlayBuilder.Models;

namespace PlayBuilder.Services;

public enum CollectionReviewSummaryFilter
{
    All,
    Confident,
    NeedsReview,
    ExtraVersions
}

public sealed class CollectionReviewFilters
{
    public string SearchText { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool SelectedOnly { get; set; }
    public bool ExcludedOnly { get; set; }
    public bool NeedsReviewOnly { get; set; }
}

public sealed record CollectionReviewAlternate(
    string Title,
    string SelectedWinner,
    string AlternateFilename,
    string Reason,
    GameSelectionPreview Selection);

public sealed class CollectionReviewState
{
    private readonly HashSet<string> _selected = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<GameSelectionPreview> _recommendations = [];

    public IReadOnlySet<string> SelectedVariants => _selected;
    public int SelectedCount => _recommendations.Count(item => IsSelected(item));
    public int ExcludedCount => _recommendations.Count - SelectedCount;

    public void LoadRecommendations(IReadOnlyList<GameSelectionPreview> recommendations)
    {
        ArgumentNullException.ThrowIfNull(recommendations);
        _recommendations = recommendations;
        ResetToAtlasRecommendations();
    }

    public bool IsSelected(GameSelectionPreview selection) =>
        _selected.Contains(selection.RecommendedVariant);

    public void SetSelected(GameSelectionPreview selection, bool isSelected)
    {
        if (isSelected)
        {
            _selected.Add(selection.RecommendedVariant);
        }
        else
        {
            _selected.Remove(selection.RecommendedVariant);
        }
    }

    public void SelectAll()
    {
        _selected.Clear();
        foreach (var selection in _recommendations)
        {
            _selected.Add(selection.RecommendedVariant);
        }
    }

    public void SelectNone() => _selected.Clear();

    public void InvertSelection()
    {
        var next = _recommendations
            .Where(selection => !IsSelected(selection))
            .Select(selection => selection.RecommendedVariant)
            .ToList();

        _selected.Clear();
        foreach (var variant in next)
        {
            _selected.Add(variant);
        }
    }

    public void ResetToAtlasRecommendations() => SelectAll();

    public IReadOnlyList<string> GetSelectedFilenames() =>
        _recommendations
            .Where(IsSelected)
            .Select(selection => selection.RecommendedVariant)
            .ToList();

    public IReadOnlyList<GameSelectionPreview> Filter(
        CollectionReviewFilters filters,
        CollectionReviewSummaryFilter summaryFilter)
    {
        ArgumentNullException.ThrowIfNull(filters);

        var rows = _recommendations.AsEnumerable();
        rows = summaryFilter switch
        {
            CollectionReviewSummaryFilter.Confident => rows.Where(selection => !selection.IsFallback),
            CollectionReviewSummaryFilter.NeedsReview => rows.Where(selection => selection.IsFallback),
            _ => rows
        };

        if (filters.NeedsReviewOnly)
        {
            rows = rows.Where(selection => selection.IsFallback);
        }

        if (filters.SelectedOnly)
        {
            rows = rows.Where(IsSelected);
        }

        if (filters.ExcludedOnly)
        {
            rows = rows.Where(selection => !IsSelected(selection));
        }

        if (!string.IsNullOrWhiteSpace(filters.Language))
        {
            rows = rows.Where(selection =>
                selection.RecommendedLanguage.Equals(filters.Language, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filters.Region))
        {
            rows = rows.Where(selection =>
                selection.RecommendedRegion.Equals(filters.Region, StringComparison.OrdinalIgnoreCase));
        }

        var search = filters.SearchText.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            rows = rows.Where(selection =>
                Contains(selection.Title, search) ||
                Contains(selection.RecommendedVariant, search) ||
                selection.Alternatives.Any(alternative => Contains(alternative, search)));
        }

        return rows
            .OrderBy(selection => selection.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<CollectionReviewAlternate> GetAlternates(CollectionReviewFilters filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        return Filter(filters, CollectionReviewSummaryFilter.All)
            .SelectMany(selection => selection.Alternatives.Select(alternative =>
                new CollectionReviewAlternate(
                    selection.Title,
                    selection.RecommendedVariant,
                    alternative,
                    BuildAlternateReason(selection),
                    selection)))
            .OrderBy(row => row.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.AlternateFilename, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string BuildAlternateReason(GameSelectionPreview selection) =>
        selection.DecisionReasons.FirstOrDefault() ??
        selection.Reason;

    private static bool Contains(string value, string search) =>
        value.Contains(search, StringComparison.OrdinalIgnoreCase);
}
