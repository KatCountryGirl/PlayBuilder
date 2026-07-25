namespace PlayBuilder.Services.Atlas;

/// <summary>
/// Converts filenames into independent Atlas candidates.
/// </summary>
public sealed class AtlasCandidateFactory
{
    private readonly FilenameMetadataParser _parser;

    public AtlasCandidateFactory()
        : this(new FilenameMetadataParser())
    {
    }

    public AtlasCandidateFactory(FilenameMetadataParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public AtlasCandidate Create(string filename)
    {
        return new AtlasCandidate
        {
            Metadata = _parser.Parse(filename)
        };
    }

    public IReadOnlyList<AtlasCandidate> CreateMany(IEnumerable<string> filenames)
    {
        ArgumentNullException.ThrowIfNull(filenames);

        return filenames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(Create)
            .ToList();
    }
}
