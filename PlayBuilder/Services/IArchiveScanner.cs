using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface IArchiveScanner
{
    Task<ArchiveScanResult> ScanAsync(
        string archivePath,
        IProgress<ArchiveScanProgress>? progress = null,
        CancellationToken cancellationToken = default,
        CatalogScanMode mode = CatalogScanMode.AddOrUpdate);
}
