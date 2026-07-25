using PlayBuilder.Models;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class VersionRuleTests
{
    private readonly VersionRule _rule = new();

    [Fact]
    public void Compare_LeftWins_WhenLeftVersionIsNewer()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 2)),
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 1)),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenRightVersionIsNewer()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 1)),
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 2)),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenVersionsMatch()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: new Version(2, 0)),
            AtlasRuleTestHelpers.Candidate(version: new Version(2, 0)),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_LeftWins_WhenRightVersionIsUnknown()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 0)),
            AtlasRuleTestHelpers.Candidate(version: null),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenLeftVersionIsUnknownOrEmpty()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: null),
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 0)),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenBothVersionsAreUnknownOrEmpty()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: null),
            AtlasRuleTestHelpers.Candidate(version: null),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_Ties_WhenOptionIsDisabled()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(version: new Version(2, 0)),
            AtlasRuleTestHelpers.Candidate(version: new Version(1, 0)),
            AtlasRuleTestHelpers.Context(new CollectionRuleOptions { PreferNewestRevision = false }));

        Assert.Equal(0, result.Comparison);
    }
}
