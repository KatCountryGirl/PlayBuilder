using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class AtlasExplanationTranslator
{
    public string ExplainRecommendation(GameSelectionPreview selection, ReleasePreferencePreset preset)
    {
        ArgumentNullException.ThrowIfNull(selection);

        var reasons = BuildCollectorReasons(selection, preset).ToList();
        return reasons.Count == 0
            ? $"This is the version I'd recommend for {selection.Title}."
            : $"I chose this release because {JoinReasons(reasons)}.";
    }

    public string ExplainNeedsReview(GameSelectionPreview selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (!string.IsNullOrWhiteSpace(selection.NeedsReviewReason))
        {
            return selection.NeedsReviewReason;
        }

        return "I'd like your opinion before this game is added to the build plan.";
    }

    public IReadOnlyList<string> ExplainAlternatives(GameSelectionPreview selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (selection.Alternatives.Count == 0)
        {
            return ["I did not find another version competing with this release."];
        }

        return selection.Alternatives
            .Select(alternative => $"{alternative} stays in your library, but it is not part of this build plan.")
            .ToList();
    }

    private static IEnumerable<string> BuildCollectorReasons(
        GameSelectionPreview selection,
        ReleasePreferencePreset preset)
    {
        if (selection.RecommendedLanguage.Equals("English", StringComparison.OrdinalIgnoreCase))
        {
            yield return "it includes English";
        }
        else if (!selection.RecommendedLanguage.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            yield return $"it is the best {selection.RecommendedLanguage} match I found";
        }

        if (!selection.RecommendedRegion.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            yield return preset switch
            {
                ReleasePreferencePreset.UsaFirst when selection.RecommendedRegion.Equals("USA", StringComparison.OrdinalIgnoreCase) => "it matches your USA preference",
                ReleasePreferencePreset.EuropeFirst when selection.RecommendedRegion.Equals("Europe", StringComparison.OrdinalIgnoreCase) => "it matches your Europe preference",
                ReleasePreferencePreset.JapaneseFirst when selection.RecommendedRegion.Equals("Japan", StringComparison.OrdinalIgnoreCase) => "it matches your Japan preference",
                _ => $"the {selection.RecommendedRegion} release is the best fit for your current preference"
            };
        }

        if (selection.DecisionReasons.Any(reason => reason.Contains("revision", StringComparison.OrdinalIgnoreCase)))
        {
            yield return "it appears to be the better revision";
        }

        if (selection.DecisionReasons.Any(reason => reason.Contains("version", StringComparison.OrdinalIgnoreCase)))
        {
            yield return "it appears to be the newer version";
        }

        if (selection.DecisionReasons.Any(reason => reason.Contains("dump", StringComparison.OrdinalIgnoreCase)))
        {
            yield return "it has better dump-quality information";
        }
    }

    private static string JoinReasons(IReadOnlyList<string> reasons)
    {
        return reasons.Count switch
        {
            0 => string.Empty,
            1 => reasons[0],
            2 => $"{reasons[0]} and {reasons[1]}",
            _ => $"{string.Join(", ", reasons.Take(reasons.Count - 1))}, and {reasons[^1]}"
        };
    }
}
