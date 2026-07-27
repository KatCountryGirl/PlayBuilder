using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class SystemSelectionStateTests
{
    [Fact]
    public void Load_SelectsAllSystemsByDefaultAndSortsAlphabetically()
    {
        var state = CreateState();

        Assert.Equal(4, state.SelectedCount);
        Assert.Equal(["Nintendo - Nintendo Entertainment System", "Nintendo - Super Nintendo Entertainment System", "Sega Mega Drive / Genesis", "Sony PlayStation Portable"], state.Systems.Select(system => system.Name));
    }

    [Fact]
    public void Search_FiltersVisibleSystemsWithoutChangingSelection()
    {
        var state = CreateState();

        state.SearchText = "snes";

        var visible = state.FilteredSystems;
        Assert.Single(visible);
        Assert.Equal("Nintendo - Super Nintendo Entertainment System", visible[0].Name);
        Assert.Equal(4, state.SelectedCount);

        state.SearchText = "";
        Assert.Equal(4, state.FilteredSystems.Count);
        Assert.Equal(4, state.SelectedCount);
    }

    [Fact]
    public void SelectAllSelectNoneSelectMatchingAndToggle_UpdateSelection()
    {
        var state = CreateState();

        state.SelectNone();
        Assert.Equal(0, state.SelectedCount);

        state.SelectAll();
        Assert.Equal(4, state.SelectedCount);

        state.SearchText = "genesis";
        state.SelectMatching();
        Assert.Equal(1, state.SelectedCount);
        Assert.True(state.IsSelected("sega-genesis"));

        state.Toggle("sega-genesis", false);
        Assert.Equal(0, state.SelectedCount);

        state.Toggle("sony-playstation-portable", true);
        Assert.Equal(1, state.SelectedCount);
    }

    [Fact]
    public void SelectionPersistsThroughSearchFiltering()
    {
        var state = CreateState();
        state.SelectNone();
        state.Toggle("sony-playstation-portable", true);

        state.SearchText = "snes";
        Assert.Single(state.FilteredSystems);
        Assert.True(state.IsSelected("sony-playstation-portable"));

        state.SearchText = "";
        Assert.Equal(4, state.FilteredSystems.Count);
        Assert.True(state.IsSelected("sony-playstation-portable"));
    }

    private static SystemSelectionState CreateState()
    {
        var state = new SystemSelectionState();
        state.Load(
        [
            new("Sony PlayStation Portable", "sony-playstation-portable", 10),
            new("Nintendo - Super Nintendo Entertainment System", "nintendo-super-nintendo-entertainment-system", 20),
            new("Sega Mega Drive / Genesis", "sega-genesis", 30),
            new("Nintendo - Nintendo Entertainment System", "nintendo-entertainment-system", 40)
        ]);
        return state;
    }
}
