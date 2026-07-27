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
        var groups = GetOneGameOneRomGroups(scan);
        preview.Diagnostics = BuildInitialDiagnostics(scan, groups);

        foreach (var group in groups)
        {
            var candidates = _candidateFactory.CreateMany(group.Variants.Distinct(StringComparer.OrdinalIgnoreCase));
            if (options.Mode == OneGameOneRomMode.EnglishOnly &&
                !candidates.Any(candidate => candidate.Metadata.Languages.Contains("English", StringComparer.OrdinalIgnoreCase)))
            {
                preview.GroupsExcludedByLanguage++;
                preview.Diagnostics.GroupsExcludedByEnglishOnlyMode++;
                continue;
            }

            if (candidates.Count == 0)
            {
                preview.Diagnostics.GroupsRejectedBeforeAtlas++;
                continue;
            }

            var decision = _decisionEngine.Evaluate(candidates, group.Title, options);
            var metadata = decision.Winner.Metadata;
            var fallback = metadata.Region.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
                metadata.PrimaryLanguage.Equals("Unknown", StringComparison.OrdinalIgnoreCase);

            preview.Selections.Add(new GameSelectionPreview
            {
                Title = group.Title,
                System = group.System,
                SystemKey = group.SystemKey,
                RecommendedVariant = metadata.FileName,
                RecommendedRegion = metadata.Region,
                RecommendedLanguage = metadata.PrimaryLanguage,
                IsFallback = fallback,
                Reason = BuildSummary(decision, options),
                DecisionReasons = BuildDecisionReasons(decision),
                Alternatives = decision.Candidates.Skip(1).Select(candidate => candidate.Metadata.FileName).ToList(),
                AtlasInspection = BuildInspection(decision)
            });

            preview.DuplicateGroupsReviewed++;
            preview.AlternativesExcluded += Math.Max(0, decision.Candidates.Count - 1);
            if (fallback) preview.FallbackSelections++; else preview.ConfidentSelections++;
        }

        preview.Diagnostics.FinalRecommendations = preview.Selections.Count;
        preview.Selections = preview.Selections
            .OrderBy(selection => selection.IsFallback)
            .ThenBy(selection => selection.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return preview;
    }

    private static IReadOnlyList<DuplicateGroupSummary> GetOneGameOneRomGroups(ArchiveScanResult scan) =>
        scan.OneGameOneRomGroups.Count > 0 ? scan.OneGameOneRomGroups : scan.DuplicateGroups;

    private static CollectionRuleDiagnostics BuildInitialDiagnostics(
        ArchiveScanResult scan,
        IReadOnlyList<DuplicateGroupSummary> groups)
    {
        var validFilenames = groups.Sum(group => group.FileCount);
        return new CollectionRuleDiagnostics
        {
            TotalRomsLoaded = scan.RecognizedFileCount,
            ValidFilenames = validFilenames,
            NormalizedTitles = groups.Count,
            UniqueTitleGroups = groups.Count,
            SingleRomGroups = groups.Count(group => group.FileCount == 1),
            MultiRomGroups = groups.Count(group => group.FileCount > 1)
        };
    }

    private static string BuildSummary(AtlasDecision decision, CollectionRuleOptions options)
    {
        var prefix = options.Mode == OneGameOneRomMode.EnglishOnly
            ? "English-only mode"
            : "All-games mode";

        var summary = $"{prefix} · Selected by {decision.DecidingReason.Rule}: {decision.DecidingReason.Description}";
        if (decision.SupportingReasons.Count == 0)
        {
            return summary;
        }

        return $"{summary} · Supporting: {string.Join(" · ", decision.SupportingReasons.Select(reason => reason.Description))}";
    }

    private static List<string> BuildDecisionReasons(AtlasDecision decision)
    {
        var reasons = new List<string>
        {
            $"Selected by {decision.DecidingReason.Rule}: {decision.DecidingReason.Description}"
        };

        reasons.AddRange(decision.SupportingReasons.Select(
            reason => $"Supporting match - {reason.Rule}: {reason.Description}"));

        return reasons;
    }

    private static AtlasInspectionPreview BuildInspection(AtlasDecision decision)
    {
        var runnerUp = decision.RunnerUp;
        return new AtlasInspectionPreview
        {
            WinningRom = decision.Winner.Metadata.FileName,
            RunnerUp = runnerUp?.Metadata.FileName ?? "No runner-up",
            DecidingRule = decision.DecidingReason.Rule,
            DecidingRuleDescription = decision.DecidingReason.Description,
            SupportingRules = decision.SupportingReasons
                .Select(reason => $"{reason.Rule}: {reason.Description}")
                .ToList(),
            Candidates = decision.Candidates
                .Select((candidate, index) => BuildCandidateInspection(
                    candidate,
                    index + 1,
                    ReferenceEquals(candidate, decision.Winner),
                    runnerUp is not null && ReferenceEquals(candidate, runnerUp)))
                .ToList()
        };
    }

    private static AtlasCandidateInspectionPreview BuildCandidateInspection(
        AtlasCandidate candidate,
        int order,
        bool isWinner,
        bool isRunnerUp)
    {
        var metadata = candidate.Metadata;
        return new AtlasCandidateInspectionPreview
        {
            Order = order,
            FileName = metadata.FileName,
            IsWinner = isWinner,
            IsRunnerUp = isRunnerUp,
            Region = metadata.Region,
            Languages = metadata.Languages.Count == 0 ? ["Unknown"] : metadata.Languages.ToList(),
            DumpQuality = GetDumpQuality(metadata),
            Revision = metadata.Revision > 0 ? $"Rev {metadata.Revision}" : "Original",
            Version = metadata.Version?.ToString() ?? "None",
            ReleaseType = GetReleaseType(metadata)
        };
    }

    private static string GetDumpQuality(FilenameMetadata metadata)
    {
        if (metadata.IsBadDump)
        {
            return "Known bad dump";
        }

        return metadata.IsVerifiedDump
            ? "Verified good dump"
            : "Neutral";
    }

    private static string GetReleaseType(FilenameMetadata metadata)
    {
        var types = new List<string>();
        if (metadata.IsBeta) types.Add("Beta");
        if (metadata.IsPrototype) types.Add("Prototype");
        if (metadata.IsDemo) types.Add("Demo");
        if (metadata.IsSample) types.Add("Sample");
        if (metadata.IsHack) types.Add("Hack");
        if (metadata.IsTranslation) types.Add("Translation");
        if (metadata.IsHomebrew) types.Add("Homebrew");
        if (metadata.IsUnlicensed) types.Add("Unlicensed");
        if (metadata.IsPirate) types.Add("Pirate");

        return types.Count == 0
            ? "Standard retail"
            : string.Join(", ", types);
    }
}
