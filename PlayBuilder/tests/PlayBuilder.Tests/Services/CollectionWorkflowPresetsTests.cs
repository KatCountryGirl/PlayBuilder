using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class CollectionWorkflowPresetsTests
{
    [Fact]
    public void ApplyWorkflow_MapsEnglishOnlyToEnglishOnlyMode()
    {
        var options = new CollectionRuleOptions();

        CollectionWorkflowPresets.ApplyWorkflow(options, CollectionWorkflow.OneGameOneRomEnglishOnly);

        Assert.Equal(OneGameOneRomMode.EnglishOnly, options.Mode);
    }

    [Fact]
    public void ApplyWorkflow_MapsCustomToAllGamesModeWithRulesUntouched()
    {
        var options = new CollectionRuleOptions();

        CollectionWorkflowPresets.ApplyWorkflow(options, CollectionWorkflow.Custom);

        Assert.Equal(OneGameOneRomMode.AllGames, options.Mode);
        Assert.Contains("Language priority", options.EnabledRuleNames);
    }

    [Fact]
    public void ApplyReleasePreference_SetsDeterministicLanguageAndRegionPriority()
    {
        var options = new CollectionRuleOptions();

        CollectionWorkflowPresets.ApplyReleasePreference(options, ReleasePreferencePreset.JapaneseFirst);

        Assert.Equal("Japanese", options.LanguagePriority[0]);
        Assert.Equal("Japan", options.RegionPriority[0]);
    }

    [Fact]
    public void ApplyReleasePreference_CustomLeavesExistingPriorityOrderAlone()
    {
        var options = new CollectionRuleOptions
        {
            LanguagePriority = ["French", "English"],
            RegionPriority = ["Europe", "USA"]
        };

        CollectionWorkflowPresets.ApplyReleasePreference(options, ReleasePreferencePreset.Custom);

        Assert.Equal(["French", "English"], options.LanguagePriority);
        Assert.Equal(["Europe", "USA"], options.RegionPriority);
    }
}
