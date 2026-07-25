using PlayBuilder.Models;
using PlayBuilder.Services.Atlas;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class SpecialReleaseRuleTests
{
    private readonly SpecialReleaseRule _rule = new();

    [Theory]
    [InlineData("beta")]
    [InlineData("prototype")]
    [InlineData("demo")]
    [InlineData("sample")]
    [InlineData("hack")]
    [InlineData("translation")]
    [InlineData("homebrew")]
    [InlineData("unlicensed")]
    [InlineData("pirate")]
    public void Compare_LeftWins_WhenLeftIsStandardAndRightIsSpecial(string specialReleaseType)
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(),
            SpecialCandidate(specialReleaseType),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenRightIsStandardAndLeftIsSpecial()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(isPrototype: true),
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Context());

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenBothAreStandard()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_Ties_WhenBothAreSpecialReleaseTypes()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(isBeta: true),
            AtlasRuleTestHelpers.Candidate(isHack: true),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_Ties_WhenOptionIsDisabled()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(),
            AtlasRuleTestHelpers.Candidate(isDemo: true),
            AtlasRuleTestHelpers.Context(new CollectionRuleOptions { AvoidSpecialReleases = false }));

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_Ties_WhenBothCandidatesHaveUnknownOrEmptyMetadata()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(fileName: string.Empty),
            AtlasRuleTestHelpers.Candidate(fileName: string.Empty),
            AtlasRuleTestHelpers.Context());

        Assert.Equal(0, result.Comparison);
    }

    private static AtlasCandidate SpecialCandidate(string type) => type switch
    {
        "beta" => AtlasRuleTestHelpers.Candidate(isBeta: true),
        "prototype" => AtlasRuleTestHelpers.Candidate(isPrototype: true),
        "demo" => AtlasRuleTestHelpers.Candidate(isDemo: true),
        "sample" => AtlasRuleTestHelpers.Candidate(isSample: true),
        "hack" => AtlasRuleTestHelpers.Candidate(isHack: true),
        "translation" => AtlasRuleTestHelpers.Candidate(isTranslation: true),
        "homebrew" => AtlasRuleTestHelpers.Candidate(isHomebrew: true),
        "unlicensed" => AtlasRuleTestHelpers.Candidate(isUnlicensed: true),
        "pirate" => AtlasRuleTestHelpers.Candidate(isPirate: true),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown special release type.")
    };
}
