using System.Text.Json;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class JsonScanReportService : IScanReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _reportPath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonScanReportService(IWebHostEnvironment environment)
    {
        var configRoot = Environment.GetEnvironmentVariable("PLAYBUILDER_CONFIG_PATH");
        if (string.IsNullOrWhiteSpace(configRoot))
        {
            configRoot = Path.Combine(environment.ContentRootPath, "config");
        }

        _reportPath = Path.Combine(configRoot, "latest-scan.json");
    }

    public async Task<ArchiveScanResult?> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_reportPath))
            {
                return null;
            }

            await using var stream = File.OpenRead(_reportPath);
            return await JsonSerializer.DeserializeAsync<ArchiveScanResult>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveLatestAsync(ArchiveScanResult result, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var directory = Path.GetDirectoryName(_reportPath)!;
            Directory.CreateDirectory(directory);

            var temporaryPath = _reportPath + ".tmp";
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, result, JsonOptions, cancellationToken);
            }

            File.Move(temporaryPath, _reportPath, true);
        }
        finally
        {
            _gate.Release();
        }
    }
}
