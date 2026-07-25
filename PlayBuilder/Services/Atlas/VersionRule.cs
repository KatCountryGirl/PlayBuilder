namespace PlayBuilder.Services.Atlas.Rules;

public sealed class VersionRule : IAtlasRule
{
    public string Name => "Version";
    public int Priority => 500;

    public AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        if (!context.Options.PreferNewestRevision) return AtlasRuleResult.Tie("Version preference is disabled.");
        var comparison = CompareVersions(left.Metadata.Version, right.Metadata.Version);
        if (comparison == 0) return AtlasRuleResult.Tie("Version does not distinguish these candidates.");

        var version = comparison > 0 ? left.Metadata.Version : right.Metadata.Version;
        var description = $"Version {version} is newer than the competing release.";
        return comparison > 0 ? AtlasRuleResult.PreferLeft(description) : AtlasRuleResult.PreferRight(description);
    }

    private static int CompareVersions(Version? left, Version? right)
    {
        if (left is null && right is null) return 0;
        if (left is null) return -1;
        if (right is null) return 1;
        return left.CompareTo(right);
    }
}
