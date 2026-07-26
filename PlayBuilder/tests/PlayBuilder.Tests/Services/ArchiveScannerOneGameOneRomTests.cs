using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class ArchiveScannerOneGameOneRomTests
{
    [Fact]
    public async Task ScanAsync_PopulatesOneGameOneRomGroupsIncludingSingletons()
    {
        var root = CreateTempDirectory();
        var factory = await CreateFactoryAsync(root);
        try
        {
            CreateRom(root, "Solo Game (USA).zip");
            CreateRom(root, "Casper (USA).zip");
            CreateRom(root, "Casper (Japan).zip");

            var scanner = new ArchiveScanner(factory);
            var result = await scanner.ScanAsync(root);

            Assert.Equal(3, result.RecognizedFileCount);
            Assert.Equal(2, result.OneGameOneRomGroups.Count);
            Assert.Contains(result.OneGameOneRomGroups, group => group.Title == "Solo Game" && group.FileCount == 1);
            Assert.Contains(result.OneGameOneRomGroups, group => group.Title == "Casper" && group.FileCount == 2);
            Assert.Single(result.DuplicateGroups);
            Assert.Equal("Casper", result.DuplicateGroups[0].Title);
        }
        finally
        {
            await factory.DisposeAsync();
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ScanAsync_KeepsMultiDiscTitlesSeparateForOneGameOneRom()
    {
        var root = CreateTempDirectory();
        var factory = await CreateFactoryAsync(root);
        try
        {
            CreateRom(root, "RPG Story (Disc 1) (USA).zip");
            CreateRom(root, "RPG Story (Disc 2) (USA).zip");

            var scanner = new ArchiveScanner(factory);
            var result = await scanner.ScanAsync(root);

            Assert.Equal(2, result.OneGameOneRomGroups.Count);
            Assert.Contains(result.OneGameOneRomGroups, group =>
                group.Title == "Rpg Story Disc 1" &&
                Assert.Single(group.Variants) == "RPG Story (Disc 1) (USA)");
            Assert.Contains(result.OneGameOneRomGroups, group =>
                group.Title == "Rpg Story Disc 2" &&
                Assert.Single(group.Variants) == "RPG Story (Disc 2) (USA)");
        }
        finally
        {
            await factory.DisposeAsync();
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public async Task ScanAsync_DoesNotGroupSameTitleAcrossDifferentSystems()
    {
        var root = CreateTempDirectory();
        var factory = await CreateFactoryAsync(root);
        try
        {
            Touch(root, "Nintendo - Super Nintendo Entertainment System", "Aladdin (USA).sfc");
            Touch(root, "Sega Genesis", "Aladdin (USA).gen");

            var scanner = new ArchiveScanner(factory);
            var result = await scanner.ScanAsync(root);

            Assert.Equal(2, result.OneGameOneRomGroups.Count);
            Assert.Empty(result.DuplicateGroups);
        }
        finally
        {
            await factory.DisposeAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ScanAsync_GroupsSameTitleWithinSameSystemAsDuplicate()
    {
        var root = CreateTempDirectory();
        var factory = await CreateFactoryAsync(root);
        try
        {
            Touch(root, "Nintendo - Super Nintendo Entertainment System", "Super Mario World (USA).sfc");
            Touch(root, "Nintendo - Super Nintendo Entertainment System", "Super Mario World (Europe).sfc");

            var scanner = new ArchiveScanner(factory);
            var result = await scanner.ScanAsync(root);

            var group = Assert.Single(result.DuplicateGroups);
            Assert.Equal("Super Mario World", group.Title);
            Assert.Equal("nintendo-super-nintendo-entertainment-system", group.SystemKey);
            Assert.Equal(2, group.FileCount);
        }
        finally
        {
            await factory.DisposeAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ScanAsync_GroupsCanonicalSystemAliasesTogether()
    {
        var root = CreateTempDirectory();
        var factory = await CreateFactoryAsync(root);
        try
        {
            Touch(root, "SNES", "EarthBound (USA).sfc");
            Touch(root, "Nintendo - Super Nintendo Entertainment System", "EarthBound (Europe).sfc");

            var scanner = new ArchiveScanner(factory);
            var result = await scanner.ScanAsync(root);

            var group = Assert.Single(result.DuplicateGroups);
            Assert.Equal("Earthbound", group.Title);
            Assert.Equal("nintendo-super-nintendo-entertainment-system", group.SystemKey);
        }
        finally
        {
            await factory.DisposeAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ScanAsync_KeepsDifferentTitlesOnSameSystemSeparate()
    {
        var root = CreateTempDirectory();
        var factory = await CreateFactoryAsync(root);
        try
        {
            Touch(root, "Sega Genesis", "Sonic the Hedgehog (USA).gen");
            Touch(root, "Sega Genesis", "Streets of Rage (USA).gen");

            var scanner = new ArchiveScanner(factory);
            var result = await scanner.ScanAsync(root);

            Assert.Equal(2, result.OneGameOneRomGroups.Count);
            Assert.Empty(result.DuplicateGroups);
        }
        finally
        {
            await factory.DisposeAsync();
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"PlayBuilderScannerTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Touch(string root, string system, string fileName)
    {
        var directory = Path.Combine(root, system);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, fileName), string.Empty);
    }

    private static void CreateRom(string root, string fileName) =>
        File.WriteAllText(Path.Combine(root, fileName), string.Empty);

    private static async Task<TestDbContextFactory> CreateFactoryAsync(string root)
    {
        var factory = new TestDbContextFactory(Path.Combine(root, "playbuilder-test.db"));
        await using var db = factory.CreateDbContext();
        await DatabaseInitializer.InitializeAsync(db);
        return factory;
    }

    private sealed class TestDbContextFactory : IDbContextFactory<PlayBuilderDbContext>, IAsyncDisposable
    {
        private readonly DbContextOptions<PlayBuilderDbContext> _options;

        public TestDbContextFactory(string databasePath)
        {
            _options = new DbContextOptionsBuilder<PlayBuilderDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
        }

        public PlayBuilderDbContext CreateDbContext() => new(_options);

        public ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();
            return ValueTask.CompletedTask;
        }
    }
}
