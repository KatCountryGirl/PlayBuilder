using PlayBuilder.Models;
using PlayBuilder.Services.Atlas;

namespace PlayBuilder.Services;

/// <summary>Builds live 1G1R previews with the deterministic Atlas engine.</summary>
public sealed class AtlasCollectionRuleService : ICollectionRuleService
{
    private readonly AtlasCandidateFactory _candidateFactory;
    private readonly AtlasDecisionEngine _decisionEngine;

    public AtlasCollectionRuleService(AtlasCandidateFactory candidateFactory, AtlasDecisionEngine decisionEngine)
    {
        _candidateFactory = candidateFactory ?? throw new ArgumentNullException(nameof(candidateFactory));
        _decisionEngine = decisionEngine ?? throw new ArgumentNullException(nameof(decisionEngine));
    }

    public CollectionRulePreview BuildPreview(ArchiveScanResult scan, CollectionRuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(scan);
        ArgumentNullException.ThrowIfNull(options);

        var preview = new CollectionRulePreview { EngineName = "Atlas" };

        foreach (var group in scan.DuplicateGroups)
        {
            var candidates = _candidateFactory.CreateMany(group.Variants.Distinct(StringComparer.OrdinalIgnoreCase));
            if (options.Mode == OneGameOneRomMode.EnglishOnly &&
                !candidates.Any(candidate => candidate.Metadata.Languages.Contains("English", StringComparer.OrdinalIgnoreCase)))
            {
                preview.GroupsExcludedByLanguage++;
                continue;
            }

            if (candidates.Count == 0) continue;

            var decision = _decisionEngine.Evaluate(candidates, group.Title, options);
            var metadata = decision.Winner.Metadata;
            var fallback = metadata.Region.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
                metadata.PrimaryLanguage.Equals("Unknown", StringComparison.OrdinalIgnoreCase);

            preview.Selections.Add(new GameSelectionPreview
            {
                Title = group.Title,
                RecommendedVariant = metadata.FileName,
                RecommendedRegion = metadata.Region,
                RecommendedLanguage = metadata.PrimaryLanguage,
                IsFallback = fallback,
                Reason = BuildSummary(decision, options),
                DecisionReasons = decision.Reasons.Select(reason => $"{reason.Rule}: {reason.Description}").ToList(),
                Alternatives = decision.Candidates.Skip(1).Select(candidate => candidate.Metadata.FileName).ToList()
            });

            preview.DuplicateGroupsReviewed++;
            preview.AlternativesExcluded += Math.Max(0, decision.Candidates.Count - 1);
            if (fallback) preview.FallbackSelections++; else preview.ConfidentSelections++;
        }

        preview.Selections = preview.Selections
            .OrderBy(selection => selection.IsFallback)
            .ThenBy(selection => selection.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return preview;
    }

    private static string BuildSummary(AtlasDecision decision, CollectionRuleOptions options)
    {
        var prefix = options.Mode == OneGameOneRomMode.EnglishOnly
            ? "English-only mode"
            : "All-games mode";
        return $"{prefix} · {string.Join(" · ", decision.Reasons.Select(reason => reason.Description))}";
    }
}
