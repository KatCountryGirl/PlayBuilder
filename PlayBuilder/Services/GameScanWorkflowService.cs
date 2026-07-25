using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class GameScanWorkflowService : IGameScanWorkflowService
{
    private readonly IArchiveScanner _archiveScanner;
    private readonly IScanReportService _scanReportService;

    public GameScanWorkflowService(IArchiveScanner archiveScanner, IScanReportService scanReportService)
    {
        _archiveScanner = archiveScanner ?? throw new ArgumentNullException(nameof(archiveScanner));
        _scanReportService = scanReportService ?? throw new ArgumentNullException(nameof(scanReportService));
    }

    public async Task<ArchiveScanResult> ScanAndSaveAsync(
        PlayBuilderSettings settings,
        IProgress<ArchiveScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var result = await _archiveScanner.ScanAsync(settings.ArchivePath, progress, cancellationToken);
        await _scanReportService.SaveLatestAsync(result, cancellationToken);
        return result;
    }
}
