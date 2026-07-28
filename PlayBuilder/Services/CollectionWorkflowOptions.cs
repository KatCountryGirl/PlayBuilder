using PlayBuilder.Models;

namespace PlayBuilder.Services;

public enum CollectionWorkflow
{
    OneGameOneRomAllGames,
    OneGameOneRomEnglishOnly,
    Favorites,
    Custom
}

public enum ReleasePreferencePreset
{
    EnglishFirst,
    UsaFirst,
    EuropeFirst,
    JapaneseFirst,
    Custom
}

public sealed record CollectionWorkflowOption(
    CollectionWorkflow Workflow,
    string Icon,
    string Title,
    string Description);

public sealed record ReleasePreferenceOption(
    ReleasePreferencePreset Preset,
    string Title,
    string Description);

public static class CollectionWorkflowPresets
{
    public static readonly IReadOnlyList<CollectionWorkflowOption> Workflows =
    [
        new(CollectionWorkflow.OneGameOneRomAllGames, "🏆", "1G1R All Games", "Choose one recommended release for each game."),
        new(CollectionWorkflow.OneGameOneRomEnglishOnly, "🔤", "1G1R English Only", "Build a collection containing games Atlas can identify as playable in English."),
        new(CollectionWorkflow.Favorites, "⭐", "Favorites", "Build a collection from games you personally marked or selected as favorites."),
        new(CollectionWorkflow.Custom, "🧩", "Custom", "Create a collection using advanced Atlas preferences and rules.")
    ];

    public static readonly IReadOnlyList<ReleasePreferenceOption> ReleasePreferences =
    [
        new(ReleasePreferencePreset.EnglishFirst, "English First", "Prefer English releases whenever one is available."),
        new(ReleasePreferencePreset.UsaFirst, "USA First", "Prefer USA releases when language and quality are otherwise suitable."),
        new(ReleasePreferencePreset.EuropeFirst, "Europe First", "Prefer European releases for region-sensitive collections."),
        new(ReleasePreferencePreset.JapaneseFirst, "Japanese First", "Prefer Japanese releases when you want the original region first."),
        new(ReleasePreferencePreset.Custom, "Custom...", "Show the detailed language and region order.")
    ];

    public static CollectionRuleOptions ApplyWorkflow(
        CollectionRuleOptions options,
        CollectionWorkflow workflow)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Mode = workflow == CollectionWorkflow.OneGameOneRomEnglishOnly
            ? OneGameOneRomMode.EnglishOnly
            : OneGameOneRomMode.AllGames;
        return options;
    }

    public static CollectionRuleOptions ApplyReleasePreference(
        CollectionRuleOptions options,
        ReleasePreferencePreset preset)
    {
        ArgumentNullException.ThrowIfNull(options);

        switch (preset)
        {
            case ReleasePreferencePreset.EnglishFirst:
                options.LanguagePriority = ["English", "Japanese", "French", "German", "Spanish", "Italian", "Portuguese", "Korean", "Dutch", "Swedish", "Unknown"];
                options.RegionPriority = ["USA", "World", "Europe", "Australia", "United Kingdom", "Canada", "Japan", "Korea", "Brazil", "France", "Germany", "Spain", "Italy", "Unknown"];
                break;
            case ReleasePreferencePreset.UsaFirst:
                options.LanguagePriority = ["English", "Japanese", "French", "German", "Spanish", "Italian", "Portuguese", "Korean", "Dutch", "Swedish", "Unknown"];
                options.RegionPriority = ["USA", "World", "Canada", "Europe", "Australia", "United Kingdom", "Japan", "Korea", "Brazil", "France", "Germany", "Spain", "Italy", "Unknown"];
                break;
            case ReleasePreferencePreset.EuropeFirst:
                options.LanguagePriority = ["English", "French", "German", "Spanish", "Italian", "Portuguese", "Dutch", "Swedish", "Japanese", "Korean", "Unknown"];
                options.RegionPriority = ["Europe", "United Kingdom", "World", "Australia", "USA", "Canada", "France", "Germany", "Spain", "Italy", "Japan", "Korea", "Brazil", "Unknown"];
                break;
            case ReleasePreferencePreset.JapaneseFirst:
                options.LanguagePriority = ["Japanese", "English", "Korean", "French", "German", "Spanish", "Italian", "Portuguese", "Dutch", "Swedish", "Unknown"];
                options.RegionPriority = ["Japan", "World", "USA", "Europe", "Korea", "Australia", "United Kingdom", "Canada", "Brazil", "France", "Germany", "Spain", "Italy", "Unknown"];
                break;
            case ReleasePreferencePreset.Custom:
                break;
        }

        return options;
    }
}
