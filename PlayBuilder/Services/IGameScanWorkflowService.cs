using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface IGameScanWorkflowService
{
    Task<ArchiveScanResult> ScanAndSaveAsync(
        PlayBuilderSettings settings,
        IProgress<ArchiveScanProgress>? progress = null,
        CancellationToken cancellationToken = default,
        CatalogScanMode mode = CatalogScanMode.AddOrUpdate);
}
