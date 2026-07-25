using PlayBuilder.Models;
using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class GameScanSessionServiceTests
{
    [Fact]
    public async Task StartScanAsync_KeepsRunningUntilWorkflowCompletes()
    {
        var workflow = new ControlledScanWorkflow();
        var service = new GameScanSessionService(workflow);

        var started = await service.StartScanAsync(CreateSettings());

        Assert.True(started);
        Assert.True(service.Snapshot.IsRunning);

        workflow.Report(new ArchiveScanProgress(10, 4, @"C:\Games", "Example.zip", TimeSpan.FromSeconds(1)));

        await WaitForAsync(() => service.Snapshot.Progress.FilesChecked == 10);
        Assert.True(service.Snapshot.IsRunning);

        workflow.Complete(CreateResult(4));

        await WaitForAsync(() => service.Snapshot.Status == GameScanStatus.Completed);
        Assert.Equal(4, service.Snapshot.LastCompletedScan?.RecognizedFileCount);
    }

    [Fact]
    public async Task Snapshot_CanBeReadFromNewObserverWhileScanRuns()
    {
        var workflow = new ControlledScanWorkflow();
        var service = new GameScanSessionService(workflow);

        await service.StartScanAsync(CreateSettings());
        workflow.Report(new ArchiveScanProgress(25, 12, @"C:\Games", "Live.zip", TimeSpan.FromSeconds(2)));

        await WaitForAsync(() => service.Snapshot.Progress.RecognizedFiles == 12);

        var newPageSnapshot = service.Snapshot;

        Assert.Equal(GameScanStatus.Running, newPageSnapshot.Status);
        Assert.Equal(25, newPageSnapshot.Progress.FilesChecked);
        Assert.Equal("Live.zip", newPageSnapshot.Progress.CurrentFile);

        workflow.Complete(CreateResult(12));
    }

    [Fact]
    public async Task StartScanAsync_PreventsConcurrentScans()
    {
        var workflow = new ControlledScanWorkflow();
        var service = new GameScanSessionService(workflow);

        var first = await service.StartScanAsync(CreateSettings());
        var second = await service.StartScanAsync(CreateSettings());

        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, workflow.StartCount);

        workflow.Complete(CreateResult(1));
    }

    [Fact]
    public async Task CancelScan_CancelsRunningWorkflow()
    {
        var workflow = new ControlledScanWorkflow();
        var service = new GameScanSessionService(workflow);

        await service.StartScanAsync(CreateSettings());
        service.CancelScan();

        await WaitForAsync(() => service.Snapshot.Status == GameScanStatus.Cancelled);

        Assert.Null(service.Snapshot.LastCompletedScan);
        Assert.Contains("cancelled", service.Snapshot.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompletedResult_RemainsAvailableAfterScanCompletes()
    {
        var workflow = new ControlledScanWorkflow();
        var service = new GameScanSessionService(workflow);

        await service.StartScanAsync(CreateSettings());
        workflow.Complete(CreateResult(8));

        await WaitForAsync(() => service.Snapshot.LastCompletedScan is not null);

        var laterSnapshot = service.Snapshot;

        Assert.Equal(GameScanStatus.Completed, laterSnapshot.Status);
        Assert.Equal(8, laterSnapshot.LastCompletedScan?.RecognizedFileCount);
    }

    private static PlayBuilderSettings CreateSettings() =>
        new()
        {
            ArchivePath = @"C:\Games",
            LibraryPath = @"C:\PlayBuilder",
            Frontend = "RetroBat",
            ProtectArchive = true,
            SetupCompletedAt = DateTimeOffset.UtcNow
        };

    private static ArchiveScanResult CreateResult(int recognizedFiles) =>
        new()
        {
            ArchivePath = @"C:\Games",
            StartedAt = DateTimeOffset.UtcNow.AddSeconds(-1),
            CompletedAt = DateTimeOffset.UtcNow,
            RecognizedFileCount = recognizedFiles
        };

    private static async Task WaitForAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!condition())
        {
            await Task.Delay(20, timeout.Token);
        }
    }

    private sealed class ControlledScanWorkflow : IGameScanWorkflowService
    {
        private readonly TaskCompletionSource<ArchiveScanResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private IProgress<ArchiveScanProgress>? _progress;

        public int StartCount { get; private set; }

        public Task<ArchiveScanResult> ScanAndSaveAsync(
            PlayBuilderSettings settings,
            IProgress<ArchiveScanProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            StartCount++;
            _progress = progress;
            cancellationToken.Register(() => _completion.TrySetCanceled(cancellationToken));
            return _completion.Task;
        }

        public void Report(ArchiveScanProgress progress) => _progress?.Report(progress);

        public void Complete(ArchiveScanResult result) => _completion.TrySetResult(result);
    }
}
