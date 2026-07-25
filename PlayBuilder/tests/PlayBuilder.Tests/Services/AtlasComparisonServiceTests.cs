using Microsoft.Extensions.DependencyInjection;
using PlayBuilder.Models;
using PlayBuilder.Services;
using PlayBuilder.Services.Atlas;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Services;

public sealed class AtlasComparisonServiceTests
{
    [Fact]
    public void Compare_RecordsAgreementWhenLegacyAndAtlasChooseSameWinner()
    {
        var service = CreateService();
        var scan = CreateScan("Example Game", "Example Game (Japan) (Ja).zip", "Example Game (USA) (En).zip");

        var report = service.Compare(scan, new CollectionRuleOptions());

        var row = Assert.Single(report.Rows);
        Assert.Equal("Example Game (USA) (En).zip", row.LegacyWinner);
        Assert.Equal("Example Game (USA) (En).zip", row.AtlasWinner);
        Assert.True(row.EnginesAgree);
        Assert.Equal("Language priority", row.AtlasDecidingRule);
        Assert.Contains("highest available language preference", row.LegacyExplanation);
    }

    [Fact]
    public void Compare_RecordsDifferenceWhenLegacyAndAtlasChooseDifferentWinners()
    {
        var service = CreateService();
        var scan = CreateScan("Example Game", "Example Game (USA) [b].zip", "Example Game (Japan) [!].zip");

        var report = service.Compare(scan, new CollectionRuleOptions());

        var row = Assert.Single(report.Rows);
        Assert.Equal("Example Game (USA) [b].zip", row.LegacyWinner);
        Assert.Equal("Example Game (Japan) [!].zip", row.AtlasWinner);
        Assert.False(row.EnginesAgree);
        Assert.Equal("Dump quality", row.AtlasDecidingRule);
        Assert.Equal(1, report.DifferenceCount);
    }

    [Fact]
    public void Compare_RecordsExcludedGroupWhenBothEnginesHaveNoEnglishWinner()
    {
        var service = CreateService();
        var scan = CreateScan("Japan Only", "Japan Only (Japan) (Ja).zip", "Japan Only (Japan) (Rev 1) (Ja).zip");

        var report = service.Compare(scan, new CollectionRuleOptions { Mode = OneGameOneRomMode.EnglishOnly });

        var row = Assert.Single(report.Rows);
        Assert.Null(row.LegacyWinner);
        Assert.Null(row.AtlasWinner);
        Assert.True(row.EnginesAgree);
        Assert.Equal(string.Empty, row.AtlasDecidingRule);
        Assert.Equal(string.Empty, row.LegacyExplanation);
    }

    [Fact]
    public void Compare_ProducesFlatRowsForFutureCsvExport()
    {
        var service = CreateService();
        var scan = CreateScan("Example Game", "Example Game (USA).zip", "Example Game (Europe).zip");

        var report = service.Compare(scan, new CollectionRuleOptions());

        var row = Assert.Single(report.Rows);
        Assert.Equal("Example Game", row.Title);
        Assert.Equal("Example Game (USA).zip | Example Game (Europe).zip", row.ComparedVariants);
        Assert.Equal(1, report.ComparedGroupCount);
        Assert.Equal(report.ComparedGroupCount, report.AgreementCount + report.DifferenceCount);
    }

    [Fact]
    public void ServiceRegistration_DoesNotChangeLiveCollectionRuleEngine()
    {
        var services = new ServiceCollection();
        services.AddSingleton<CollectionRuleService>();
        services.AddSingleton<ICollectionRuleService, AtlasCollectionRuleService>();
        services.AddSingleton<IAtlasComparisonService, AtlasComparisonService>();
        services.AddSingleton<FilenameTokenizer>();
        services.AddSingleton<FilenameMetadataParser>();
        services.AddSingleton<AtlasCandidateFactory>();
        services.AddSingleton<IAtlasRule, LanguageRule>();
        services.AddSingleton<IAtlasRule, RegionRule>();
        services.AddSingleton<IAtlasRule, RevisionRule>();
        services.AddSingleton<IAtlasRule, VersionRule>();
        services.AddSingleton<IAtlasRule, SpecialReleaseRule>();
        services.AddSingleton<IAtlasRule, DumpQualityRule>();
        services.AddSingleton<AtlasDecisionEngine>();

        using var provider = services.BuildServiceProvider();

        Assert.IsType<AtlasCollectionRuleService>(provider.GetRequiredService<ICollectionRuleService>());
        Assert.IsType<AtlasComparisonService>(provider.GetRequiredService<IAtlasComparisonService>());
    }

    private static AtlasComparisonService CreateService()
    {
        IAtlasRule[] rules =
        [
            new DumpQualityRule(),
            new LanguageRule(),
            new RegionRule(),
            new SpecialReleaseRule(),
            new RevisionRule(),
            new VersionRule()
        ];

        return new AtlasComparisonService(
            new CollectionRuleService(),
            new AtlasCandidateFactory(),
            new AtlasDecisionEngine(rules));
    }

    private static ArchiveScanResult CreateScan(string title, params string[] variants)
    {
        return new ArchiveScanResult
        {
            OneGameOneRomGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = title,
                    FileCount = variants.Length,
                    Variants = variants.ToList()
                }
            ]
        };
    }

    [Fact]
    public void Compare_UsesAllOneGameOneRomGroupsIncludingSingletons()
    {
        var service = CreateService();
        var scan = new ArchiveScanResult
        {
            DuplicateGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "Variant Game",
                    FileCount = 2,
                    Variants = ["Variant Game (Japan).zip", "Variant Game (USA).zip"]
                }
            ],
            OneGameOneRomGroups =
            [
                new DuplicateGroupSummary
                {
                    Title = "Solo Game",
                    FileCount = 1,
                    Variants = ["Solo Game (USA).zip"]
                },
                new DuplicateGroupSummary
                {
                    Title = "Variant Game",
                    FileCount = 2,
                    Variants = ["Variant Game (Japan).zip", "Variant Game (USA).zip"]
                }
            ]
        };

        var report = service.Compare(scan, new CollectionRuleOptions());

        Assert.Equal(2, report.ComparedGroupCount);
        Assert.Contains(report.Rows, row => row.Title == "Solo Game" && row.AtlasWinner == "Solo Game (USA).zip");
        Assert.Contains(report.Rows, row => row.Title == "Variant Game" && row.AtlasWinner == "Variant Game (USA).zip");
    }
}
