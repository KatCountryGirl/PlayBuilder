namespace PlayBuilder.Services.Atlas;

/// <summary>Result of comparing two candidates with one deterministic rule.</summary>
public sealed record AtlasRuleResult(int Comparison, string Description)
{
    public static AtlasRuleResult Tie(string description) => new(0, description);
    public static AtlasRuleResult PreferLeft(string description) => new(-1, description);
    public static AtlasRuleResult PreferRight(string description) => new(1, description);
}
