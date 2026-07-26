using PlayBuilder.Data.Entities;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class DuplicateGroupingTests
{
    [Fact]
    public void BuildCounts_CountsDuplicateGroupsPerCanonicalSystem()
    {
        var games = new[]
        {
            CreateGame("Aladdin", "Nintendo - Super Nintendo Entertainment System"),
            CreateGame("Aladdin", "Sega Genesis"),
            CreateGame("Super Mario World", "SNES"),
            CreateGame("Super Mario World", "Nintendo - Super Nintendo Entertainment System")
        };

        var counts = DuplicateGrouping.BuildCounts(games);

        Assert.Equal(3, counts.Count);
        Assert.Equal(2, counts["nintendo-super-nintendo-entertainment-system|super mario world"]);
        Assert.Equal(1, counts["nintendo-super-nintendo-entertainment-system|aladdin"]);
        Assert.Equal(1, counts["sega-genesis|aladdin"]);
    }

    [Fact]
    public void IsDuplicate_OnlyReturnsTrueForSameSystemGroupsWithMultipleReleases()
    {
        var snesUsa = CreateGame("Super Mario World", "SNES");
        var snesEurope = CreateGame("Super Mario World", "Nintendo - Super Nintendo Entertainment System");
        var genesis = CreateGame("Super Mario World", "Sega Genesis");
        var counts = DuplicateGrouping.BuildCounts([snesUsa, snesEurope, genesis]);

        Assert.True(DuplicateGrouping.IsDuplicate(snesUsa, counts));
        Assert.True(DuplicateGrouping.IsDuplicate(snesEurope, counts));
        Assert.False(DuplicateGrouping.IsDuplicate(genesis, counts));
    }

    [Fact]
    public void BuildDuplicateGroups_DoesNotMergeSameTitleAcrossSystems()
    {
        var groups = DuplicateGrouping.BuildDuplicateGroups(
        [
            ("Nintendo - Super Nintendo Entertainment System", "aladdin", "Aladdin (USA)"),
            ("Sega Genesis", "aladdin", "Aladdin (USA)")
        ]);

        Assert.Empty(groups);
    }

    private static Game CreateGame(string title, string system) =>
        new()
        {
            Title = title,
            SortTitle = title.ToLowerInvariant(),
            System = system
        };
}
