namespace PlayBuilder.Services.Atlas;

/// <summary>Explains one deterministic rule outcome in an Atlas decision.</summary>
public sealed record AtlasReason(string Rule, string Description);
