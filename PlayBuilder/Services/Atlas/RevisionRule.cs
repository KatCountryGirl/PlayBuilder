namespace PlayBuilder.Services.Atlas.Rules;

public sealed class RevisionRule : IAtlasRule
{
    public string Name => "Revision";
    public int Priority => 400;

    public AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        if (!context.Options.PreferNewestRevision || left.Metadata.Revision == right.Metadata.Revision)
            return AtlasRuleResult.Tie("Revision does not distinguish these candidates.");

        var revision = Math.Max(left.Metadata.Revision, right.Metadata.Revision);
        var description = $"Revision {revision} is newer than the competing release.";
        return left.Metadata.Revision > right.Metadata.Revision ? AtlasRuleResult.PreferLeft(description) : AtlasRuleResult.PreferRight(description);
    }
}
