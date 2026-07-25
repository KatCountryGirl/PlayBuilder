namespace PlayBuilder.Services.Atlas;

/// <summary>Final, explainable result returned by the deterministic Atlas engine.</summary>
public sealed class AtlasDecision
{
    public required AtlasCandidate Winner { get; init; }
    public required IReadOnlyList<AtlasCandidate> Candidates { get; init; }
    public required AtlasReason DecidingReason { get; init; }
    public required IReadOnlyList<AtlasReason> SupportingReasons { get; init; }
    public IReadOnlyList<AtlasReason> Reasons => [DecidingReason, .. SupportingReasons];
    public AtlasCandidate? RunnerUp => Candidates.Skip(1).FirstOrDefault();
}
