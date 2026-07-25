using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Data.Entities;
using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class LibraryGameQueryTests : IAsyncDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"PlayBuilderLibraryQueryTests-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task Apply_FiltersBySystem()
    {
        await using var db = await CreateDatabaseAsync();

        var results = await LibraryGameQuery.Apply(db.Games.AsNoTracking(), new LibraryGameFilters(
                "Nintendo - Super Nintendo Entertainment System",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty))
            .Select(game => game.Title)
            .ToListAsync();

        Assert.Equal(["Chrono Trigger", "Final Fantasy III"], results.OrderBy(value => value));
    }

    [Fact]
    public async Task Apply_SearchesBySystemName()
    {
        await using var db = await CreateDatabaseAsync();

        var results = await LibraryGameQuery.Apply(db.Games.AsNoTracking(), new LibraryGameFilters(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                "Genesis"))
            .Select(game => game.Title)
            .ToListAsync();

        Assert.Equal(["Sonic the Hedgehog"], results);
    }

    [Fact]
    public async Task Apply_SearchesByFilename()
    {
        await using var db = await CreateDatabaseAsync();

        var results = await LibraryGameQuery.Apply(db.Games.AsNoTracking(), new LibraryGameFilters(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                "ff3-us"))
            .Select(game => game.Title)
            .ToListAsync();

        Assert.Equal(["Final Fantasy III"], results);
    }

    private async Task<PlayBuilderDbContext> CreateDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<PlayBuilderDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        var db = new PlayBuilderDbContext(options);
        await db.Database.EnsureCreatedAsync();

        if (!await db.Games.AnyAsync())
        {
            db.Games.AddRange(
                CreateGame("Chrono Trigger", "Nintendo - Super Nintendo Entertainment System", "chrono.zip"),
                CreateGame("Sonic the Hedgehog", "Sega Genesis", "sonic.bin"),
                CreateGame("Final Fantasy III", "Nintendo - Super Nintendo Entertainment System", "ff3-us.sfc"));

            await db.SaveChangesAsync();
        }

        return db;
    }

    private static Game CreateGame(string title, string system, string filename) =>
        new()
        {
            Title = title,
            SortTitle = title.ToLowerInvariant(),
            System = system,
            Region = "USA",
            Language = "En",
            Extension = Path.GetExtension(filename),
            SourcePath = Path.Combine(@"C:\Games", system, filename),
            RelativePath = Path.Combine(system, filename)
        };

    public async ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }

        await Task.CompletedTask;
    }
}
