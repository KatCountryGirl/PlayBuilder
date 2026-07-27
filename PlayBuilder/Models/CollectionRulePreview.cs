namespace PlayBuilder.Models;

public enum OneGameOneRomMode
{
    AllGames,
    EnglishOnly
}

public sealed class CollectionRuleOptions
{
    public OneGameOneRomMode Mode { get; set; } = OneGameOneRomMode.AllGames;

    public List<string> RegionPriority { get; set; } =
    [
        "USA", "World", "Europe", "Australia", "United Kingdom", "Canada",
        "Japan", "Korea", "Brazil", "France", "Germany", "Spain", "Italy", "Unknown"
    ];

    public List<string> LanguagePriority { get; set; } =
    [
        "English", "Japanese", "French", "German", "Spanish", "Italian",
        "Portuguese", "Korean", "Dutch", "Swedish", "Unknown"
    ];

    public bool AvoidSpecialReleases { get; set; } = true;
    public bool PreferNewestRevision { get; set; } = true;
    public bool PreferNewestVersion { get; set; } = true;
    public HashSet<string> EnabledRuleNames { get; set; } =
    [
        "Dump quality",
        "Language priority",
        "Region priority",
        "Release type",
        "Revision",
        "Version"
    ];
}

public sealed class CollectionRulePreview
{
    public string EngineName { get; set; } = string.Empty;
    public int DuplicateGroupsReviewed { get; set; }
    public int ConfidentSelections { get; set; }
    public int FallbackSelections { get; set; }
    public int AlternativesExcluded { get; set; }
    public int GroupsExcludedByLanguage { get; set; }
    public CollectionRuleDiagnostics Diagnostics { get; set; } = new();
    public List<GameSelectionPreview> Selections { get; set; } = [];
}

public sealed class CollectionRuleDiagnostics
{
    public long TotalRomsLoaded { get; set; }
    public long ValidFilenames { get; set; }
    public int NormalizedTitles { get; set; }
    public int UniqueTitleGroups { get; set; }
    public int SingleRomGroups { get; set; }
    public int MultiRomGroups { get; set; }
    public int GroupsRejectedBeforeAtlas { get; set; }
    public int GroupsExcludedByEnglishOnlyMode { get; set; }
    public int FinalRecommendations { get; set; }
}

public sealed class GameSelectionPreview
{
    public string Title { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public string SystemKey { get; set; } = string.Empty;
    public string RecommendedVariant { get; set; } = string.Empty;
    public string RecommendedRegion { get; set; } = "Unknown";
    public string RecommendedLanguage { get; set; } = "Unknown";
    public string Reason { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
    public List<string> DecisionReasons { get; set; } = [];
    public List<string> Alternatives { get; set; } = [];
    public AtlasInspectionPreview? AtlasInspection { get; set; }
}

public sealed class AtlasInspectionPreview
{
    public string WinningRom { get; set; } = string.Empty;
    public string RunnerUp { get; set; } = string.Empty;
    public string DecidingRule { get; set; } = string.Empty;
    public string DecidingRuleDescription { get; set; } = string.Empty;
    public List<string> SupportingRules { get; set; } = [];
    public List<AtlasCandidateInspectionPreview> Candidates { get; set; } = [];
}

public sealed class AtlasCandidateInspectionPreview
{
    public int Order { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsWinner { get; set; }
    public bool IsRunnerUp { get; set; }
    public string Region { get; set; } = "Unknown";
    public List<string> Languages { get; set; } = [];
    public string DumpQuality { get; set; } = "Neutral";
    public string Revision { get; set; } = "Original";
    public string Version { get; set; } = "None";
    public string ReleaseType { get; set; } = "Standard retail";
}
