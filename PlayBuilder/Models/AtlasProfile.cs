namespace PlayBuilder.Models;

public sealed class AtlasProfile
{
    public static readonly string[] DefaultRuleNames =
    [
        "Dump quality",
        "Language priority",
        "Region priority",
        "Release type",
        "Revision",
        "Version"
    ];

    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Personal";
    public Dictionary<string, bool> RuleEnabled { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> RegionPriority { get; set; } = [];
    public List<string> LanguagePriority { get; set; } = [];
    public List<string> ReleaseTypePriority { get; set; } = [];
    public List<string> DumpQualityPriority { get; set; } = [];
    public bool PreferNewestRevision { get; set; } = true;
    public bool PreferNewestVersion { get; set; } = true;

    public CollectionRuleOptions ToOptions(OneGameOneRomMode mode)
    {
        var enabledRules = DefaultRuleNames
            .Where(IsRuleEnabled)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new CollectionRuleOptions
        {
            Mode = mode,
            RegionPriority = RegionPriority.Count == 0 ? new CollectionRuleOptions().RegionPriority : RegionPriority.ToList(),
            LanguagePriority = LanguagePriority.Count == 0 ? new CollectionRuleOptions().LanguagePriority : LanguagePriority.ToList(),
            AvoidSpecialReleases = IsRuleEnabled("Release type"),
            PreferNewestRevision = PreferNewestRevision && IsRuleEnabled("Revision"),
            PreferNewestVersion = PreferNewestVersion && IsRuleEnabled("Version"),
            EnabledRuleNames = enabledRules
        };
    }

    public void ApplyOptions(CollectionRuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RegionPriority = options.RegionPriority.ToList();
        LanguagePriority = options.LanguagePriority.ToList();
        PreferNewestRevision = options.PreferNewestRevision;
        PreferNewestVersion = options.PreferNewestVersion;
        foreach (var ruleName in DefaultRuleNames)
        {
            SetRuleEnabled(ruleName, options.EnabledRuleNames.Contains(ruleName));
        }

        SetRuleEnabled("Release type", options.AvoidSpecialReleases);
    }

    public bool IsRuleEnabled(string ruleName) =>
        !RuleEnabled.TryGetValue(ruleName, out var enabled) || enabled;

    public void SetRuleEnabled(string ruleName, bool enabled) =>
        RuleEnabled[ruleName] = enabled;
}

public sealed class AtlasProfileStore
{
    public string ActiveProfileId { get; set; } = string.Empty;
    public List<AtlasProfile> Profiles { get; set; } = [];

    public AtlasProfile ActiveProfile =>
        Profiles.FirstOrDefault(profile => profile.Id.Equals(ActiveProfileId, StringComparison.OrdinalIgnoreCase)) ??
        Profiles.First();
}
