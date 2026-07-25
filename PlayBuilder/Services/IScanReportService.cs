using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface IScanReportService
{
    Task<ArchiveScanResult?> LoadLatestAsync(CancellationToken cancellationToken = default);
    Task SaveLatestAsync(ArchiveScanResult result, CancellationToken cancellationToken = default);
}
