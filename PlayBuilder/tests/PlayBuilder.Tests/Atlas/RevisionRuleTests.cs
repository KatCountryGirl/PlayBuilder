using PlayBuilder.Models;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class RevisionRuleTests
{
    private readonly RevisionRule _rule = new();

    [Fact]
    public void Compare_LeftWins_WhenLeftRevisionIsNewer()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(revision: 2),
            AtlasRuleTestHelpers.Candidate(revision: 1),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenRightRevisionIsNewer()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(revision: 1),
            AtlasRuleTestHelpers.Candidate(revision: 2),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenRevisionsMatch()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(revision: 2),
            AtlasRuleTestHelpers.Candidate(revision: 2),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_LeftWins_WhenRightRevisionIsUnknown()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(revision: 1),
            AtlasRuleTestHelpers.Candidate(revision: 0),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenLeftRevisionIsUnknownOrEmpty()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(revision: 0),
            AtlasRuleTestHelpers.Candidate(revision: 1),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenOptionIsDisabled()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(revision: 2),
            AtlasRuleTestHelpers.Candidate(revision: 1),
            AtlasRuleTestHelpers.Context(new CollectionRuleOptions { PreferNewestRevision = false }));

        Assert.Equal(0, result.Comparison);
    }
}
