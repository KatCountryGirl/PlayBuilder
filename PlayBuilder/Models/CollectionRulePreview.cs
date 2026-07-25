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
}

public sealed class CollectionRulePreview
{
    public string EngineName { get; set; } = string.Empty;
    public int DuplicateGroupsReviewed { get; set; }
    public int ConfidentSelections { get; set; }
    public int FallbackSelections { get; set; }
    public int AlternativesExcluded { get; set; }
    public int GroupsExcludedByLanguage { get; set; }
    public List<GameSelectionPreview> Selections { get; set; } = [];
}

public sealed class GameSelectionPreview
{
    public string Title { get; set; } = string.Empty;
    public string RecommendedVariant { get; set; } = string.Empty;
    public string RecommendedRegion { get; set; } = "Unknown";
    public string RecommendedLanguage { get; set; } = "Unknown";
    public string Reason { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
    public List<string> DecisionReasons { get; set; } = [];
    public List<string> Alternatives { get; set; } = [];
}
