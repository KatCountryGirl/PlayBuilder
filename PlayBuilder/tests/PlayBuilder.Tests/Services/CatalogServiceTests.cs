using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Data.Entities;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class CatalogServiceTests : IAsyncDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"PlayBuilderCatalogServiceTests-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task RemoveSystemsAsync_RemovesSelectedSystemOnly()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        db.Games.AddRange(
            CreateGame("Pilotwings", "SNES"),
            CreateGame("Sonic", "Sega Genesis"));
        await db.SaveChangesAsync();

        var service = new CatalogService(new TestDbContextFactory(_databasePath));

        var result = await service.RemoveSystemsAsync(["snes"]);

        Assert.Equal(1, result.SystemsRemoved);
        Assert.Equal(1, result.ReleasesRemoved);
        var remaining = Assert.Single(await db.Games.AsNoTracking().ToListAsync());
        Assert.Equal("Sega Genesis", remaining.System);
    }

    [Fact]
    public async Task GetSystemsAsync_ReturnsAlphabeticalCanonicalSystemSummaries()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        db.Games.AddRange(
            CreateGame("Pilotwings", "SNES"),
            CreateGame("EarthBound", "Nintendo - Super Nintendo Entertainment System"),
            CreateGame("Sonic", "Sega Genesis"));
        await db.SaveChangesAsync();

        var service = new CatalogService(new TestDbContextFactory(_databasePath));

        var systems = await service.GetSystemsAsync();

        Assert.Equal(["Nintendo - Super Nintendo Entertainment System", "Sega Genesis"], systems.Select(system => system.Name));
        Assert.Contains(systems, system => system.SystemKey == "nintendo-super-nintendo-entertainment-system" && system.ReleaseCount == 2);
    }

    private PlayBuilderDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<PlayBuilderDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options);

    private static Game CreateGame(string title, string system) =>
        new()
        {
            Title = title,
            SortTitle = title.ToLowerInvariant(),
            System = system,
            SourcePath = Path.Combine(@"C:\Games", system, $"{title}.zip"),
            RelativePath = Path.Combine(system, $"{title}.zip"),
            Extension = ".zip"
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

    private sealed class TestDbContextFactory(string databasePath) : IDbContextFactory<PlayBuilderDbContext>
    {
        public PlayBuilderDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<PlayBuilderDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options);
    }
}
