using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class AtlasExplanationTranslatorTests
{
    private readonly AtlasExplanationTranslator _translator = new();

    [Fact]
    public void ExplainRecommendation_UsesCollectorFacingLanguage()
    {
        var selection = new GameSelectionPreview
        {
            Title = "Sample Game",
            RecommendedLanguage = "English",
            RecommendedRegion = "USA",
            DecisionReasons = ["Selected by Region priority: USA wins."]
        };

        var explanation = _translator.ExplainRecommendation(selection, ReleasePreferencePreset.UsaFirst);

        Assert.Contains("I chose this release", explanation);
        Assert.Contains("includes English", explanation);
        Assert.Contains("USA preference", explanation);
        Assert.DoesNotContain("candidate", explanation, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("rule engine", explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExplainNeedsReview_UsesStoredReasonWhenAvailable()
    {
        var selection = new GameSelectionPreview
        {
            NeedsReviewReason = "I found more than one possible release, but the language information is incomplete."
        };

        var explanation = _translator.ExplainNeedsReview(selection);

        Assert.Equal(selection.NeedsReviewReason, explanation);
    }

    [Fact]
    public void ExplainAlternatives_ExplainsThatOtherFilesRemainInLibrary()
    {
        var selection = new GameSelectionPreview
        {
            Alternatives = ["Sample Game (Europe).zip"]
        };

        var alternatives = _translator.ExplainAlternatives(selection);

        var explanation = Assert.Single(alternatives);
        Assert.Contains("stays in your library", explanation);
        Assert.Contains("not part of this build plan", explanation);
    }
}
