using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class GameScanSessionService : IGameScanSessionService
{
    private readonly IGameScanWorkflowService _workflow;
    private readonly object _gate = new();
    private CancellationTokenSource? _scanCancellation;
    private GameScanSessionSnapshot _snapshot = GameScanSessionSnapshot.Idle();

    public GameScanSessionService(IGameScanWorkflowService workflow)
    {
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
    }

    public event Action? SnapshotChanged;

    public GameScanSessionSnapshot Snapshot
    {
        get
        {
            lock (_gate)
            {
                return _snapshot;
            }
        }
    }

    public Task<bool> StartScanAsync(
        PlayBuilderSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        CancellationTokenSource scanCancellation;
        lock (_gate)
        {
            if (_snapshot.IsRunning)
            {
                return Task.FromResult(false);
            }

            scanCancellation = new CancellationTokenSource();
            _scanCancellation = scanCancellation;
            _snapshot = new GameScanSessionSnapshot(
                GameScanStatus.Running,
                new ArchiveScanProgress(0, 0, settings.ArchivePath, string.Empty, TimeSpan.Zero),
                _snapshot.LastCompletedScan,
                "Scan running in the background. You can leave this page and return later.",
                false,
                DateTimeOffset.UtcNow,
                null);
        }

        NotifyChanged();
        _ = RunScanAsync(settings, scanCancellation);
        return Task.FromResult(true);
    }

    public void CancelScan()
    {
        lock (_gate)
        {
            _scanCancellation?.Cancel();
        }
    }

    private async Task RunScanAsync(PlayBuilderSettings settings, CancellationTokenSource scanCancellation)
    {
        var progress = new Progress<ArchiveScanProgress>(value =>
        {
            UpdateSnapshot(snapshot => snapshot with
            {
                Progress = value,
                Message = "Scan running in the background. You can leave this page and return later.",
                MessageIsError = false
            });
        });

        try
        {
            var result = await _workflow.ScanAndSaveAsync(settings, progress, scanCancellation.Token);
            UpdateSnapshot(snapshot => snapshot with
            {
                Status = GameScanStatus.Completed,
                LastCompletedScan = result,
                Message = "Scan complete. You can choose a collection type next.",
                MessageIsError = false,
                CompletedAt = DateTimeOffset.UtcNow
            });
        }
        catch (OperationCanceledException)
        {
            UpdateSnapshot(snapshot => snapshot with
            {
                Status = GameScanStatus.Cancelled,
                Message = "Scan cancelled. No game files were changed.",
                MessageIsError = false,
                CompletedAt = DateTimeOffset.UtcNow
            });
        }
        catch (DirectoryNotFoundException)
        {
            UpdateSnapshot(snapshot => snapshot with
            {
                Status = GameScanStatus.Failed,
                Message = "The source Game folder could not be found.",
                MessageIsError = true,
                CompletedAt = DateTimeOffset.UtcNow
            });
        }
        catch (UnauthorizedAccessException)
        {
            UpdateSnapshot(snapshot => snapshot with
            {
                Status = GameScanStatus.Failed,
                Message = "PlayBuilder does not have permission to read the source Game folder.",
                MessageIsError = true,
                CompletedAt = DateTimeOffset.UtcNow
            });
        }
        catch (Exception)
        {
            UpdateSnapshot(snapshot => snapshot with
            {
                Status = GameScanStatus.Failed,
                Message = "The scan could not finish. Your game files were not changed.",
                MessageIsError = true,
                CompletedAt = DateTimeOffset.UtcNow
            });
        }
        finally
        {
            lock (_gate)
            {
                if (ReferenceEquals(_scanCancellation, scanCancellation))
                {
                    _scanCancellation = null;
                }
            }

            scanCancellation.Dispose();
        }
    }

    private void UpdateSnapshot(Func<GameScanSessionSnapshot, GameScanSessionSnapshot> update)
    {
        lock (_gate)
        {
            _snapshot = update(_snapshot);
        }

        NotifyChanged();
    }

    private void NotifyChanged() => SnapshotChanged?.Invoke();
}
