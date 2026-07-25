namespace PlayBuilder.Services.Atlas;

/// <summary>
/// Normalized metadata interpreted from a ROM filename.
/// </summary>
public sealed class FilenameMetadata
{
    public string FileName { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Region { get; init; } = "Unknown";

    public List<string> Languages { get; init; } = [];

    public string PrimaryLanguage => Languages.FirstOrDefault() ?? "Unknown";

    public int Revision { get; init; }

    public Version? Version { get; init; }

    public int DiscNumber { get; init; }

    public bool IsBeta { get; init; }

    public bool IsPrototype { get; init; }

    public bool IsDemo { get; init; }

    public bool IsSample { get; init; }

    public bool IsHack { get; init; }

    public bool IsTranslation { get; init; }

    public bool IsHomebrew { get; init; }

    public bool IsUnlicensed { get; init; }

    public bool IsPirate { get; init; }

    public bool IsVerifiedDump { get; init; }

    public bool IsBadDump { get; init; }

    public bool IsMultiDisc => DiscNumber > 0;

    public bool IsSpecialRelease =>
        IsBeta || IsPrototype || IsDemo || IsSample || IsHack ||
        IsTranslation || IsHomebrew || IsUnlicensed || IsPirate;

    public bool IsFallback =>
        Region.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
        PrimaryLanguage.Equals("Unknown", StringComparison.OrdinalIgnoreCase);

    public string Information =>
        $"{Region} · {PrimaryLanguage} · Rev {Revision} · Disc {DiscNumber}";
}
