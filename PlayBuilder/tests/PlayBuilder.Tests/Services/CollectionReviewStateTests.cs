using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class CollectionReviewStateTests
{
    [Fact]
    public void LoadRecommendations_SelectsEveryGameByDefault()
    {
        var state = CreateState();

        Assert.Equal(3, state.SelectedCount);
        Assert.Equal(0, state.ExcludedCount);
    }

    [Fact]
    public void SelectionActions_SelectNoneSelectAllInvertAndReset()
    {
        var state = CreateState();

        state.SelectNone();
        Assert.Equal(0, state.SelectedCount);

        state.SelectAll();
        Assert.Equal(3, state.SelectedCount);

        state.SetSelected(Recommendations[0], false);
        state.InvertSelection();
        Assert.Single(state.GetSelectedFilenames());
        Assert.Contains("Alpha (USA).zip", state.GetSelectedFilenames());

        state.ResetToAtlasRecommendations();
        Assert.Equal(3, state.SelectedCount);
    }

    [Fact]
    public void SelectionState_SurvivesFiltering()
    {
        var state = CreateState();
        state.SetSelected(Recommendations[1], false);

        var filtered = state.Filter(new CollectionReviewFilters { SearchText = "Gamma" }, CollectionReviewSummaryFilter.All);
        var allRows = state.Filter(new CollectionReviewFilters(), CollectionReviewSummaryFilter.All);

        Assert.Single(filtered);
        Assert.Equal(2, state.SelectedCount);
        Assert.Contains(allRows, row => row.RecommendedVariant == "Beta (Japan).zip" && !state.IsSelected(row));
    }

    [Fact]
    public void Filter_ReturnsSummarySearchLanguageRegionAndSelectionMatches()
    {
        var state = CreateState();
        state.SetSelected(Recommendations[1], false);

        Assert.Equal(2, state.Filter(new CollectionReviewFilters(), CollectionReviewSummaryFilter.Confident).Count);
        Assert.Single(state.Filter(new CollectionReviewFilters(), CollectionReviewSummaryFilter.NeedsReview));
        Assert.Single(state.Filter(new CollectionReviewFilters { SearchText = "Alt" }, CollectionReviewSummaryFilter.All));
        Assert.Single(state.Filter(new CollectionReviewFilters { Language = "Japanese" }, CollectionReviewSummaryFilter.All));
        Assert.Equal(2, state.Filter(new CollectionReviewFilters { Region = "USA" }, CollectionReviewSummaryFilter.All).Count);
        Assert.Equal(2, state.Filter(new CollectionReviewFilters { SelectedOnly = true }, CollectionReviewSummaryFilter.All).Count);
        Assert.Single(state.Filter(new CollectionReviewFilters { ExcludedOnly = true }, CollectionReviewSummaryFilter.All));
        Assert.Single(state.Filter(new CollectionReviewFilters { NeedsReviewOnly = true }, CollectionReviewSummaryFilter.All));
    }

    [Fact]
    public void GetAlternates_ReturnsExtraVersionDisplayData()
    {
        var state = CreateState();

        var alternate = Assert.Single(state.GetAlternates(new CollectionReviewFilters { SearchText = "Alt" }));

        Assert.Equal("Gamma", alternate.Title);
        Assert.Equal("Gamma (USA).zip", alternate.SelectedWinner);
        Assert.Equal("Gamma Alt (USA).zip", alternate.AlternateFilename);
        Assert.Contains("Selected by", alternate.Reason);
    }

    private static readonly GameSelectionPreview[] Recommendations =
    [
        new()
        {
            Title = "Alpha",
            RecommendedVariant = "Alpha (USA).zip",
            RecommendedLanguage = "English",
            RecommendedRegion = "USA",
            Reason = "Selected by language",
            DecisionReasons = ["Selected by Language priority: English wins."]
        },
        new()
        {
            Title = "Beta",
            RecommendedVariant = "Beta (Japan).zip",
            RecommendedLanguage = "Japanese",
            RecommendedRegion = "Japan",
            Reason = "Unknown language or region",
            IsFallback = true,
            DecisionReasons = ["Selected by Stable tie-breaker: Metadata is unclear."]
        },
        new()
        {
            Title = "Gamma",
            RecommendedVariant = "Gamma (USA).zip",
            RecommendedLanguage = "English",
            RecommendedRegion = "USA",
            Reason = "Selected by region",
            DecisionReasons = ["Selected by Region priority: USA wins."],
            Alternatives = ["Gamma Alt (USA).zip"]
        }
    ];

    private static CollectionReviewState CreateState()
    {
        var state = new CollectionReviewState();
        state.LoadRecommendations(Recommendations);
        return state;
    }
}
