using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class DumpQualityRuleTests
{
    private readonly DumpQualityRule _rule = new();

    [Fact]
    public void Compare_LeftWins_WhenLeftIsVerifiedAndRightIsNeutral()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(isVerifiedDump: true),
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenRightIsVerifiedAndLeftIsNeutral()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Candidate(isVerifiedDump: true),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_NeutralWinsOverBadDump()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Candidate(isBadDump: true),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_VerifiedWinsOverBadDump()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(isBadDump: true),
            AtlasRuleTestHelpers.Candidate(isVerifiedDump: true),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenBothHaveEquivalentEvidence()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(isVerifiedDump: true),
            AtlasRuleTestHelpers.Candidate(isVerifiedDump: true),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_Ties_WhenBothAreUnknownOrEmptyNeutralValues()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(fileName: string.Empty),
            AtlasRuleTestHelpers.Candidate(fileName: string.Empty),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }
}
