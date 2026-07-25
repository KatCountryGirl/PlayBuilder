namespace PlayBuilder.Models;

public enum GameScanStatus
{
    Idle,
    Running,
    Completed,
    Cancelled,
    Failed
}

public sealed record GameScanSessionSnapshot(
    GameScanStatus Status,
    ArchiveScanProgress Progress,
    ArchiveScanResult? LastCompletedScan,
    string Message,
    bool MessageIsError,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt)
{
    public bool IsRunning => Status == GameScanStatus.Running;

    public static GameScanSessionSnapshot Idle(ArchiveScanResult? lastCompletedScan = null) =>
        new(
            GameScanStatus.Idle,
            new ArchiveScanProgress(0, 0, string.Empty, string.Empty, TimeSpan.Zero),
            lastCompletedScan,
            string.Empty,
            false,
            null,
            lastCompletedScan?.CompletedAt);
}
