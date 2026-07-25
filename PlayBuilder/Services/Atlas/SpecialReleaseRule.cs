namespace PlayBuilder.Services.Atlas.Rules;

public sealed class SpecialReleaseRule : IAtlasRule
{
    public string Name => "Release type";
    public int Priority => 300;

    public AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        if (!context.Options.AvoidSpecialReleases || left.Metadata.IsSpecialRelease == right.Metadata.IsSpecialRelease)
            return AtlasRuleResult.Tie("Release type does not distinguish these candidates.");

        const string description = "A standard retail release is preferred over beta, demo, prototype, hack, translation, homebrew, unlicensed, or pirate variants.";
        return !left.Metadata.IsSpecialRelease ? AtlasRuleResult.PreferLeft(description) : AtlasRuleResult.PreferRight(description);
    }
}
