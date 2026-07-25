namespace PlayBuilder.Services.Atlas.Rules;

public sealed class DumpQualityRule : IAtlasRule
{
    public string Name => "Dump quality";
    public int Priority => 50;

    public AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        var leftRank = Rank(left.Metadata);
        var rightRank = Rank(right.Metadata);
        if (leftRank == rightRank) return AtlasRuleResult.Tie("Both candidates have equivalent dump-quality evidence.");

        var preferred = leftRank < rightRank ? left : right;
        var description = preferred.Metadata.IsVerifiedDump
            ? "A verified good dump is preferred."
            : "A candidate without a known bad-dump marker is preferred.";
        return leftRank < rightRank ? AtlasRuleResult.PreferLeft(description) : AtlasRuleResult.PreferRight(description);
    }

    private static int Rank(FilenameMetadata metadata) => metadata.IsBadDump ? 2 : metadata.IsVerifiedDump ? 0 : 1;
}
