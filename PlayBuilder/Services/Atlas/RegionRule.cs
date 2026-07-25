namespace PlayBuilder.Services.Atlas.Rules;

public sealed class RegionRule : IAtlasRule
{
    public string Name => "Region priority";
    public int Priority => 200;

    public AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        var leftIndex = IndexOf(context.Options.RegionPriority, left.Metadata.Region);
        var rightIndex = IndexOf(context.Options.RegionPriority, right.Metadata.Region);
        if (leftIndex == rightIndex) return AtlasRuleResult.Tie("Both candidates have the same region priority.");

        var region = leftIndex < rightIndex ? left.Metadata.Region : right.Metadata.Region;
        var description = $"{region} is the highest configured region preference available within the preferred language.";
        return leftIndex < rightIndex ? AtlasRuleResult.PreferLeft(description) : AtlasRuleResult.PreferRight(description);
    }

    private static int IndexOf(IReadOnlyList<string> priority, string value)
    {
        for (var i = 0; i < priority.Count; i++)
            if (priority[i].Equals(value, StringComparison.OrdinalIgnoreCase)) return i;
        return priority.Count + 1;
    }
}
