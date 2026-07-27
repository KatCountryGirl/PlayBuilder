using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface ICatalogService
{
    Task<IReadOnlyList<CatalogSystemSummary>> GetSystemsAsync(CancellationToken cancellationToken = default);

    Task<RemoveSystemsResult> RemoveSystemsAsync(
        IEnumerable<string> systemKeys,
        CancellationToken cancellationToken = default);
}
