using PlayBuilder.Models;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class LanguageRuleTests
{
    private readonly LanguageRule _rule = new();

    [Fact]
    public void Compare_LeftWins_WhenLeftHasPreferredLanguage()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: ["English"]),
            AtlasRuleTestHelpers.Candidate(languages: ["Japanese"]),
            Context(["English", "Japanese", "Unknown"]));

        Assert.True(result.Comparison < 0);
    }

    [Fact]
    public void Compare_RightWins_WhenRightHasPreferredLanguage()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: ["Japanese"]),
            AtlasRuleTestHelpers.Candidate(languages: ["English"]),
            Context(["English", "Japanese", "Unknown"]));

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenBestLanguagesHaveSamePriority()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: ["English", "French"]),
            AtlasRuleTestHelpers.Candidate(languages: ["English", "Japanese"]),
            Context(["English", "Japanese", "French", "Unknown"]));

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_RightWins_WhenLeftLanguageIsUnknown()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: ["Unknown"]),
            AtlasRuleTestHelpers.Candidate(languages: ["English"]),
            Context(["English", "Japanese", "Unknown"]));

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_RightWins_WhenLeftHasEmptyLanguageValues()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: []),
            AtlasRuleTestHelpers.Candidate(languages: ["English"]),
            Context(["English", "Japanese", "Unknown"]));

        Assert.True(result.Comparison > 0);
    }

    [Fact]
    public void Compare_Ties_WhenBothLanguagesAreMissingFromPriority()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: ["Klingon"]),
            AtlasRuleTestHelpers.Candidate(languages: ["Elvish"]),
            Context(["English", "Japanese", "Unknown"]));

        Assert.Equal(0, result.Comparison);
    }

    [Fact]
    public void Compare_UsesBestLanguage_WhenCandidateHasMultipleLanguages()
    {
        var result = _rule.Compare(
            AtlasRuleTestHelpers.Candidate(languages: ["French", "English"]),
            AtlasRuleTestHelpers.Candidate(languages: ["Japanese"]),
            Context(["English", "Japanese", "French", "Unknown"]));

        Assert.True(result.Comparison < 0);
    }

    private static PlayBuilder.Services.Atlas.AtlasRuleContext Context(List<string> priority) =>
        AtlasRuleTestHelpers.Context(new CollectionRuleOptions { LanguagePriority = priority });
}
