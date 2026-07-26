using System.Text.RegularExpressions;

namespace PlayBuilder.Services;

public static partial class GameTitleIdentity
{
    public static string NormalizeTitle(string fileNameWithoutExtension)
    {
        var withoutDisc = DiscRegex().Replace(fileNameWithoutExtension, " ");
        var withoutTags = ParentheticalTagRegex().Replace(withoutDisc, " ");
        var withoutRevision = RevisionRegex().Replace(withoutTags, " ");
        var normalized = NonAlphaNumericRegex().Replace(withoutRevision, " ");
        return WhitespaceRegex().Replace(normalized, " ").Trim().ToLowerInvariant();
    }

    public static string NormalizeOneGameOneRomTitle(string fileNameWithoutExtension)
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

    public static string CleanDisplayTitle(string fileNameWithoutExtension)
    {
        var value = DiscRegex().Replace(fileNameWithoutExtension, " ");
        value = ParentheticalTagRegex().Replace(value, " ");
        value = RevisionRegex().Replace(value, " ");
        value = WhitespaceRegex().Replace(value, " ").Trim(' ', '-', '_', '.');
        return string.IsNullOrWhiteSpace(value) ? fileNameWithoutExtension.Trim() : value;
    }

    public static string ToDisplayTitle(string normalizedTitle)
    {
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return "Unknown title";
        }

        return string.Join(' ', normalizedTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    [GeneratedRegex(@"(?i)(?:\(|\[|\b)(?:disc|disk|cd)\s*[-_ ]*0*([1-9][0-9]*)(?:\)|\]|\b)", RegexOptions.Compiled)]
    private static partial Regex DiscRegex();

    [GeneratedRegex(@"\(([^()]*)\)", RegexOptions.Compiled)]
    private static partial Regex ParentheticalTagRegex();

    [GeneratedRegex(@"(?i)\b(?:rev(?:ision)?|ver(?:sion)?|v)\s*[a-z0-9.]+\b", RegexOptions.Compiled)]
    private static partial Regex RevisionRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
