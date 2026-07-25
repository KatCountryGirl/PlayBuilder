using PlayBuilder.Models;
using PlayBuilder.Services.Atlas;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Atlas;

public sealed class AtlasDecisionEngineTests
{
    private readonly AtlasCandidateFactory _factory = new();

    [Fact]
    public void Evaluate_SelectsPreferredLanguageBeforeRegion()
    {
        var options = new CollectionRuleOptions
        {
            LanguagePriority = ["English", "Japanese", "Unknown"],
            RegionPriority = ["Japan", "USA", "Unknown"]
        };

        var decision = CreateEngine().Evaluate(
            _factory.CreateMany(["Example Game (Japan) (Ja).zip", "Example Game (USA) (En).zip"]),
            "Example Game", options);

        Assert.Equal("Example Game (USA) (En).zip", decision.Winner.Metadata.FileName);
        Assert.Contains(decision.Reasons, reason => reason.Rule == "Language priority");
    }

    [Fact]
    public void Evaluate_NeverSelectsKnownBadDumpWhenBetterDumpExists()
    {
        var decision = CreateEngine().Evaluate(
            _factory.CreateMany(["Example Game (USA) [b].zip", "Example Game (Japan) [!].zip"]),
            "Example Game", new CollectionRuleOptions());

        Assert.Equal("Example Game (Japan) [!].zip", decision.Winner.Metadata.FileName);
        Assert.Contains(decision.Reasons, reason => reason.Rule == "Dump quality");
    }

    [Fact]
    public void Evaluate_UsesFilenameAsStableTieBreaker()
    {
        var decision = CreateEngine().Evaluate(
            _factory.CreateMany(["Example Game B (USA).zip", "Example Game A (USA).zip"]),
            "Example Game", new CollectionRuleOptions());

        Assert.Equal("Example Game A (USA).zip", decision.Winner.Metadata.FileName);
        Assert.Contains(decision.Reasons, reason => reason.Rule == "Stable tie-breaker");
    }

    private static AtlasDecisionEngine CreateEngine()
    {
        IAtlasRule[] rules = [new DumpQualityRule(), new LanguageRule(), new RegionRule(), new SpecialReleaseRule(), new RevisionRule(), new VersionRule()];
        return new AtlasDecisionEngine(rules);
    }
}
