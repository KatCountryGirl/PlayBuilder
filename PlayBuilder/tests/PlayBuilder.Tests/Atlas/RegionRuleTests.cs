using PlayBuilder.Models;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class RegionRuleTests
{
    private readonly RegionRule _rule = new();

    [Fact]
    public void Compare_LeftWins_WhenLeftHasPreferredRegion()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(region: "USA"),
            AtlasRuleTestHelpers.Candidate(region: "Europe"),
            Context(["USA", "Europe", "Japan", "Unknown"]));

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenRightHasPreferredRegion()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(region: "Europe"),
            AtlasRuleTestHelpers.Candidate(region: "USA"),
            Context(["USA", "Europe", "Japan", "Unknown"]));

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenRegionsHaveSamePriority()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(region: "World"),
            AtlasRuleTestHelpers.Candidate(region: "World"),
            Context(["USA", "World", "Europe", "Unknown"]));

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_RightWins_WhenLeftRegionIsUnknown()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(region: "Unknown"),
            AtlasRuleTestHelpers.Candidate(region: "USA"),
            Context(["USA", "World", "Europe", "Unknown"]));

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_RightWins_WhenLeftRegionIsEmpty()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(region: string.Empty),
            AtlasRuleTestHelpers.Candidate(region: "USA"),
            Context(["USA", "World", "Europe", "Unknown"]));

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenBothRegionsAreMissingFromPriority()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(region: "Mars"),
            AtlasRuleTestHelpers.Candidate(region: "Venus"),
            Context(["USA", "World", "Europe", "Unknown"]));

        Assert.Equal(0, result.Comparison);
    }

    private static PlayBuilder.Services.Atlas.AtlasRuleContext Context(List<string> priority) =>
        AtlasRuleTestHelpers.Context(new CollectionRuleOptions { RegionPriority = priority });
}
