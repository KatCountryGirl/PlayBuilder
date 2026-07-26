namespace PlayBuilder.Services;

public sealed record AtlasHelpPage(
    string Title,
    string Purpose,
    IReadOnlyList<string> MainControls,
    string NextStep,
    string Safety,
    IReadOnlyList<string> CommonQuestions);

public static class AtlasHelpContent
{
    private static readonly AtlasHelpPage DefaultHelp = new(
        "Home",
        "Home shows your latest scan, collection health, and the safest next step.",
        ["Use the summary cards to check scan status.", "Open Scan Games when your Game folder changes.", "Open Collections when you are ready to review recommendations."],
        "Scan your games or choose a collection type.",
        "Home is read-only. It never changes game files.",
        ["Game file count is the number of files found.", "Duplicate groups are groups of two or more releases that appear to represent the same game on the same system. Matching titles on different systems are kept separate."]);

    private static readonly Dictionary<string, AtlasHelpPage> Pages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dashboard"] = DefaultHelp,
        ["scan"] = new(
            "Scan Games",
            "Scan Games reads your source Game folder and creates the title groups used by Collection Builder.",
            ["Source folder is where your original game files live.", "Browse buttons open a Windows folder picker while keeping manual path entry available.", "Destination folder is where curated collections will be built later.", "Frontend tells PlayBuilder how your finished collection should be organized. Different frontends expect different folder names and layouts. PlayBuilder uses this choice when it builds or exports your collection. It does not change your original game files.", "RetroBat, Batocera, and other EmulationStation-style layouts use system folders expected by those frontends.", "Generic folder layout keeps output simple when no specific frontend layout is needed.", "Scan starts a read-only filename scan that continues in the background.", "Cancel Scan stops the running scan and keeps the last completed report.", "Rescan replaces the previous scan report with fresh results."],
            "Run a scan, then open Collections.",
            "Scanning is read-only. PlayBuilder does not copy, move, rename, or delete game files during a scan.",
            ["Game file count is every recognized file.", "1G1R group count is every unique title group.", "Single-game groups are valid and can be recommended automatically.", "Changing frontend affects the destination structure, not the source files.", "Rescan is safe and updates the latest scan report."]),
        ["library"] = new(
            "Library",
            "Library lets you browse the games found during the latest scan.",
            ["The wider Systems panel lets you browse one platform at a time without hiding normal console names.", "System, region, language, format, and duplicate-status filters narrow the list.", "Search finds game titles, filenames, and system names.", "Duplicate groups are groups of two or more releases that appear to represent the same game on the same system. Matching titles on different systems are kept separate.", "Duplicate release indicators show titles with more than one detected release on that system.", "Favorite stars mark games for a Favorites collection."],
            "Favorite games or return to Collections.",
            "Browsing and favoriting do not alter source game files.",
            ["A game can appear once per scanned file.", "The System filter and Systems panel use the same selection.", "Unknown metadata usually means the filename did not include a clear tag."]),
        ["collection-rules"] = new(
            "Collections",
            "Collections helps you choose which game files belong in a curated collection.",
            ["1G1R means one selected game file per unique game or title group.", "All Games keeps one best playable copy of every unique game.", "English Only keeps one best English-capable copy of every unique game.", "Language priority decides preferred language before region.", "Region priority breaks ties after language.", "Needs Review means Atlas found unknown or less certain metadata.", "Extra Versions are alternate game files not selected as the winner.", "Checkboxes decide what will be included in Build Preview.", "Advanced rule switches are optional expert controls.", "Atlas Profiles are optional advanced presets."],
            "Review the checked recommendations, then open Build Preview.",
            "Changing checkboxes does not change Atlas recommendations. Unchecked games will not be included in the saved build collection.",
            ["Atlas chooses by deterministic rules, not scoring.", "A game file can win because its language, region, dump quality, revision, version, or release type is preferred.", "Unchecking a game only excludes it from your build selection."]),
        ["build"] = new(
            "Build Preview",
            "Build Preview shows the saved collection before any file operation is approved.",
            ["Selected items are included in the current review.", "Excluded items are removed from the saved collection list.", "Approve and Build remains disabled until execution is implemented."],
            "Review selected and excluded items before approving any future build.",
            "Nothing is copied, moved, renamed, or deleted until a later confirmation step explicitly performs file operations.",
            ["Selected means included in the collection plan.", "Excluded means removed from that saved collection.", "File operation counts stay zero until build execution exists."]),
        ["tools"] = new(
            "Conversion",
            "Conversion will prepare playable formats while keeping originals separate.",
            ["Choose a source.", "Choose a separate destination.", "Review conversion output before using it."],
            "Use this after a collection is reviewed.",
            "Conversions should write to a separate destination and leave source game files untouched.",
            ["Conversion is not required for every frontend.", "Original game files should remain protected."]),
        ["downloads"] = new(
            "Downloads",
            "Downloads will manage source links that you provide.",
            ["Add sources you are allowed to use.", "Review planned files.", "Start downloads only after checking the plan."],
            "Add a source only when you have permission to access it.",
            "PlayBuilder does not provide game sources or bypass access rules.",
            ["Downloads are separate from scanning.", "Only use content you are permitted to access."]),
        ["metadata"] = new(
            "Metadata",
            "Metadata helps review artwork and game details for your library.",
            ["Find missing metadata.", "Review provider results.", "Approve updates before applying them."],
            "Review missing or unclear game details.",
            "Metadata changes should be previewed before they are written.",
            ["Metadata does not decide Atlas recommendations.", "Filename metadata and artwork metadata are different."]),
        ["advanced"] = new(
            "Review & Cleanup",
            "Review & Cleanup is for cautious maintenance and quarantine workflows.",
            ["Preview cleanup actions.", "Review quarantine.", "Restore files if needed."],
            "Use cleanup only after reviewing a clear plan.",
            "Quarantine is safer than permanent deletion.",
            ["Cleanup is separate from Collection Builder.", "Permanent deletion should require explicit confirmation."]),
        ["settings"] = new(
            "Settings",
            "Settings stores your folders, frontend, and archive-protection preference.",
            ["Source folder points to original game files.", "Destination folder points to curated output.", "Frontend changes future collection planning.", "Archive protection keeps destructive operations away from originals."],
            "Save settings, then scan games.",
            "Saving settings does not move, rename, or delete game files.",
            ["Changing folders affects future scans.", "Destination should be separate from the source folder."])
    };

    public static AtlasHelpPage Get(string path)
    {
        var normalized = string.IsNullOrWhiteSpace(path)
            ? "dashboard"
            : path.Trim('/').ToLowerInvariant();

        return Pages.TryGetValue(normalized, out var page)
            ? page
            : DefaultHelp;
    }

    public static IReadOnlyDictionary<string, AtlasHelpPage> AllPages => Pages;
}
