namespace PlayBuilder.Services.Atlas;

/// <summary>
/// One structural token produced from a ROM filename.
/// </summary>
public sealed record FilenameToken(
    FilenameTokenType Type,
    string Value,
    int Position);

public enum FilenameTokenType
{
    Text,
    ParentheticalTag,
    BracketTag
}
