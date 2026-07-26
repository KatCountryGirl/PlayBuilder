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

    private static Game Game(string title, string relativePath) => new()
    {
        Title = title,
        SortTitle = title.ToLowerInvariant(),
        System = "SNES",
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
