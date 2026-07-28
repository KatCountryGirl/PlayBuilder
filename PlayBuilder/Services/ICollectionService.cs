using PlayBuilder.Data.Entities;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface ICollectionService
{
    Task<IReadOnlyList<Collection>> GetCollectionsAsync(
        CancellationToken cancellationToken = default);

    Task<Collection?> GetCollectionAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Collection> SaveFavoritesCollectionAsync(
        string name,
        string destinationPath,
        string frontend,
        IEnumerable<string>? selectedSystemKeys = null,
        IEnumerable<int>? selectedGameIds = null,
        string workflow = "",
        string releasePreference = "",
        int excludedGameCount = 0,
        int needsReviewCount = 0,
        CancellationToken cancellationToken = default);

    Task<Collection> SaveOneGameOneRomCollectionAsync(
        string name,
        string destinationPath,
        string frontend,
        IEnumerable<string> selectedFilenames,
        IEnumerable<string>? selectedSystemKeys = null,
        string workflow = "",
        string releasePreference = "",
        int excludedGameCount = 0,
        int needsReviewCount = 0,
        CancellationToken cancellationToken = default);

    Task<int> DeleteCollectionAsync(
        int collectionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogSystemSummary>> GetCatalogSystemsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FavoriteGameSearchResult>> SearchFavoriteGamesAsync(
        string searchText,
        IEnumerable<string>? selectedSystemKeys = null,
        CancellationToken cancellationToken = default);

    Task<bool> ToggleFavoriteAsync(
        int gameId,
        CancellationToken cancellationToken = default);

    Task<int> SetFavoritesAsync(
        IEnumerable<int> gameIds,
        bool isFavorite,
        CancellationToken cancellationToken = default);

    Task<int> GetFavoriteCountAsync(
        CancellationToken cancellationToken = default);

    Task<int> AddGamesAsync(
        int collectionId,
        IEnumerable<int> gameIds,
        CancellationToken cancellationToken = default);

    Task<int> RemoveGamesAsync(
        int collectionId,
        IEnumerable<int> gameIds,
        CancellationToken cancellationToken = default);

    Task<int> ReplaceGamesAsync(
        int collectionId,
        IEnumerable<int> gameIds,
        CancellationToken cancellationToken = default);

    Task<int> GetGameCountAsync(
        int collectionId,
        CancellationToken cancellationToken = default);
}
