using System.Text.RegularExpressions;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed partial class CollectionRuleService : ICollectionRuleService
{
    private static readonly string[] SpecialTokens =
    [
        "beta", "proto", "prototype", "demo", "sample", "hack", "translation",
        "translated", "homebrew", "unlicensed", "pirate"
    ];

    private static readonly (string Language, string[] Tokens)[] LanguageRules =
    [
        ("English", ["english", "eng", "en"]),
        ("Japanese", ["japanese", "jpn", "ja"]),
        ("French", ["french", "fre", "fra", "fr"]),
        ("German", ["german", "ger", "deu", "de"]),
        ("Spanish", ["spanish", "spa", "es"]),
        ("Italian", ["italian", "ita", "it"]),
        ("Portuguese", ["portuguese", "por", "pt"]),
        ("Korean", ["korean", "kor", "ko"]),
        ("Dutch", ["dutch", "nld", "dut", "nl"]),
        ("Swedish", ["swedish", "swe", "sv"])
    ];

    public CollectionRulePreview BuildPreview(ArchiveScanResult scan, CollectionRuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(scan);
        ArgumentNullException.ThrowIfNull(options);

        var preview = new CollectionRulePreview();

        foreach (var group in scan.DuplicateGroups)
        {
            var candidates = group.Variants
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(name => Score(name, options))
                .ToList();

            if (options.Mode == OneGameOneRomMode.EnglishOnly)
            {
                candidates = candidates
                    .Where(candidate => candidate.Languages.Contains("English", StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (candidates.Count == 0)
                {
                    preview.GroupsExcludedByLanguage++;
                    continue;
                }
            }

            candidates = candidates
                .OrderByDescending(candidate => candidate.Score)
                .ThenBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (candidates.Count == 0)
            {
                continue;
            }

            var winner = candidates[0];
            var fallback = IsFallback(winner, options);

            preview.Selections.Add(new GameSelectionPreview
            {
                Title = group.Title,
                RecommendedVariant = winner.Name,
                RecommendedRegion = winner.Region,
                RecommendedLanguage = winner.PrimaryLanguage,
                IsFallback = fallback,
                Reason = BuildReason(winner, options, fallback),
                Alternatives = candidates.Skip(1).Select(candidate => candidate.Name).ToList()
            });

            preview.DuplicateGroupsReviewed++;
            preview.AlternativesExcluded += Math.Max(0, candidates.Count - 1);
            if (fallback) preview.FallbackSelections++;
            else preview.ConfidentSelections++;
        }

        preview.Selections = preview.Selections
            .OrderBy(selection => selection.IsFallback)
            .ThenBy(selection => selection.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return preview;
    }

    private static Candidate Score(string name, CollectionRuleOptions options)
    {
        var region = DetectRegion(name);
        var languages = DetectLanguages(name, region);
        var primaryLanguage = languages
            .OrderBy(language => PriorityIndex(options.LanguagePriority, language))
            .FirstOrDefault() ?? "Unknown";

        var regionIndex = PriorityIndex(options.RegionPriority, region);
        var languageIndex = PriorityIndex(options.LanguagePriority, primaryLanguage);

        // Language is intentionally the strongest preference. Region breaks ties within
        // the preferred language, while unique non-preferred-language games remain valid
        // in All Games mode.
        var score = 100_000 - (languageIndex * 5_000) - (regionIndex * 500);
        var isSpecial = SpecialTokens.Any(token =>
            Regex.IsMatch(name, $@"(?i)(?:\b|\[|\(){Regex.Escape(token)}(?:\b|\]|\))"));
        var revision = ParseRevision(name);

        if (options.AvoidSpecialReleases && isSpecial) score -= 25_000;
        if (options.PreferNewestRevision) score += revision * 10;

        return new Candidate(name, region, languages, primaryLanguage, isSpecial, revision, score);
    }

    private static bool IsFallback(Candidate winner, CollectionRuleOptions options)
    {
        var regionKnown = !winner.Region.Equals("Unknown", StringComparison.OrdinalIgnoreCase);
        var languageKnown = !winner.PrimaryLanguage.Equals("Unknown", StringComparison.OrdinalIgnoreCase);
        return !regionKnown || !languageKnown;
    }

    private static string BuildReason(Candidate winner, CollectionRuleOptions options, bool fallback)
    {
        var parts = new List<string>
        {
            $"{winner.PrimaryLanguage} is the highest available language preference",
            $"{winner.Region} is the highest available region within that language"
        };

        if (options.Mode == OneGameOneRomMode.EnglishOnly)
        {
            parts.Insert(0, "English-only mode");
        }
        else
        {
            parts.Insert(0, "All-games mode keeps unique games even when no English release exists");
        }

        if (options.PreferNewestRevision && winner.Revision > 0)
        {
            parts.Add($"revision {winner.Revision} preferred");
        }

        if (options.AvoidSpecialReleases && !winner.IsSpecial)
        {
            parts.Add("standard release preferred over beta/demo/hack variants");
        }

        if (fallback)
        {
            parts.Add("review suggested because language or region could not be identified confidently");
        }

        return string.Join(" · ", parts);
    }

    private static int PriorityIndex(IReadOnlyList<string> priority, string value)
    {
        for (var index = 0; index < priority.Count; index++)
        {
            if (priority[index].Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return priority.Count + 1;
    }

    private static List<string> DetectLanguages(string value, string region)
    {
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tags = ParentheticalRegex().Matches(value)
            .Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);

        foreach (var tag in tags)
        {
            var tokens = TokenSplitRegex().Split(tag.ToLowerInvariant())
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in LanguageRules)
            {
                if (rule.Tokens.Any(tokens.Contains)) found.Add(rule.Language);
            }
        }

        // Region inference is used only when explicit language tags are absent.
        if (found.Count == 0)
        {
            switch (region)
            {
                case "USA":
                case "World":
                case "Europe":
                case "Australia":
                case "United Kingdom":
                case "Canada":
                    found.Add("English");
                    break;
                case "Japan": found.Add("Japanese"); break;
                case "Korea": found.Add("Korean"); break;
                case "France": found.Add("French"); break;
                case "Germany": found.Add("German"); break;
                case "Spain": found.Add("Spanish"); break;
                case "Italy": found.Add("Italian"); break;
                case "Brazil": found.Add("Portuguese"); break;
            }
        }

        if (found.Count == 0) found.Add("Unknown");
        return found.ToList();
    }

    private static string DetectRegion(string value)
    {
        if (Token(value, "USA") || Token(value, "United States")) return "USA";
        if (Token(value, "World")) return "World";
        if (Token(value, "Europe")) return "Europe";
        if (Token(value, "Australia")) return "Australia";
        if (Token(value, "UK") || Token(value, "United Kingdom")) return "United Kingdom";
        if (Token(value, "Canada")) return "Canada";
        if (Token(value, "Japan")) return "Japan";
        if (Token(value, "Germany")) return "Germany";
        if (Token(value, "France")) return "France";
        if (Token(value, "Spain")) return "Spain";
        if (Token(value, "Italy")) return "Italy";
        if (Token(value, "Korea")) return "Korea";
        if (Token(value, "Brazil")) return "Brazil";
        return "Unknown";
    }

    private static bool Token(string value, string token) =>
        Regex.IsMatch(value, $@"(?i)(?:\(|\[|,|^|\s){Regex.Escape(token)}(?:\)|\]|,|$|\s)");

    private static int ParseRevision(string value)
    {
        var match = RevisionRegex().Match(value);
        if (!match.Success) return 0;

        var token = match.Groups[1].Value;
        if (int.TryParse(token, out var numeric)) return numeric;
        if (token.Length == 1 && char.IsLetter(token[0])) return char.ToUpperInvariant(token[0]) - 'A' + 1;
        return 0;
    }

    private sealed record Candidate(
        string Name,
        string Region,
        List<string> Languages,
        string PrimaryLanguage,
        bool IsSpecial,
        int Revision,
        int Score);

    [GeneratedRegex(@"\(([^()]*)\)|\[([^\[\]]*)\]", RegexOptions.Compiled)]
    private static partial Regex ParentheticalRegex();

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex TokenSplitRegex();

    [GeneratedRegex(@"(?i)\b(?:rev(?:ision)?|ver(?:sion)?|v)\s*([a-z0-9]+)\b", RegexOptions.Compiled)]
    private static partial Regex RevisionRegex();
}
