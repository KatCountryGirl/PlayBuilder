using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Data.Entities;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class CollectionService(
    IDbContextFactory<PlayBuilderDbContext> dbFactory)
    : ICollectionService
{
    public async Task<IReadOnlyList<Collection>> GetCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Collections
            .AsNoTracking()
            .Include(collection => collection.Games)
            .ThenInclude(collectionGame => collectionGame.Game)
            .OrderBy(collection => collection.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Collection?> GetCollectionAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Collections
            .AsNoTracking()
            .Include(collection => collection.Games)
            .ThenInclude(collectionGame => collectionGame.Game)
            .SingleOrDefaultAsync(
                collection => collection.Id == id,
                cancellationToken);
    }

    public async Task<Collection> SaveFavoritesCollectionAsync(
        string name,
        string destinationPath,
        string frontend,
        IEnumerable<string>? selectedSystemKeys = null,
        IEnumerable<int>? selectedGameIds = null,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var normalizedName = string.IsNullOrWhiteSpace(name)
            ? "Favorites"
            : name.Trim();

        var collection = await db.Collections
            .Include(item => item.Games)
            .FirstOrDefaultAsync(
                item => item.Name == normalizedName,
                cancellationToken);

        if (collection is null)
        {
            collection = new Collection
            {
                Name = normalizedName,
                Type = "favorites",
                CreatedAt = DateTime.UtcNow
            };

            db.Collections.Add(collection);
        }

        collection.Type = "favorites";
        collection.DestinationPath = destinationPath?.Trim() ?? string.Empty;
        collection.Frontend = frontend?.Trim() ?? string.Empty;
        collection.RuleJson = CollectionRuleStateJson.Write(selectedSystemKeys ?? []);
        collection.UpdatedAt = DateTime.UtcNow;

        collection.Games.Clear();

        var systemKeys = NormalizeSystemKeys(selectedSystemKeys);
        var requestedIds = NormalizeGameIds(selectedGameIds);
        var favoriteQuery = requestedIds.Length == 0
            ? db.Games.Where(game => game.IsFavorite)
            : db.Games.Where(game => requestedIds.Contains(game.Id));

        var favoriteIds = (await favoriteQuery
                .Select(game => new { game.Id, game.System })
                .ToListAsync(cancellationToken))
            .Where(game => systemKeys.Count == 0 || systemKeys.Contains(SystemIdentity.CanonicalKey(game.System)))
            .Select(game => game.Id)
            .ToList();

        collection.Games.AddRange(
            favoriteIds.Select(gameId => new CollectionGame
            {
                GameId = gameId
            }));

        await db.SaveChangesAsync(cancellationToken);

        return collection;
    }

    public async Task<Collection> SaveOneGameOneRomCollectionAsync(
        string name,
        string destinationPath,
        string frontend,
        IEnumerable<string> selectedFilenames,
        IEnumerable<string>? selectedSystemKeys = null,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var normalizedName = string.IsNullOrWhiteSpace(name)
            ? "1G1R Collection"
            : name.Trim();

        var collection = await db.Collections
            .Include(item => item.Games)
            .FirstOrDefaultAsync(
                item => item.Name == normalizedName,
                cancellationToken);

        if (collection is null)
        {
            collection = new Collection
            {
                Name = normalizedName,
                Type = "1g1r",
                CreatedAt = DateTime.UtcNow
            };

            db.Collections.Add(collection);
        }

        collection.Type = "1g1r";
        collection.DestinationPath = destinationPath?.Trim() ?? string.Empty;
        collection.Frontend = frontend?.Trim() ?? string.Empty;
        collection.RuleJson = CollectionRuleStateJson.Write(selectedSystemKeys ?? []);
        collection.UpdatedAt = DateTime.UtcNow;

        var selectedTokens = selectedFilenames
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var selectedKeys = selectedTokens
            .Select(NormalizeFilenameKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var systemKeys = NormalizeSystemKeys(selectedSystemKeys);
        var gameIds = new List<int>();
        if (selectedKeys.Count > 0)
        {
            var games = await db.Games
                .Select(game => new
                {
                    game.Id,
                    game.SourcePath,
                    game.RelativePath,
                    game.Title,
                    game.System
                })
                .ToListAsync(cancellationToken);

            gameIds = games
                .Where(game =>
                    (systemKeys.Count == 0 || systemKeys.Contains(SystemIdentity.CanonicalKey(game.System))) &&
                    (MatchesSelectionToken(selectedTokens, game.System, game.Title, game.SourcePath, game.RelativePath) ||
                     selectedKeys.Contains(NormalizeFilenameKey(game.SourcePath)) ||
                     selectedKeys.Contains(NormalizeFilenameKey(game.RelativePath)) ||
                     selectedKeys.Contains(NormalizeFilenameKey(game.Title))))
                .Select(game => game.Id)
                .ToList();
        }

        collection.Games.Clear();
        collection.Games.AddRange(
            gameIds.Distinct().Select(gameId => new CollectionGame
            {
                GameId = gameId
            }));

        await db.SaveChangesAsync(cancellationToken);

        return collection;
    }

    public async Task<int> DeleteCollectionAsync(
        int collectionId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var collection = await db.Collections.FindAsync([collectionId], cancellationToken);
        if (collection is null)
        {
            return 0;
        }

        db.Collections.Remove(collection);
        await db.SaveChangesAsync(cancellationToken);
        return 1;
    }

    public async Task<IReadOnlyList<CatalogSystemSummary>> GetCatalogSystemsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var games = await db.Games
            .AsNoTracking()
            .Select(game => new { game.System })
            .ToListAsync(cancellationToken);

        return games
            .GroupBy(game => SystemIdentity.CanonicalKey(game.System), StringComparer.OrdinalIgnoreCase)
            .Select(group => new CatalogSystemSummary(
                group.Select(game => game.System).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).FirstOrDefault() ?? "Unknown",
                group.Key,
                group.Count()))
            .OrderBy(system => system.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<FavoriteGameSearchResult>> SearchFavoriteGamesAsync(
        string searchText,
        IEnumerable<string>? selectedSystemKeys = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var systemKeys = NormalizeSystemKeys(selectedSystemKeys);
        var search = searchText?.Trim() ?? string.Empty;

        var games = await db.Games
            .AsNoTracking()
            .Select(game => new
            {
                game.Id,
                game.Title,
                game.SourcePath,
                game.RelativePath,
                game.System,
                game.Region,
                game.Language,
                game.IsFavorite
            })
            .ToListAsync(cancellationToken);

        return games
            .Where(game => systemKeys.Count == 0 || systemKeys.Contains(SystemIdentity.CanonicalKey(game.System)))
            .Where(game => string.IsNullOrWhiteSpace(search) || MatchesFavoriteSearch(game.Title, game.SourcePath, game.RelativePath, game.System, search))
            .OrderBy(game => game.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(game => game.System, StringComparer.OrdinalIgnoreCase)
            .ThenBy(game => game.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Take(250)
            .Select(game => new FavoriteGameSearchResult(
                game.Id,
                game.Title,
                string.IsNullOrWhiteSpace(game.RelativePath) ? Path.GetFileName(game.SourcePath) : game.RelativePath,
                game.System,
                SystemIdentity.CanonicalKey(game.System),
                string.IsNullOrWhiteSpace(game.Region) ? "Unknown" : game.Region,
                string.IsNullOrWhiteSpace(game.Language) ? "Unknown" : game.Language,
                game.IsFavorite))
            .ToList();
    }

    public async Task<bool> ToggleFavoriteAsync(
        int gameId,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var game = await db.Games.FindAsync(
            [gameId],
            cancellationToken);

        if (game is null)
        {
            return false;
        }

        game.IsFavorite = !game.IsFavorite;

        await db.SaveChangesAsync(cancellationToken);

        return game.IsFavorite;
    }

    public async Task<int> SetFavoritesAsync(
        IEnumerable<int> gameIds,
        bool isFavorite,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeGameIds(gameIds);

        if (ids.Length == 0)
        {
            return 0;
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var games = await db.Games
            .Where(game => ids.Contains(game.Id))
            .ToListAsync(cancellationToken);

        foreach (var game in games)
        {
            game.IsFavorite = isFavorite;
        }

        await db.SaveChangesAsync(cancellationToken);

        return games.Count;
    }

    public async Task<int> GetFavoriteCountAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Games.CountAsync(
            game => game.IsFavorite,
            cancellationToken);
    }

    public async Task<int> AddGamesAsync(
        int collectionId,
        IEnumerable<int> gameIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = NormalizeGameIds(gameIds);

        if (requestedIds.Length == 0)
        {
            return 0;
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var collectionExists = await db.Collections.AnyAsync(
            collection => collection.Id == collectionId,
            cancellationToken);

        if (!collectionExists)
        {
            return 0;
        }

        // Only permit IDs that point to real games.
        var validIds = await db.Games
            .Where(game => requestedIds.Contains(game.Id))
            .Select(game => game.Id)
            .ToListAsync(cancellationToken);

        if (validIds.Count == 0)
        {
            return 0;
        }

        var existingIds = await db.CollectionGames
            .Where(item =>
                item.CollectionId == collectionId &&
                validIds.Contains(item.GameId))
            .Select(item => item.GameId)
            .ToListAsync(cancellationToken);

        var existingSet = existingIds.ToHashSet();

        var linksToAdd = validIds
            .Where(gameId => !existingSet.Contains(gameId))
            .Select(gameId => new CollectionGame
            {
                CollectionId = collectionId,
                GameId = gameId
            })
            .ToList();

        if (linksToAdd.Count == 0)
        {
            return 0;
        }

        db.CollectionGames.AddRange(linksToAdd);

        await TouchCollectionAsync(
            db,
            collectionId,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return linksToAdd.Count;
    }

    public async Task<int> RemoveGamesAsync(
        int collectionId,
        IEnumerable<int> gameIds,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeGameIds(gameIds);

        if (ids.Length == 0)
        {
            return 0;
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var linksToRemove = await db.CollectionGames
            .Where(item =>
                item.CollectionId == collectionId &&
                ids.Contains(item.GameId))
            .ToListAsync(cancellationToken);

        if (linksToRemove.Count == 0)
        {
            return 0;
        }

        db.CollectionGames.RemoveRange(linksToRemove);

        await TouchCollectionAsync(
            db,
            collectionId,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return linksToRemove.Count;
    }

    public async Task<int> ReplaceGamesAsync(
        int collectionId,
        IEnumerable<int> gameIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = NormalizeGameIds(gameIds);

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var collection = await db.Collections
            .Include(item => item.Games)
            .SingleOrDefaultAsync(
                item => item.Id == collectionId,
                cancellationToken);

        if (collection is null)
        {
            return 0;
        }

        var validIds = requestedIds.Length == 0
            ? []
            : await db.Games
                .Where(game => requestedIds.Contains(game.Id))
                .Select(game => game.Id)
                .ToListAsync(cancellationToken);

        collection.Games.Clear();

        collection.Games.AddRange(
            validIds.Select(gameId => new CollectionGame
            {
                CollectionId = collectionId,
                GameId = gameId
            }));

        collection.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return validIds.Count;
    }

    public async Task<int> GetGameCountAsync(
        int collectionId,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.CollectionGames.CountAsync(
            item => item.CollectionId == collectionId,
            cancellationToken);
    }

    private static int[] NormalizeGameIds(
        IEnumerable<int>? gameIds)
    {
        if (gameIds is null)
        {
            return [];
        }

        return gameIds
            .Where(gameId => gameId > 0)
            .Distinct()
            .ToArray();
    }

    private static HashSet<string> NormalizeSystemKeys(IEnumerable<string>? systemKeys) =>
        (systemKeys ?? [])
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Select(SystemIdentity.CanonicalKey)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string NormalizeFilenameKey(string value)
    {
        var fileName = Path.GetFileNameWithoutExtension(value);
        return string.IsNullOrWhiteSpace(fileName)
            ? value.Trim()
            : fileName.Trim();
    }

    private static bool MatchesSelectionToken(
        HashSet<string> selectedTokens,
        string system,
        string title,
        string sourcePath,
        string relativePath)
    {
        if (selectedTokens.Count == 0)
        {
            return false;
        }

        var canonicalSystem = SystemIdentity.CanonicalKey(system);
        var sourceKey = NormalizeFilenameKey(sourcePath);
        var relativeKey = NormalizeFilenameKey(relativePath);
        var titleKey = NormalizeFilenameKey(title);

        foreach (var token in selectedTokens)
        {
            var parts = token.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length != 4)
            {
                continue;
            }

            if (!SystemIdentity.CanonicalKey(parts[0]).Equals(canonicalSystem, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var selectedFileKey = NormalizeFilenameKey(parts[3]);
            if (selectedFileKey.Equals(sourceKey, StringComparison.OrdinalIgnoreCase) ||
                selectedFileKey.Equals(relativeKey, StringComparison.OrdinalIgnoreCase) ||
                selectedFileKey.Equals(titleKey, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesFavoriteSearch(
        string title,
        string sourcePath,
        string relativePath,
        string system,
        string search)
    {
        static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();

        return Normalize(title).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               Normalize(GameTitleIdentity.NormalizeTitle(title)).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               Normalize(sourcePath).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               Normalize(relativePath).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               Normalize(system).Contains(search, StringComparison.OrdinalIgnoreCase) ||
               SystemIdentity.MatchesSearch(system, SystemIdentity.CanonicalKey(system), search);
    }

    private static async Task TouchCollectionAsync(
        PlayBuilderDbContext db,
        int collectionId,
        CancellationToken cancellationToken)
    {
        var collection = await db.Collections
            .FirstOrDefaultAsync(
                item => item.Id == collectionId,
                cancellationToken);

        if (collection is not null)
        {
            collection.UpdatedAt = DateTime.UtcNow;
        }
    }
}
