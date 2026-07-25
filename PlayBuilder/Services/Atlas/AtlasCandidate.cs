namespace PlayBuilder.Services.Atlas;

/// <summary>Represents one ROM candidate during Atlas evaluation.</summary>
public sealed class AtlasCandidate
{
    public required FilenameMetadata Metadata { get; init; }
    public override string ToString() => Metadata.FileName;
}
