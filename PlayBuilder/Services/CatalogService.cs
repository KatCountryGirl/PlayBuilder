using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class CatalogService(IDbContextFactory<PlayBuilderDbContext> dbFactory) : ICatalogService
{
    public async Task<IReadOnlyList<CatalogSystemSummary>> GetSystemsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var games = await db.Games
            .AsNoTracking()
            .Select(game => new { game.System })
            .ToListAsync(cancellationToken);

        return games
            .GroupBy(game => SystemIdentity.CanonicalKey(game.System), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var name = group
                    .Select(item => item.System)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault() ?? "Unknown";
                return new CatalogSystemSummary(name, group.Key, group.Count());
            })
            .OrderBy(system => system.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<RemoveSystemsResult> RemoveSystemsAsync(
        IEnumerable<string> systemKeys,
        CancellationToken cancellationToken = default)
    {
        var keys = systemKeys
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(SystemIdentity.CanonicalKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (keys.Count == 0)
        {
            return new RemoveSystemsResult(0, 0);
        }

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var games = await db.Games.ToListAsync(cancellationToken);
        var targets = games
            .Where(game => keys.Contains(SystemIdentity.CanonicalKey(game.System)))
            .ToList();

        if (targets.Count == 0)
        {
            return new RemoveSystemsResult(0, 0);
        }

        var removedSystems = targets
            .Select(game => SystemIdentity.CanonicalKey(game.System))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        db.Games.RemoveRange(targets);
        await db.SaveChangesAsync(cancellationToken);

        return new RemoveSystemsResult(removedSystems, targets.Count);
    }
}
