using PlayBuilder.Services.Atlas;

namespace PlayBuilder.Tests.Atlas;

public sealed class FilenameMetadataParserTests
{
    private readonly FilenameMetadataParser _parser = new();

    [Fact]
    public void Parse_ReadsNoIntroStyleMetadata()
    {
        var metadata = _parser.Parse("Example Game (USA) (En,Fr) (Rev 2).zip");

        Assert.Equal("Example Game", metadata.Title);
        Assert.Equal("USA", metadata.Region);
        Assert.Equal(["English", "French"], metadata.Languages);
        Assert.Equal(2, metadata.Revision);
    }

    [Fact]
    public void Parse_ReadsDiscVersionAndDumpMarkers()
    {
        var metadata = _parser.Parse("Example Game (Europe) (Disc 2) (v1.3) [!].chd");

        Assert.Equal(2, metadata.DiscNumber);
        Assert.Equal(new Version(1, 3), metadata.Version);
        Assert.True(metadata.IsVerifiedDump);
        Assert.False(metadata.IsBadDump);
    }

    [Fact]
    public void Parse_RecognizesSpecialReleaseFlags()
    {
        var metadata = _parser.Parse("Example Game (Japan) (Proto) [T].rom");

        Assert.True(metadata.IsPrototype);
        Assert.True(metadata.IsTranslation);
        Assert.True(metadata.IsSpecialRelease);
        Assert.Equal("Japanese", metadata.PrimaryLanguage);
    }

    [Fact]
    public void Parse_RecognizesGoodToolsBadDumpMarker()
    {
        var metadata = _parser.Parse("Example Game (U) [b].rom");

        Assert.Equal("USA", metadata.Region);
        Assert.True(metadata.IsBadDump);
    }
}
