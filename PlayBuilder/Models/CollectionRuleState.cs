namespace PlayBuilder.Models;

public sealed class CollectionRuleState
{
    public List<string> SelectedSystemKeys { get; set; } = [];
    public string Workflow { get; set; } = string.Empty;
    public string ReleasePreference { get; set; } = string.Empty;
    public int ExcludedGameCount { get; set; }
    public int NeedsReviewCount { get; set; }
}
