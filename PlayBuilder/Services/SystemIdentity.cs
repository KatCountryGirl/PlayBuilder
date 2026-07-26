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
        ["genesis"] = "sega-genesis",
        ["mega-drive"] = "sega-genesis",
        ["sega-mega-drive"] = "sega-genesis",
        ["sega-genesis"] = "sega-genesis",
        ["nes"] = "nintendo-entertainment-system",
        ["nintendo-nes"] = "nintendo-entertainment-system",
        ["nintendo-entertainment-system"] = "nintendo-entertainment-system",
        ["gb"] = "nintendo-game-boy",
        ["game-boy"] = "nintendo-game-boy",
        ["nintendo-game-boy"] = "nintendo-game-boy",
        ["gbc"] = "nintendo-game-boy-color",
        ["game-boy-color"] = "nintendo-game-boy-color",
        ["nintendo-game-boy-color"] = "nintendo-game-boy-color",
        ["gba"] = "nintendo-game-boy-advance",
        ["game-boy-advance"] = "nintendo-game-boy-advance",
        ["nintendo-game-boy-advance"] = "nintendo-game-boy-advance"
    };

    public static string CanonicalKey(string systemName)
    {
        var normalized = Normalize(systemName);
        return string.IsNullOrWhiteSpace(normalized)
            ? "unknown"
            : Aliases.GetValueOrDefault(normalized, normalized);
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

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex RepeatedDashRegex();
}
