using PlayBuilder.Models;
using PlayBuilder.Services.Atlas;

namespace PlayBuilder.Services;

/// <summary>Compares legacy and Atlas collection decisions without changing live user behavior.</summary>
public sealed class AtlasComparisonService : IAtlasComparisonService
{
    private readonly CollectionRuleService _legacyService;
    private readonly AtlasCandidateFactory _candidateFactory;
    private readonly AtlasDecisionEngine _decisionEngine;

    public AtlasComparisonService(
        CollectionRuleService legacyService,
        AtlasCandidateFactory candidateFactory,
        AtlasDecisionEngine decisionEngine)
    {
        _legacyService = legacyService ?? throw new ArgumentNullException(nameof(legacyService));
        _candidateFactory = candidateFactory ?? throw new ArgumentNullException(nameof(candidateFactory));
        _decisionEngine = decisionEngine ?? throw new ArgumentNullException(nameof(decisionEngine));
    }

    public AtlasComparisonReport Compare(ArchiveScanResult scan, CollectionRuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(scan);
        ArgumentNullException.ThrowIfNull(options);

        var legacyPreview = _legacyService.BuildPreview(scan, options);
        var legacySelections = legacyPreview.Selections
            .GroupBy(selection => selection.Title, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var report = new AtlasComparisonReport();
        foreach (var group in GetOneGameOneRomGroups(scan))
        {
            legacySelections.TryGetValue(group.Title, out var legacySelection);
            var atlasDecision = EvaluateAtlas(group, options);
            var atlasWinner = atlasDecision?.Winner.Metadata.FileName;
            var legacyWinner = legacySelection?.RecommendedVariant;

            report.Rows.Add(new AtlasComparisonRow
            {
                Title = group.Title,
                ComparedVariants = string.Join(" | ", group.Variants.Distinct(StringComparer.OrdinalIgnoreCase)),
                LegacyWinner = legacyWinner,
                AtlasWinner = atlasWinner,
                EnginesAgree = string.Equals(legacyWinner, atlasWinner, StringComparison.OrdinalIgnoreCase),
                AtlasDecidingRule = atlasDecision?.DecidingReason.Rule ?? string.Empty,
                LegacyExplanation = legacySelection?.Reason ?? string.Empty
            });
        }

        return report;
    }

    private static IReadOnlyList<DuplicateGroupSummary> GetOneGameOneRomGroups(ArchiveScanResult scan) =>
        scan.OneGameOneRomGroups.Count > 0 ? scan.OneGameOneRomGroups : scan.DuplicateGroups;

    private AtlasDecision? EvaluateAtlas(DuplicateGroupSummary group, CollectionRuleOptions options)
    {
        var candidates = _candidateFactory.CreateMany(group.Variants.Distinct(StringComparer.OrdinalIgnoreCase));
        if (options.Mode == OneGameOneRomMode.EnglishOnly &&
            !candidates.Any(candidate => candidate.Metadata.Languages.Contains("English", StringComparer.OrdinalIgnoreCase)))
        {
            return null;
        }

        return candidates.Count == 0
            ? null
            : _decisionEngine.Evaluate(candidates, group.Title, options);
    }
}
