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
            OneGameOneRomGroups =
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
            OneGameOneRomGroups =
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
            OneGameOneRomGroups =
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

    [Fact]
    public void BuildPreview_RecommendsLargeSetOfSingletonTitles()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            RecognizedFileCount = 1_000,
            OneGameOneRomGroups = Enumerable.Range(1, 1_000)
                .Select(index => new DuplicateGroupSummary
                {
                    Title = $"Game {index}",
                    FileCount = 1,
                    Variants = [$"Game {index} (USA).zip"]
                })
                .ToList()
        };

        var preview = service.BuildPreview(scan, new CollectionRuleOptions());

        Assert.Equal(1_000, preview.Selections.Count);
        Assert.Equal(1_000, preview.DuplicateGroupsReviewed);
        Assert.Equal(1_000, preview.Diagnostics.SingleRomGroups);
        Assert.Equal(0, preview.Diagnostics.MultiRomGroups);
        Assert.Equal(1_000, preview.Diagnostics.FinalRecommendations);
    }

    [Fact]
    public void BuildPreview_HandlesMixedSingletonAndDuplicateGroupsInAllGamesMode()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            RecognizedFileCount = 3,
            OneGameOneRomGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "Solo Game",
                    FileCount = 1,
                    Variants = ["Solo Game (Japan).zip"]
                },
                new DuplicateGroupSummary
                {
                    Title = "Variant Game",
                    FileCount = 2,
                    Variants = ["Variant Game (Japan).zip", "Variant Game (USA).zip"]
                }
            ]
        };

        var preview = service.BuildPreview(scan, new CollectionRuleOptions());

        Assert.Equal(2, preview.Selections.Count);
        Assert.Contains(preview.Selections, selection => selection.RecommendedVariant == "Solo Game (Japan).zip");
        Assert.Contains(preview.Selections, selection => selection.RecommendedVariant == "Variant Game (USA).zip");
        Assert.Equal(1, preview.Diagnostics.SingleRomGroups);
        Assert.Equal(1, preview.Diagnostics.MultiRomGroups);
    }

    [Fact]
    public void BuildPreview_EnglishOnlyExcludesSingletonsWithoutEnglishCandidate()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            RecognizedFileCount = 2,
            OneGameOneRomGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "English Game",
                    FileCount = 1,
                    Variants = ["English Game (USA).zip"]
                },
                new DuplicateGroupSummary
                {
                    Title = "Japan Only",
                    FileCount = 1,
                    Variants = ["Japan Only (Japan).zip"]
                }
            ]
        };

        var preview = service.BuildPreview(scan, new CollectionRuleOptions { Mode = OneGameOneRomMode.EnglishOnly });

        var selection = Assert.Single(preview.Selections);
        Assert.Equal("English Game (USA).zip", selection.RecommendedVariant);
        Assert.Equal(1, preview.GroupsExcludedByLanguage);
        Assert.Equal(1, preview.Diagnostics.GroupsExcludedByEnglishOnlyMode);
        Assert.Equal(1, preview.Diagnostics.FinalRecommendations);
    }

    private static AtlasCollectionRuleService CreateService()
    {
        IAtlasRule[] rules = [new DumpQualityRule(), new LanguageRule(), new RegionRule(), new SpecialReleaseRule(), new RevisionRule(), new VersionRule()];
        return new AtlasCollectionRuleService(new AtlasCandidateFactory(), new AtlasDecisionEngine(rules));
    }
}
