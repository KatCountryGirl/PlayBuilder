using PlayBuilder.Models;
using PlayBuilder.Services;
using PlayBuilder.Services.Atlas;
using PlayBuilder.Services.Atlas.Rules;

namespace PlayBuilder.Tests.Services;

public sealed class JsonAtlasProfileServiceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"playbuilder-atlas-profiles-{Guid.NewGuid():N}");
    private readonly JsonAtlasProfileService _service;

    public JsonAtlasProfileServiceTests()
    {
        Directory.CreateDirectory(_directory);
        _service = new JsonAtlasProfileService(Path.Combine(_directory, "atlas-profiles.json"));
    }

    [Fact]
    public async Task LoadAsync_CreatesDefaultProfiles()
    {
        var store = await _service.LoadAsync();

        Assert.Contains(store.Profiles, profile => profile.Name == "No-Intro");
        Assert.Contains(store.Profiles, profile => profile.Name == "Redump");
        Assert.Contains(store.Profiles, profile => profile.Name == "Arcade");
        Assert.Contains(store.Profiles, profile => profile.Name == "Translation");
        Assert.Contains(store.Profiles, profile => profile.Name == "Personal");
        Assert.False(string.IsNullOrWhiteSpace(store.ActiveProfileId));
    }

    [Fact]
    public async Task CreateRenameDuplicateDeleteAndSetActive_UpdateProfileStore()
    {
        var created = await _service.CreateProfileAsync("Strict");
        var renamed = await _service.RenameProfileAsync(created.Id, "Strict Archive");
        var duplicate = await _service.DuplicateProfileAsync(renamed.Id, "Strict Copy");
        var active = await _service.SetActiveProfileAsync(renamed.Id);

        await _service.DeleteProfileAsync(duplicate.Id);
        var store = await _service.LoadAsync();

        Assert.Equal("Strict Archive", active.Name);
        Assert.Equal(renamed.Id, store.ActiveProfileId);
        Assert.Contains(store.Profiles, profile => profile.Name == "Strict Archive");
        Assert.DoesNotContain(store.Profiles, profile => profile.Id == duplicate.Id);
    }

    [Fact]
    public async Task SaveProfileAsync_PersistsPriorityAndRulePreferences()
    {
        var profile = await _service.CreateProfileAsync("Personal Strict");
        profile.RegionPriority = ["Japan", "USA", "Unknown"];
        profile.LanguagePriority = ["Japanese", "English", "Unknown"];
        profile.PreferNewestRevision = false;
        profile.PreferNewestVersion = false;
        profile.SetRuleEnabled("Dump quality", false);

        await _service.SaveProfileAsync(profile);
        var active = await _service.SetActiveProfileAsync(profile.Id);
        var options = active.ToOptions(OneGameOneRomMode.AllGames);

        Assert.Equal(["Japan", "USA", "Unknown"], options.RegionPriority);
        Assert.Equal(["Japanese", "English", "Unknown"], options.LanguagePriority);
        Assert.False(options.PreferNewestRevision);
        Assert.False(options.PreferNewestVersion);
        Assert.DoesNotContain("Dump quality", options.EnabledRuleNames);
    }

    [Fact]
    public async Task ActiveProfileOptions_CanDisableAtlasRuleWithoutChangingRuleOrder()
    {
        var profile = await _service.CreateProfileAsync("Ignore Dump Quality");
        profile.SetRuleEnabled("Dump quality", false);
        await _service.SaveProfileAsync(profile);

        var options = (await _service.GetActiveProfileAsync()).ToOptions(OneGameOneRomMode.AllGames);
        var decision = CreateEngine().Evaluate(
            new AtlasCandidateFactory().CreateMany(["Example Game (USA) [b].zip", "Example Game (Japan) [!].zip"]),
            "Example Game",
            options);

        Assert.Equal("Example Game (USA) [b].zip", decision.Winner.Metadata.FileName);
        Assert.Equal("Language priority", decision.DecidingReason.Rule);
    }

    [Fact]
    public async Task DeleteProfileAsync_RequiresAtLeastOneProfile()
    {
        var store = await _service.LoadAsync();
        foreach (var profile in store.Profiles.Skip(1).ToList())
        {
            await _service.DeleteProfileAsync(profile.Id);
        }

        var lastProfile = (await _service.LoadAsync()).Profiles.Single();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteProfileAsync(lastProfile.Id));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static AtlasDecisionEngine CreateEngine()
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

        return new AtlasDecisionEngine(rules);
    }
}
