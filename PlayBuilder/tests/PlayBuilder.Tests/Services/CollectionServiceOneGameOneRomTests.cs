using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Data.Entities;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class CollectionServiceOneGameOneRomTests : IAsyncDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"PlayBuilderCollection-{Guid.NewGuid():N}.db");
    private readonly TestDbContextFactory _factory;

    public CollectionServiceOneGameOneRomTests()
    {
        _factory = new TestDbContextFactory(_databasePath);
    }

    [Fact]
    public async Task SaveOneGameOneRomCollectionAsync_UsesSelectedFilenamesOnly()
    {
        await using (var db = _factory.CreateDbContext())
        {
            await DatabaseInitializer.InitializeAsync(db);
            db.Games.AddRange(
                Game("Alpha", @"SNES\Alpha (USA).zip"),
                Game("Beta", @"SNES\Beta (USA).zip"),
                Game("Gamma", @"SNES\Gamma (USA).zip"));
            await db.SaveChangesAsync();
        }

        var service = new CollectionService(_factory);

        var collection = await service.SaveOneGameOneRomCollectionAsync(
            "1G1R - All Games",
            @"C:\Library\1G1R",
            "RetroBat",
            ["Alpha (USA).zip", "Gamma (USA).zip"]);

        Assert.Equal("1g1r", collection.Type);

        var saved = await service.GetCollectionAsync(collection.Id);
        Assert.NotNull(saved);
        Assert.Equal(2, saved.Games.Count);
        Assert.Contains(saved.Games, item => item.Game.Title == "Alpha");
        Assert.Contains(saved.Games, item => item.Game.Title == "Gamma");
        Assert.DoesNotContain(saved.Games, item => item.Game.Title == "Beta");
    }

    [Fact]
    public async Task SearchFavoriteGamesAsync_SearchesTitleFilenameSystemAndScope()
    {
        await using (var db = _factory.CreateDbContext())
        {
            await DatabaseInitializer.InitializeAsync(db);
            db.Games.AddRange(
                Game("Super Mario World", @"SNES\Super Mario World (USA).zip", "SNES"),
                Game("Sonic the Hedgehog", @"Genesis\Sonic the Hedgehog (USA).zip", "Genesis"),
                Game("Mario Kart", @"N64\Mario Kart 64 (USA).zip", "Nintendo 64"));
            await db.SaveChangesAsync();
        }

        var service = new CollectionService(_factory);

        var mario = await service.SearchFavoriteGamesAsync("Mario", ["snes"]);
        var genesis = await service.SearchFavoriteGamesAsync("genesis", ["sega-genesis"]);

        var result = Assert.Single(mario);
        Assert.Equal("Super Mario World", result.Title);
        Assert.Equal("SNES", result.System);
        Assert.Equal("English", result.Language);
        Assert.Single(genesis);
    }

    [Fact]
    public async Task SaveOneGameOneRomCollectionAsync_UsesStableSelectionKeysForDuplicateFilenames()
    {
        await using (var db = _factory.CreateDbContext())
        {
            await DatabaseInitializer.InitializeAsync(db);
            db.Games.AddRange(
                Game("Shared Game", @"SNES\Shared Game (USA).zip", "SNES"),
                Game("Shared Game", @"Genesis\Shared Game (USA).zip", "Genesis"));
            await db.SaveChangesAsync();
        }

        var service = new CollectionService(_factory);

        var collection = await service.SaveOneGameOneRomCollectionAsync(
            "1G1R - All Games",
            @"C:\Library\1G1R",
            "RetroBat",
            ["sega-genesis|Genesis|Shared Game|Shared Game (USA).zip"],
            ["snes", "sega-genesis"]);

        var saved = await service.GetCollectionAsync(collection.Id);

        Assert.NotNull(saved);
        var game = Assert.Single(saved.Games);
        Assert.Equal("Genesis", game.Game.System);
    }

    [Fact]
    public async Task SearchFavoriteGamesAsync_EmptyCatalogReturnsEmptyResults()
    {
        await using (var db = _factory.CreateDbContext())
        {
            await DatabaseInitializer.InitializeAsync(db);
        }

        var service = new CollectionService(_factory);

        Assert.Empty(await service.SearchFavoriteGamesAsync("Mario", ["snes"]));
    }

    [Fact]
    public async Task FavoritesWorkflow_AddRemoveAndSaveSelectedGamesSeparatelyFromFlags()
    {
        int alphaId;
        int betaId;
        await using (var db = _factory.CreateDbContext())
        {
            await DatabaseInitializer.InitializeAsync(db);
            var alpha = Game("Alpha", @"SNES\Alpha (USA).zip");
            var beta = Game("Beta", @"SNES\Beta (USA).zip");
            db.Games.AddRange(alpha, beta);
            await db.SaveChangesAsync();
            alphaId = alpha.Id;
            betaId = beta.Id;
        }

        var service = new CollectionService(_factory);

        Assert.Equal(2, await service.SetFavoritesAsync([alphaId, betaId], true));
        Assert.Equal(2, await service.GetFavoriteCountAsync());
        Assert.Equal(1, await service.SetFavoritesAsync([betaId], false));
        Assert.Equal(1, await service.GetFavoriteCountAsync());

        var collection = await service.SaveFavoritesCollectionAsync(
            "My Favorites",
            @"C:\Library\Favorites",
            "RetroBat",
            ["snes"],
            [alphaId]);

        var saved = await service.GetCollectionAsync(collection.Id);

        Assert.NotNull(saved);
        Assert.Equal("favorites", saved.Type);
        Assert.Single(saved.Games);
        Assert.Equal("Alpha", saved.Games[0].Game.Title);
    }

    private static Game Game(string title, string relativePath, string system = "SNES") => new()
    {
        Title = title,
        SortTitle = title.ToLowerInvariant(),
        System = system,
        Region = "USA",
        Language = "English",
        Extension = ".zip",
        SourcePath = Path.Combine(@"R:\Roms", relativePath),
        RelativePath = relativePath
    };

    public ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }

        return ValueTask.CompletedTask;
    }

    private sealed class TestDbContextFactory : IDbContextFactory<PlayBuilderDbContext>
    {
        private readonly DbContextOptions<PlayBuilderDbContext> _options;

        public TestDbContextFactory(string databasePath)
        {
            _options = new DbContextOptionsBuilder<PlayBuilderDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
        }

        public PlayBuilderDbContext CreateDbContext() => new(_options);
    }
}
