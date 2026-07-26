using PlayBuilder.Services;

namespace PlayBuilder.Tests.Components;

public sealed class UserFacingTerminologyTests
{
    [Fact]
    public void ScanPage_UsesGameTerminologyAndBrowseButtons()
    {
        var source = ReadPage("Scan.razor");

        Assert.Contains("Source Game folder", source);
        Assert.Contains("BrowseSourceFolderAsync", source);
        Assert.Contains("BrowseDestinationFolderAsync", source);
        Assert.DoesNotContain("ROM", source);
    }

    [Fact]
    public void LibraryPage_UsesGameTerminology()
    {
        var source = ReadPage("Library.razor");

        Assert.DoesNotContain("ROM", source);
    }

    [Fact]
    public void LibraryPage_UsesStableSystemsPanelWithoutCollapseDrawer()
    {
        var source = ReadPage("Library.razor");
        var styles = File.ReadAllText(Path.Combine(ProjectRoot(), "Components", "Pages", "Library.razor.css"));

        Assert.Contains("systems-panel", source);
        Assert.Contains("Filter by system", source);
        Assert.DoesNotContain("ToggleSystemsPanel", source);
        Assert.DoesNotContain("_systemsCollapsed", source);
        Assert.Contains("grid-template-columns: minmax(330px, 380px) minmax(520px, 1fr) 350px", styles);
        Assert.Contains("display: block", styles);
    }

    [Fact]
    public void AskAtlasScanHelp_ExplainsFrontendAndGameTerminology()
    {
        var text = Flatten(AtlasHelpContent.Get("scan"));

        Assert.Contains("Frontend tells PlayBuilder how your finished collection should be organized", text);
        Assert.Contains("RetroBat", text);
        Assert.Contains("EmulationStation-style", text);
        Assert.Contains("Generic folder layout", text);
        Assert.Contains("destination structure, not the source files", text);
        Assert.DoesNotContain("ROM", text);
    }

    private static string ReadPage(string filename)
    {
        return File.ReadAllText(Path.Combine(ProjectRoot(), "Components", "Pages", filename));
    }

    private static string ProjectRoot()
    {
        var root = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(root, "PlayBuilder.csproj")))
        {
            root = Directory.GetParent(root)?.FullName
                ?? throw new InvalidOperationException("Could not find PlayBuilder project root.");
        }

        return root;
    }

    private static string Flatten(AtlasHelpPage page) =>
        string.Join(
            " ",
            new[] { page.Title, page.Purpose, page.NextStep, page.Safety }
                .Concat(page.MainControls)
                .Concat(page.CommonQuestions));
}
