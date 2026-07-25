namespace PlayBuilder.Services.Atlas;

/// <summary>One ordered, deterministic Atlas comparison rule.</summary>
public interface IAtlasRule
{
    string Name { get; }
    int Priority { get; }
    AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context);
}
