using PlayBuilder.Models;
using PlayBuilder.Services;
using PlayBuilder.Services.Atlas;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Services;

public sealed class AtlasCollectionRuleServiceTests
{
    [Fact]
    public void BuildPreview_UsesAtlasAndReturnsExplanations()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            DuplicateGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "Example Game",
                    Variants = ["Example Game (Japan) (Ja).zip", "Example Game (USA) (En).zip"]
                }
            ]
        };

        var preview = service.BuildPreview(scan, new CollectionRuleOptions());

        var selection = Assert.Single(preview.Selections);
        Assert.Equal("Atlas", preview.EngineName);
        Assert.Equal("Example Game (USA) (En).zip", selection.RecommendedVariant);
        Assert.NotEmpty(selection.DecisionReasons);
        Assert.StartsWith("Selected by Language priority:", selection.DecisionReasons[0]);
        Assert.Contains("Selected by Language priority:", selection.Reason);
    }

    [Fact]
    public void BuildPreview_EnglishOnlyExcludesGroupsWithoutEnglishCandidate()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            DuplicateGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "Japan Only",
                    Variants = ["Japan Only (Japan) (Ja).zip", "Japan Only (Japan) (Rev 1) (Ja).zip"]
                }
            ]
        };

        var preview = service.BuildPreview(scan, new CollectionRuleOptions { Mode = OneGameOneRomMode.EnglishOnly });

        Assert.Empty(preview.Selections);
        Assert.Equal(1, preview.GroupsExcludedByLanguage);
    }

    [Fact]
    public void BuildPreview_PopulatesAtlasInspectionFromDecision()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            DuplicateGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "Example Game",
                    Variants =
                    [
                        "Example Game (USA) (En) (Rev 2) [!].zip",
                        "Example Game (Europe) (En,Fr) (Rev 1).zip",
                        "Example Game (Japan) (Ja) [b].zip"
                    ]
                }
            ]
        };

        var preview = service.BuildPreview(scan, new CollectionRuleOptions());

        var selection = Assert.Single(preview.Selections);
        Assert.NotNull(selection.AtlasInspection);
        var inspection = selection.AtlasInspection;
        Assert.Equal("Example Game (USA) (En) (Rev 2) [!].zip", inspection.WinningRom);
        Assert.Equal("Example Game (Europe) (En,Fr) (Rev 1).zip", inspection.RunnerUp);
        Assert.Equal("Dump quality", inspection.DecidingRule);
        Assert.Contains(inspection.SupportingRules, rule => rule.StartsWith("Region priority:", StringComparison.Ordinal));

        Assert.Collection(inspection.Candidates,
            candidate =>
            {
                Assert.True(candidate.IsWinner);
                Assert.Equal(1, candidate.Order);
                Assert.Equal("USA", candidate.Region);
                Assert.Equal(["English"], candidate.Languages);
                Assert.Equal("Verified good dump", candidate.DumpQuality);
                Assert.Equal("Rev 2", candidate.Revision);
                Assert.Equal("None", candidate.Version);
                Assert.Equal("Standard retail", candidate.ReleaseType);
            },
            candidate =>
            {
                Assert.True(candidate.IsRunnerUp);
                Assert.Equal(2, candidate.Order);
                Assert.Equal("Europe", candidate.Region);
                Assert.Equal(["English", "French"], candidate.Languages);
                Assert.Equal("Neutral", candidate.DumpQuality);
            },
            candidate =>
            {
                Assert.Equal(3, candidate.Order);
                Assert.Equal("Japan", candidate.Region);
                Assert.Equal("Known bad dump", candidate.DumpQuality);
            });
    }

    private static AtlasCollectionRuleService CreateService()
    {
        IAtlasRule[] rules = [new DumpQualityRule(), new LanguageRule(), new RegionRule(), new SpecialReleaseRule(), new RevisionRule(), new VersionRule()];
        return new AtlasCollectionRuleService(new AtlasCandidateFactory(), new AtlasDecisionEngine(rules));
    }
}
