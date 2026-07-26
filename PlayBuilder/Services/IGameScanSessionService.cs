using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface IGameScanSessionService
{
    event Action? SnapshotChanged;

    GameScanSessionSnapshot Snapshot { get; }

    Task<bool> StartScanAsync(
        PlayBuilderSettings settings,
        CancellationToken cancellationToken = default);

    void CancelScan();
}
