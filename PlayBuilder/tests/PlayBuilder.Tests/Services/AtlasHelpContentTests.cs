using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class AtlasHelpContentTests
{
    [Fact]
    public void HelpContent_ExistsForEveryNavigablePage()
    {
        var pages = new[]
        {
            "dashboard",
            "scan",
            "library",
            "collection-rules",
            "build",
            "tools",
            "downloads",
            "metadata",
            "advanced",
            "settings"
        };

        foreach (var page in pages)
        {
            var help = AtlasHelpContent.Get(page);

            Assert.False(string.IsNullOrWhiteSpace(help.Purpose));
            Assert.NotEmpty(help.MainControls);
            Assert.False(string.IsNullOrWhiteSpace(help.NextStep));
            Assert.False(string.IsNullOrWhiteSpace(help.Safety));
            Assert.NotEmpty(help.CommonQuestions);
        }
    }

    [Fact]
    public void CollectionHelp_ContainsRequiredTopics()
    {
        var text = Flatten(AtlasHelpContent.Get("collection-rules"));

        Assert.Contains("1G1R means one selected ROM per unique game", text);
        Assert.Contains("All Games", text);
        Assert.Contains("English Only", text);
        Assert.Contains("Language priority", text);
        Assert.Contains("Region priority", text);
        Assert.Contains("Needs Review", text);
        Assert.Contains("Extra Versions", text);
        Assert.Contains("deterministic rules", text);
        Assert.Contains("Advanced rule switches", text);
        Assert.Contains("Atlas Profiles", text);
        Assert.Contains("Unchecked games", text);
    }

    [Fact]
    public void ScanHelp_ContainsRequiredTopics()
    {
        var text = Flatten(AtlasHelpContent.Get("scan"));

        Assert.Contains("Source folder", text);
        Assert.Contains("Destination folder", text);
        Assert.Contains("Frontend", text);
        Assert.Contains("read-only", text);
        Assert.Contains("Rescan", text);
        Assert.Contains("ROM count", text);
        Assert.Contains("1G1R group count", text);
    }

    private static string Flatten(AtlasHelpPage page) =>
        string.Join(" ", [page.Purpose, page.NextStep, page.Safety, .. page.MainControls, .. page.CommonQuestions]);
}
