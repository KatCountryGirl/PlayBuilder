using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class JsonScanReportService : IScanReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _reportPath;
    private readonly IDbContextFactory<PlayBuilderDbContext> _dbFactory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonScanReportService(IWebHostEnvironment environment, IDbContextFactory<PlayBuilderDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        var configRoot = Environment.GetEnvironmentVariable("PLAYBUILDER_CONFIG_PATH");
        if (string.IsNullOrWhiteSpace(configRoot))
        {
            configRoot = Path.Combine(environment.ContentRootPath, "config");
        }

        _reportPath = Path.Combine(configRoot, "latest-scan.json");
    }

    public async Task<ArchiveScanResult?> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_reportPath))
            {
                return null;
            }

            await using var stream = File.OpenRead(_reportPath);
            var result = await JsonSerializer.DeserializeAsync<ArchiveScanResult>(stream, JsonOptions, cancellationToken);
            return result is null
                ? null
                : await RepairDerivedGroupsAsync(result, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveLatestAsync(ArchiveScanResult result, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var directory = Path.GetDirectoryName(_reportPath)!;
            Directory.CreateDirectory(directory);

            var temporaryPath = _reportPath + ".tmp";
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, result, JsonOptions, cancellationToken);
            }

            File.Move(temporaryPath, _reportPath, true);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<ArchiveScanResult> RepairDerivedGroupsAsync(
        ArchiveScanResult result,
        CancellationToken cancellationToken)
    {
        if (result.OneGameOneRomGroups.All(HasSystemScopedIdentity) &&
            result.DuplicateGroups.All(HasSystemScopedIdentity))
        {
            return result;
        }

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var games = await db.Games
            .AsNoTracking()
            .Select(game => new
            {
                game.System,
                game.SourcePath
            })
            .ToListAsync(cancellationToken);

        if (games.Count == 0)
        {
            return result;
        }

        var releases = games
            .Select(game =>
            {
                var displayTitle = Path.GetFileNameWithoutExtension(game.SourcePath);
                return (
                    game.System,
                    NormalizedTitle: GameTitleIdentity.NormalizeTitle(displayTitle),
                    OneGameOneRomTitle: GameTitleIdentity.NormalizeOneGameOneRomTitle(displayTitle),
                    DisplayTitle: displayTitle);
            })
            .ToList();

        result.OneGameOneRomGroups = DuplicateGrouping.BuildOneGameOneRomGroups(
            releases.Select(release => (release.System, release.OneGameOneRomTitle, release.DisplayTitle)));
        result.DuplicateGroups = DuplicateGrouping.BuildDuplicateGroups(
            releases.Select(release => (release.System, release.NormalizedTitle, release.DisplayTitle)),
            take: 250);

        foreach (var group in result.DuplicateGroups)
        {
            group.Variants = group.Variants.Take(12).ToList();
        }

        return result;
    }

    private static bool HasSystemScopedIdentity(DuplicateGroupSummary group) =>
        !string.IsNullOrWhiteSpace(group.SystemKey) &&
        !string.IsNullOrWhiteSpace(group.GroupKey);
}
