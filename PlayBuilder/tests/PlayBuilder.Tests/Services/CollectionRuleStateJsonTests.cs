using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class CollectionRuleStateJsonTests
{
    [Fact]
    public void Write_PersistsWorkflowSummaryFields()
    {
        var json = CollectionRuleStateJson.Write(
            ["SNES", "snes", "Genesis"],
            "1G1R All Games",
            "English First",
            excludedGameCount: 4,
            needsReviewCount: 2);

        var state = CollectionRuleStateJson.Read(json);

        Assert.Equal(
            ["nintendo-super-nintendo-entertainment-system", "sega-genesis"],
            state.SelectedSystemKeys);
        Assert.Equal("1G1R All Games", state.Workflow);
        Assert.Equal("English First", state.ReleasePreference);
        Assert.Equal(4, state.ExcludedGameCount);
        Assert.Equal(2, state.NeedsReviewCount);
    }

    [Fact]
    public void Read_InvalidJson_ReturnsEmptyState()
    {
        var state = CollectionRuleStateJson.Read("{not-json");

        Assert.Empty(state.SelectedSystemKeys);
        Assert.Equal(string.Empty, state.Workflow);
    }
}
