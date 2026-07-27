using System.Text.RegularExpressions;

namespace PlayBuilder.Services;

public static partial class SystemIdentity
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["snes"] = "nintendo-super-nintendo-entertainment-system",
        ["super-nintendo"] = "nintendo-super-nintendo-entertainment-system",
        ["super-nintendo-entertainment-system"] = "nintendo-super-nintendo-entertainment-system",
        ["nintendo-snes"] = "nintendo-super-nintendo-entertainment-system",
        ["nintendo-super-nintendo-entertainment-system"] = "nintendo-super-nintendo-entertainment-system",
        ["nintendo-nintendo-super-nintendo-entertainment-system"] = "nintendo-super-nintendo-entertainment-system",
        ["genesis"] = "sega-genesis",
        ["mega-drive"] = "sega-genesis",
        ["sega-mega-drive"] = "sega-genesis",
        ["sega-mega-drive-genesis"] = "sega-genesis",
        ["sega-genesis"] = "sega-genesis",
        ["nes"] = "nintendo-entertainment-system",
        ["nintendo-nes"] = "nintendo-entertainment-system",
        ["nintendo-entertainment-system"] = "nintendo-entertainment-system",
        ["nintendo-nintendo-entertainment-system"] = "nintendo-entertainment-system",
        ["gb"] = "nintendo-game-boy",
        ["game-boy"] = "nintendo-game-boy",
        ["nintendo-game-boy"] = "nintendo-game-boy",
        ["gbc"] = "nintendo-game-boy-color",
        ["game-boy-color"] = "nintendo-game-boy-color",
        ["nintendo-game-boy-color"] = "nintendo-game-boy-color",
        ["gba"] = "nintendo-game-boy-advance",
        ["game-boy-advance"] = "nintendo-game-boy-advance",
        ["nintendo-game-boy-advance"] = "nintendo-game-boy-advance",
        ["psp"] = "sony-playstation-portable",
        ["playstation-portable"] = "sony-playstation-portable",
        ["sony-playstation-portable"] = "sony-playstation-portable",
        ["megadrive"] = "sega-genesis"
    };

    private static readonly Dictionary<string, string[]> SearchAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nintendo-super-nintendo-entertainment-system"] = ["snes", "super", "super nintendo", "super nintendo entertainment system"],
        ["nintendo-entertainment-system"] = ["nes", "nintendo", "nintendo entertainment system"],
        ["sega-genesis"] = ["genesis", "mega drive", "megadrive", "sega mega drive"],
        ["sony-playstation-portable"] = ["psp", "playstation portable", "sony playstation portable"]
    };

    public static string CanonicalKey(string systemName)
    {
        var normalized = Normalize(systemName);
        return string.IsNullOrWhiteSpace(normalized)
            ? "unknown"
            : Aliases.GetValueOrDefault(normalized, normalized);
    }

    public static bool MatchesSearch(string systemName, string systemKey, string searchText)
    {
        var search = NormalizeForSearch(searchText);
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var canonicalKey = CanonicalKey(string.IsNullOrWhiteSpace(systemKey) ? systemName : systemKey);
        var searchableValues = new List<string>
        {
            NormalizeForSearch(systemName),
            NormalizeForSearch(systemKey),
            NormalizeForSearch(canonicalKey)
        };

        var currentAliases = Array.Empty<string>();
        if (SearchAliases.TryGetValue(canonicalKey, out var aliases))
        {
            currentAliases = aliases;
            searchableValues.AddRange(aliases.Select(NormalizeForSearch));
        }

        var exactAliasExists = SearchAliases.Values
            .SelectMany(value => value)
            .Select(NormalizeForSearch)
            .Any(value => value.Equals(search, StringComparison.OrdinalIgnoreCase));

        if (exactAliasExists)
        {
            return currentAliases
                .Select(NormalizeForSearch)
                .Any(value => value.Equals(search, StringComparison.OrdinalIgnoreCase));
        }

        return searchableValues.Any(value =>
            value.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    public static string ResolveMediaType(string systemName, string systemKey)
    {
        var searchable = $"{NormalizeForSearch(systemName)} {NormalizeForSearch(systemKey)}";

        if (searchable.Contains("arcade", StringComparison.OrdinalIgnoreCase))
        {
            return "Arcade board";
        }

        if (searchable.Contains("tape", StringComparison.OrdinalIgnoreCase) ||
            searchable.Contains("cassette", StringComparison.OrdinalIgnoreCase))
        {
            return "Cassette";
        }

        if (searchable.Contains("flop", StringComparison.OrdinalIgnoreCase) ||
            searchable.Contains("disk", StringComparison.OrdinalIgnoreCase) ||
            searchable.Contains("woz", StringComparison.OrdinalIgnoreCase))
        {
            return "Floppy disk";
        }

        if (searchable.Contains("playstation", StringComparison.OrdinalIgnoreCase) ||
            searchable.Contains("cd", StringComparison.OrdinalIgnoreCase) ||
            searchable.Contains("dvd", StringComparison.OrdinalIgnoreCase))
        {
            return "Optical disc";
        }

        return "Cartridge";
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var lowered = value.Trim().ToLowerInvariant()
            .Replace('&', ' ')
            .Replace('+', ' ');
        var collapsed = NonAlphaNumericRegex().Replace(lowered, "-").Trim('-');
        return RepeatedDashRegex().Replace(collapsed, "-");
    }

    private static string NormalizeForSearch(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return NonAlphaNumericRegex()
            .Replace(value.Trim().ToLowerInvariant(), " ")
            .Trim();
    }

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex RepeatedDashRegex();
}
