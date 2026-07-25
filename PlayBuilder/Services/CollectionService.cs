using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data;
using PlayBuilder.Data.Entities;

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
        collection.UpdatedAt = DateTime.UtcNow;

        collection.Games.Clear();

        var favoriteIds = await db.Games
            .Where(game => game.IsFavorite)
            .Select(game => game.Id)
            .ToListAsync(cancellationToken);

        collection.Games.AddRange(
            favoriteIds.Select(gameId => new CollectionGame
            {
                GameId = gameId
            }));

        await db.SaveChangesAsync(cancellationToken);

        return collection;
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