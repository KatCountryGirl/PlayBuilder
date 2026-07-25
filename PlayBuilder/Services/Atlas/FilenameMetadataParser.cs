using System.Text.RegularExpressions;

namespace PlayBuilder.Services.Atlas;

/// <summary>
/// Interprets structural filename tokens into normalized Atlas metadata.
/// </summary>
public sealed partial class FilenameMetadataParser
{
    private static readonly Dictionary<string, string> RegionAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["U"] = "USA", ["US"] = "USA", ["USA"] = "USA", ["United States"] = "USA",
            ["W"] = "World", ["World"] = "World",
            ["E"] = "Europe", ["EU"] = "Europe", ["EUR"] = "Europe", ["Europe"] = "Europe",
            ["J"] = "Japan", ["JP"] = "Japan", ["JPN"] = "Japan", ["Japan"] = "Japan",
            ["AUS"] = "Australia", ["Australia"] = "Australia",
            ["UK"] = "United Kingdom", ["United Kingdom"] = "United Kingdom",
            ["Canada"] = "Canada", ["France"] = "France", ["Germany"] = "Germany",
            ["Italy"] = "Italy", ["Spain"] = "Spain", ["Brazil"] = "Brazil",
            ["Korea"] = "Korea", ["China"] = "China", ["Taiwan"] = "Taiwan"
        };

    private static readonly Dictionary<string, string> LanguageAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["En"] = "English", ["Eng"] = "English", ["English"] = "English",
            ["Fr"] = "French", ["Fre"] = "French", ["Fra"] = "French", ["French"] = "French",
            ["De"] = "German", ["Ger"] = "German", ["Deu"] = "German", ["German"] = "German",
            ["Es"] = "Spanish", ["Spa"] = "Spanish", ["Spanish"] = "Spanish",
            ["It"] = "Italian", ["Ita"] = "Italian", ["Italian"] = "Italian",
            ["Pt"] = "Portuguese", ["Por"] = "Portuguese", ["Portuguese"] = "Portuguese",
            ["Nl"] = "Dutch", ["Dut"] = "Dutch", ["Nld"] = "Dutch", ["Dutch"] = "Dutch",
            ["Sv"] = "Swedish", ["Swe"] = "Swedish", ["Swedish"] = "Swedish",
            ["Ja"] = "Japanese", ["Jpn"] = "Japanese", ["Japanese"] = "Japanese",
            ["Ko"] = "Korean", ["Kor"] = "Korean", ["Korean"] = "Korean",
            ["Zh"] = "Chinese", ["Chi"] = "Chinese", ["Chinese"] = "Chinese"
        };

    private readonly FilenameTokenizer _tokenizer;

    public FilenameMetadataParser()
        : this(new FilenameTokenizer())
    {
    }

    public FilenameMetadataParser(FilenameTokenizer tokenizer)
    {
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
    }

    public FilenameMetadata Parse(string filename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        var tokens = _tokenizer.Tokenize(filename);
        var tags = tokens
            .Where(token => token.Type is FilenameTokenType.ParentheticalTag or FilenameTokenType.BracketTag)
            .SelectMany(token => SplitTag(token.Value))
            .ToList();

        var region = FindFirstAlias(tags, RegionAliases) ?? "Unknown";
        var languages = FindAliases(tags, LanguageAliases);
        if (languages.Count == 0)
        {
            var inferred = InferLanguageFromRegion(region);
            if (inferred is not null)
            {
                languages.Add(inferred);
            }
        }

        var searchableValue = Path.GetFileNameWithoutExtension(filename);

        return new FilenameMetadata
        {
            FileName = filename,
            Title = BuildTitle(tokens),
            Region = region,
            Languages = languages,
            Revision = ParseRevision(searchableValue),
            Version = ParseVersion(searchableValue),
            DiscNumber = ParseDiscNumber(searchableValue),
            IsBeta = ContainsTag(tags, "beta"),
            IsPrototype = ContainsAnyTag(tags, "proto", "prototype"),
            IsDemo = ContainsTag(tags, "demo"),
            IsSample = ContainsTag(tags, "sample"),
            IsHack = ContainsAnyTag(tags, "hack", "h"),
            IsTranslation = ContainsAnyTag(tags, "translation", "translated", "t"),
            IsHomebrew = ContainsTag(tags, "homebrew"),
            IsUnlicensed = ContainsTag(tags, "unlicensed"),
            IsPirate = ContainsTag(tags, "pirate"),
            IsVerifiedDump = tags.Any(tag => tag.Equals("!", StringComparison.OrdinalIgnoreCase)),
            IsBadDump = tags.Any(tag =>
                tag.Equals("b", StringComparison.OrdinalIgnoreCase) ||
                tag.StartsWith("bad dump", StringComparison.OrdinalIgnoreCase))
        };
    }

    private static string BuildTitle(IReadOnlyList<FilenameToken> tokens)
    {
        var title = string.Join(" ", tokens
            .Where(token => token.Type == FilenameTokenType.Text)
            .Select(token => token.Value));

        title = MultiSpaceRegex().Replace(title, " ").Trim(' ', '-', '_', '.');
        return title;
    }

    private static IEnumerable<string> SplitTag(string tag) =>
        TagSeparatorRegex()
            .Split(tag)
            .Select(value => value.Trim())
            .Where(value => value.Length > 0);

    private static string? FindFirstAlias(
        IEnumerable<string> tags,
        IReadOnlyDictionary<string, string> aliases)
    {
        foreach (var tag in tags)
        {
            if (aliases.TryGetValue(tag, out var normalized))
            {
                return normalized;
            }
        }

        return null;
    }

    private static List<string> FindAliases(
        IEnumerable<string> tags,
        IReadOnlyDictionary<string, string> aliases)
    {
        var values = new List<string>();
        foreach (var tag in tags)
        {
            if (aliases.TryGetValue(tag, out var normalized) &&
                !values.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                values.Add(normalized);
            }
        }

        return values;
    }

    private static bool ContainsTag(IEnumerable<string> tags, string value) =>
        tags.Any(tag => tag.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static bool ContainsAnyTag(IEnumerable<string> tags, params string[] values) =>
        tags.Any(tag => values.Contains(tag, StringComparer.OrdinalIgnoreCase));

    private static int ParseRevision(string value)
    {
        var match = RevisionRegex().Match(value);
        if (!match.Success)
        {
            return 0;
        }

        var revision = match.Groups[1].Value;
        if (int.TryParse(revision, out var numericRevision))
        {
            return numericRevision;
        }

        return revision.Length == 1 && char.IsLetter(revision[0])
            ? char.ToUpperInvariant(revision[0]) - 'A' + 1
            : 0;
    }

    private static Version? ParseVersion(string value)
    {
        var match = VersionRegex().Match(value);
        return match.Success && Version.TryParse(match.Groups[1].Value, out var version)
            ? version
            : null;
    }

    private static int ParseDiscNumber(string value)
    {
        var match = DiscRegex().Match(value);
        return match.Success && int.TryParse(match.Groups[1].Value, out var discNumber)
            ? discNumber
            : 0;
    }

    private static string? InferLanguageFromRegion(string region) => region switch
    {
        "USA" or "World" or "Europe" or "Australia" or "United Kingdom" or "Canada" => "English",
        "Japan" => "Japanese",
        "Korea" => "Korean",
        "France" => "French",
        "Germany" => "German",
        "Spain" => "Spanish",
        "Italy" => "Italian",
        "Brazil" => "Portuguese",
        "China" or "Taiwan" => "Chinese",
        _ => null
    };

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpaceRegex();

    [GeneratedRegex(@"\s*[,;+/]\s*")]
    private static partial Regex TagSeparatorRegex();

    [GeneratedRegex(@"(?i)\b(?:rev(?:ision)?)\s*[-._]?\s*([a-z0-9]+)\b")]
    private static partial Regex RevisionRegex();

    [GeneratedRegex(@"(?i)\b(?:ver(?:sion)?|v)\s*[-._]?\s*([0-9]+(?:\.[0-9]+)*)\b")]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"(?i)\b(?:disc|disk|cd)\s*[-._]?\s*([0-9]+)\b")]
    private static partial Regex DiscRegex();
}
