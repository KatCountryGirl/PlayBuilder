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

    private static AtlasCollectionRuleService CreateService()
    {
        IAtlasRule[] rules = [new DumpQualityRule(), new LanguageRule(), new RegionRule(), new SpecialReleaseRule(), new RevisionRule(), new VersionRule()];
        return new AtlasCollectionRuleService(new AtlasCandidateFactory(), new AtlasDecisionEngine(rules));
    }
}
