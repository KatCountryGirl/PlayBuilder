using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Data.Entities;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed partial class ArchiveScanner : IArchiveScanner
{
    private readonly IDbContextFactory<PlayBuilderDbContext> _dbFactory;

    public ArchiveScanner(IDbContextFactory<PlayBuilderDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    private static readonly HashSet<string> RecognizedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".3ds", ".7z", ".a26", ".a52", ".a78", ".adf", ".atr", ".bin", ".cdi",
        ".chd", ".cia", ".col", ".cue", ".d64", ".d88", ".fds", ".fig", ".gb",
        ".gba", ".gbc", ".gcm", ".gcz", ".gen", ".gg", ".iso", ".j64", ".lnx",
        ".md", ".mdf", ".nds", ".nes", ".ngc", ".nkit", ".nsp", ".pce", ".pbp",
        ".rvz", ".sfc", ".sg", ".smc", ".sms", ".st", ".swc", ".tap", ".tgc",
        ".v64", ".wad", ".wbfs", ".wia", ".wud", ".wux", ".xci", ".z64", ".zip"
    };

    private static readonly string[] RegionPriority =
    [
        "USA", "World", "Europe", "Australia", "United Kingdom", "Canada",
        "Japan", "Germany", "France", "Spain", "Italy", "Korea", "Brazil", "Unknown"
    ];

    private static readonly (string Name, string[] Tokens)[] LanguageRules =
    [
        ("English", ["en", "eng", "english"]),
        ("French", ["fr", "fre", "fra", "french"]),
        ("German", ["de", "ger", "deu", "german"]),
        ("Spanish", ["es", "spa", "spanish"]),
        ("Italian", ["it", "ita", "italian"]),
        ("Japanese", ["ja", "jpn", "japanese"]),
        ("Korean", ["ko", "kor", "korean"]),
        ("Portuguese", ["pt", "por", "portuguese"]),
        ("Dutch", ["nl", "dut", "nld", "dutch"]),
        ("Swedish", ["sv", "swe", "swedish"])
    ];

    private static readonly (string Name, Regex Pattern)[] SpecialTagRules =
    [
        ("Beta", BetaRegex()),
        ("Prototype", PrototypeRegex()),
        ("Demo", DemoRegex()),
        ("Hack", HackRegex()),
        ("Translation", TranslationRegex()),
        ("Homebrew", HomebrewRegex()),
        ("Unlicensed", UnlicensedRegex()),
        ("Sample", SampleRegex()),
        ("Pirate", PirateRegex())
    ];

    public async Task<ArchiveScanResult> ScanAsync(
        string archivePath,
        IProgress<ArchiveScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var output = await Task.Run(
            () => ScanCore(archivePath, progress, cancellationToken),
            cancellationToken);

        progress?.Report(new ArchiveScanProgress(
            output.Result.RecognizedFileCount,
            output.Result.RecognizedFileCount,
            output.Result.ArchivePath,
            string.Empty,
            output.Result.Duration,
            "Saving catalog to Library"));

        await SaveCatalogAsync(output.Games, cancellationToken);
        return output.Result;
    }

    private static ScanOutput ScanCore(
        string archivePath,
        IProgress<ArchiveScanProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(archivePath))
        {
            throw new ArgumentException("An archive path is required.", nameof(archivePath));
        }

        var root = Path.GetFullPath(archivePath.Trim());
        if (!Directory.Exists(root))
        {
            throw new DirectoryNotFoundException($"The archive folder does not exist: {root}");
        }

        var result = new ArchiveScanResult
        {
            ArchivePath = root,
            StartedAt = DateTimeOffset.UtcNow,
            IncludesFileSizes = false
        };

        var systems = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var extensions = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var regions = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var languages = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var specialTags = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var duplicateCandidates = new List<(string SystemName, string NormalizedTitle, string DisplayTitle)>();
        var oneGameOneRomCandidates = new List<(string SystemName, string NormalizedTitle, string DisplayTitle)>();
        var discGroups = new Dictionary<string, List<(int Disc, string File)>>(StringComparer.OrdinalIgnoreCase);
        var catalog = new List<Game>();
        var stopwatch = Stopwatch.StartNew();
        var lastProgressAt = TimeSpan.Zero;

        long checkedFiles = 0;
        string currentFile = string.Empty;
        string currentFolder = root;

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.ReparsePoint,
            MatchType = MatchType.Simple
        };

        try
        {
            // EnumerateFiles avoids the extra Directory.Exists/File.GetAttributes call that
            // v0.0.5 made for every entry. This matters enormously on mapped/network drives.
            foreach (var filePath in Directory.EnumerateFiles(root, "*", options))
            {
                cancellationToken.ThrowIfCancellationRequested();

                checkedFiles++;
                currentFile = Path.GetFileName(filePath);
                currentFolder = Path.GetDirectoryName(filePath) ?? root;
                var extension = Path.GetExtension(currentFile);

                if (!RecognizedExtensions.Contains(extension))
                {
                    result.IgnoredFileCount++;
                    ReportProgressIfDue(progress, checkedFiles, result.RecognizedFileCount,
                        currentFolder, currentFile, stopwatch.Elapsed, ref lastProgressAt, "Reading filenames");
                    continue;
                }

                result.RecognizedFileCount++;
                var systemName = GetSystemName(root, filePath);
                Increment(systems, systemName);
                Increment(extensions, extension.ToLowerInvariant());

                var displayTitle = Path.GetFileNameWithoutExtension(currentFile);
                var tags = ExtractTags(displayTitle);
                var region = DetectRegion(tags);
                Increment(regions, region);

                var detectedLanguages = DetectLanguages(tags).ToList();
                foreach (var language in detectedLanguages)
                {
                    Increment(languages, language);
                }

                foreach (var rule in SpecialTagRules)
                {
                    if (rule.Pattern.IsMatch(displayTitle))
                    {
                        Increment(specialTags, rule.Name);
                    }
                }

                var normalizedTitle = GameTitleIdentity.NormalizeTitle(displayTitle);
                if (string.IsNullOrWhiteSpace(normalizedTitle))
                {
                    normalizedTitle = displayTitle.Trim().ToLowerInvariant();
                }

                duplicateCandidates.Add((systemName, normalizedTitle, displayTitle));

                var oneGameOneRomTitle = GameTitleIdentity.NormalizeOneGameOneRomTitle(displayTitle);
                if (string.IsNullOrWhiteSpace(oneGameOneRomTitle))
                {
                    oneGameOneRomTitle = normalizedTitle;
                }

                oneGameOneRomCandidates.Add((systemName, oneGameOneRomTitle, displayTitle));

                var discNumber = 0;
                var discMatch = DiscRegex().Match(displayTitle);
                if (discMatch.Success && int.TryParse(discMatch.Groups[1].Value, out var parsedDiscNumber))
                {
                    discNumber = parsedDiscNumber;
                    var baseTitle = DiscRegex().Replace(displayTitle, " ");
                    baseTitle = GameTitleIdentity.NormalizeTitle(baseTitle);
                    if (!discGroups.TryGetValue(baseTitle, out var discs))
                    {
                        discs = [];
                        discGroups[baseTitle] = discs;
                    }
                    discs.Add((discNumber, displayTitle));
                }

                var revisionMatch = RevisionRegex().Match(displayTitle);
                var revision = revisionMatch.Success ? revisionMatch.Value.Trim(' ', '(', ')', '[', ']') : string.Empty;
                catalog.Add(new Game
                {
                    Title = GameTitleIdentity.CleanDisplayTitle(displayTitle),
                    SortTitle = normalizedTitle,
                    System = systemName,
                    Region = region,
                    Language = detectedLanguages.Count > 0
                        ? string.Join(", ", detectedLanguages)
                        : InferLanguageFromRegion(region),
                    Revision = revision,
                    DiscNumber = discNumber,
                    Extension = extension.ToLowerInvariant(),
                    SourcePath = filePath,
                    RelativePath = Path.GetRelativePath(root, filePath),
                    Added = DateTime.UtcNow,
                    Modified = DateTime.UtcNow
                });

                ReportProgressIfDue(progress, checkedFiles, result.RecognizedFileCount,
                    currentFolder, currentFile, stopwatch.Elapsed, ref lastProgressAt, "Building game catalog");
            }
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            AddWarning(result, $"The scan stopped at an inaccessible location: {currentFolder}");
        }

        progress?.Report(new ArchiveScanProgress(
            checkedFiles, result.RecognizedFileCount, currentFolder, currentFile,
            stopwatch.Elapsed, "Grouping releases"));

        result.Systems = ToSystemSummaries(systems);
        result.FileTypes = ToExtensionSummaries(extensions);
        result.Regions = ToRegionSummaries(regions);
        result.Languages = ToMetadataSummaries(languages);
        result.SpecialTags = ToMetadataSummaries(specialTags);

        result.OneGameOneRomGroups = DuplicateGrouping.BuildOneGameOneRomGroups(oneGameOneRomCandidates);

        result.DuplicateGroups = DuplicateGrouping.BuildDuplicateGroups(duplicateCandidates, take: 250);
        foreach (var group in result.DuplicateGroups)
        {
            group.Variants = group.Variants.Take(12).ToList();
        }

        result.MultiDiscGroups = discGroups
            .Where(item => item.Value.Select(value => value.Disc).Distinct().Count() > 1)
            .Select(item => new MultiDiscGroupSummary
            {
                Title = ToDisplayTitle(item.Key),
                DiscCount = item.Value.Select(value => value.Disc).Distinct().Count(),
                Files = item.Value
                    .OrderBy(value => value.Disc)
                    .ThenBy(value => value.File, StringComparer.OrdinalIgnoreCase)
                    .Select(value => value.File)
                    .Take(20)
                    .ToList()
            })
            .OrderByDescending(item => item.DiscCount)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .Take(250)
            .ToList();

        stopwatch.Stop();
        result.CompletedAt = DateTimeOffset.UtcNow;

        progress?.Report(new ArchiveScanProgress(
            checkedFiles, result.RecognizedFileCount, currentFolder, currentFile,
            stopwatch.Elapsed, "Complete"));

        return new ScanOutput(result, catalog);
    }

    private async Task SaveCatalogAsync(IReadOnlyCollection<Game> games, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // Keep user choices across rescans even though the catalog itself is rebuilt.
        var favoritePathList = await db.Games
            .Where(game => game.IsFavorite)
            .Select(game => game.SourcePath)
            .ToListAsync(cancellationToken);
        var favoritePaths = favoritePathList.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var game in games)
        {
            game.IsFavorite = favoritePaths.Contains(game.SourcePath);
        }

        // PlayBuilder currently manages one configured archive. A completed scan replaces
        // the catalog atomically from the user's point of view while leaving source files untouched.
        await db.Games.ExecuteDeleteAsync(cancellationToken);

        const int batchSize = 2_000;
        foreach (var batch in games.Chunk(batchSize))
        {
            await db.Games.AddRangeAsync(batch, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }
    }

    private static string InferLanguageFromRegion(string region) => region switch
    {
        "USA" or "World" or "Australia" or "United Kingdom" or "Canada" => "English",
        "Japan" => "Japanese",
        "Germany" => "German",
        "France" => "French",
        "Spain" => "Spanish",
        "Italy" => "Italian",
        "Korea" => "Korean",
        "Brazil" => "Portuguese",
        _ => string.Empty
    };

    private sealed record ScanOutput(ArchiveScanResult Result, List<Game> Games);

    private static List<SystemScanSummary> ToSystemSummaries(Dictionary<string, long> values) =>
        values.Select(item => new SystemScanSummary { Name = item.Key, FileCount = item.Value })
            .OrderByDescending(item => item.FileCount)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<ExtensionScanSummary> ToExtensionSummaries(Dictionary<string, long> values) =>
        values.Select(item => new ExtensionScanSummary { Extension = item.Key, FileCount = item.Value })
            .OrderByDescending(item => item.FileCount)
            .ThenBy(item => item.Extension, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<RegionScanSummary> ToRegionSummaries(Dictionary<string, long> values) =>
        values.Select(item => new RegionScanSummary { Region = item.Key, FileCount = item.Value })
            .OrderBy(item => Array.IndexOf(RegionPriority, item.Region) is var index && index >= 0 ? index : int.MaxValue)
            .ThenByDescending(item => item.FileCount)
            .ToList();

    private static List<MetadataScanSummary> ToMetadataSummaries(Dictionary<string, long> values) =>
        values.Select(item => new MetadataScanSummary { Name = item.Key, FileCount = item.Value })
            .OrderByDescending(item => item.FileCount)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string GetSystemName(string root, string filePath)
    {
        var relativePath = Path.GetRelativePath(root, filePath);
        var separatorIndex = relativePath.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        return separatorIndex > 0
            ? relativePath[..separatorIndex]
            : Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }

    private static List<string> ExtractTags(string title) =>
        ParentheticalTagRegex().Matches(title)
            .Select(match => match.Groups[1].Value.Trim())
            .Where(tag => tag.Length > 0)
            .ToList();

    private static string DetectRegion(IEnumerable<string> tags)
    {
        foreach (var tag in tags)
        {
            var normalized = tag.Replace("United States", "USA", StringComparison.OrdinalIgnoreCase);

            if (ContainsToken(normalized, "USA")) return "USA";
            if (ContainsToken(normalized, "World")) return "World";
            if (ContainsToken(normalized, "Europe")) return "Europe";
            if (ContainsToken(normalized, "Australia")) return "Australia";
            if (ContainsToken(normalized, "UK") || ContainsToken(normalized, "United Kingdom")) return "United Kingdom";
            if (ContainsToken(normalized, "Canada")) return "Canada";
            if (ContainsToken(normalized, "Japan")) return "Japan";
            if (ContainsToken(normalized, "Germany")) return "Germany";
            if (ContainsToken(normalized, "France")) return "France";
            if (ContainsToken(normalized, "Spain")) return "Spain";
            if (ContainsToken(normalized, "Italy")) return "Italy";
            if (ContainsToken(normalized, "Korea")) return "Korea";
            if (ContainsToken(normalized, "Brazil")) return "Brazil";
        }

        return "Unknown";
    }

    private static IEnumerable<string> DetectLanguages(IEnumerable<string> tags)
    {
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            var tokens = TagTokenRegex().Split(tag.ToLowerInvariant())
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in LanguageRules)
            {
                if (rule.Tokens.Any(tokens.Contains))
                {
                    found.Add(rule.Name);
                }
            }
        }

        return found;
    }

    private static bool ContainsToken(string value, string token) =>
        value.Equals(token, StringComparison.OrdinalIgnoreCase) ||
        value.Split([',', '+', '/', '&'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part.Equals(token, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeTitle(string fileNameWithoutExtension)
    {
        var withoutDisc = DiscRegex().Replace(fileNameWithoutExtension, " ");
        var withoutTags = ParentheticalTagRegex().Replace(withoutDisc, " ");
        var withoutRevision = RevisionRegex().Replace(withoutTags, " ");
        var normalized = NonAlphaNumericRegex().Replace(withoutRevision, " ");
        return WhitespaceRegex().Replace(normalized, " ").Trim().ToLowerInvariant();
    }

    private static string NormalizeOneGameOneRomTitle(string fileNameWithoutExtension)
    {
        var withDiscIdentity = DiscRegex().Replace(fileNameWithoutExtension, match =>
            int.TryParse(match.Groups[1].Value, out var discNumber)
                ? $" disc {discNumber} "
                : " ");
        var withoutTags = ParentheticalTagRegex().Replace(withDiscIdentity, " ");
        var withoutRevision = RevisionRegex().Replace(withoutTags, " ");
        var normalized = NonAlphaNumericRegex().Replace(withoutRevision, " ");
        return WhitespaceRegex().Replace(normalized, " ").Trim().ToLowerInvariant();
    }

    private static string CleanDisplayTitle(string fileNameWithoutExtension)
    {
        var value = DiscRegex().Replace(fileNameWithoutExtension, " ");
        value = ParentheticalTagRegex().Replace(value, " ");
        value = RevisionRegex().Replace(value, " ");
        value = WhitespaceRegex().Replace(value, " ").Trim(' ', '-', '_', '.');
        return string.IsNullOrWhiteSpace(value) ? fileNameWithoutExtension.Trim() : value;
    }

    private static string ToDisplayTitle(string normalizedTitle)
    {
        if (string.IsNullOrWhiteSpace(normalizedTitle)) return "Unknown title";
        return string.Join(' ', normalizedTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    private static void Increment(IDictionary<string, long> totals, string key)
    {
        totals.TryGetValue(key, out var current);
        totals[key] = current + 1;
    }

    private static void AddWarning(ArchiveScanResult result, string warning)
    {
        if (result.Warnings.Count < 25)
        {
            result.Warnings.Add(warning);
        }
    }

    private static void ReportProgressIfDue(
        IProgress<ArchiveScanProgress>? progress,
        long checkedFiles,
        long recognizedFiles,
        string folder,
        string file,
        TimeSpan elapsed,
        ref TimeSpan lastProgressAt,
        string phase)
    {
        if (progress is null) return;

        // Four updates per second keeps Blazor responsive without slowing the scan.
        if (checkedFiles == 1 || elapsed - lastProgressAt >= TimeSpan.FromMilliseconds(250))
        {
            lastProgressAt = elapsed;
            progress.Report(new ArchiveScanProgress(
                checkedFiles, recognizedFiles, folder, file, elapsed, phase));
        }
    }

    [GeneratedRegex(@"\(([^()]*)\)", RegexOptions.Compiled)]
    private static partial Regex ParentheticalTagRegex();

    [GeneratedRegex(@"(?i)(?:\(|\[|\b)(?:disc|disk|cd)\s*[-_ ]*0*([1-9][0-9]*)(?:\)|\]|\b)", RegexOptions.Compiled)]
    private static partial Regex DiscRegex();

    [GeneratedRegex(@"(?i)\b(?:rev(?:ision)?|ver(?:sion)?|v)\s*[a-z0-9.]+\b", RegexOptions.Compiled)]
    private static partial Regex RevisionRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex TagTokenRegex();

    [GeneratedRegex(@"(?i)(?:\(|\[|\b)beta(?:\)|\]|\b)", RegexOptions.Compiled)]
    private static partial Regex BetaRegex();

    [GeneratedRegex(@"(?i)(?:\(|\[|\b)(?:proto|prototype)(?:\)|\]|\b)", RegexOptions.Compiled)]
    private static partial Regex PrototypeRegex();

    [GeneratedRegex(@"(?i)(?:\(|\[|\b)demo(?:\)|\]|\b)", RegexOptions.Compiled)]
    private static partial Regex DemoRegex();

    [GeneratedRegex(@"(?i)(?:\(|\[|\b)hack(?:\)|\]|\b)", RegexOptions.Compiled)]
    private static partial Regex HackRegex();

    [GeneratedRegex(@"(?i)(?:translation|translated|trans)", RegexOptions.Compiled)]
    private static partial Regex TranslationRegex();

    [GeneratedRegex(@"(?i)\bhomebrew\b", RegexOptions.Compiled)]
    private static partial Regex HomebrewRegex();

    [GeneratedRegex(@"(?i)\bunlicensed\b", RegexOptions.Compiled)]
    private static partial Regex UnlicensedRegex();

    [GeneratedRegex(@"(?i)\bsample\b", RegexOptions.Compiled)]
    private static partial Regex SampleRegex();

    [GeneratedRegex(@"(?i)\bpirate\b", RegexOptions.Compiled)]
    private static partial Regex PirateRegex();
}
