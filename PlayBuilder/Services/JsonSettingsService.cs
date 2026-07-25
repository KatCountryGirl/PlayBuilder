using System.Text.Json;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsFilePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonSettingsService(IHostEnvironment environment)
    {
        var configuredPath = Environment.GetEnvironmentVariable("PLAYBUILDER_CONFIG_PATH");
        var configDirectory = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "config")
            : configuredPath;

        _settingsFilePath = Path.Combine(configDirectory, "settings.json");
    }

    public async Task<PlayBuilderSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new PlayBuilderSettings();
            }

            await using var stream = File.OpenRead(_settingsFilePath);
            return await JsonSerializer.DeserializeAsync<PlayBuilderSettings>(
                       stream,
                       SerializerOptions,
                       cancellationToken)
                   ?? new PlayBuilderSettings();
        }
        catch (JsonException)
        {
            return new PlayBuilderSettings();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveAsync(
        PlayBuilderSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            var directory = Path.GetDirectoryName(_settingsFilePath)
                ?? throw new InvalidOperationException("The settings location is invalid.");

            Directory.CreateDirectory(directory);

            var temporaryFile = $"{_settingsFilePath}.tmp";
            await using (var stream = File.Create(temporaryFile))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    settings,
                    SerializerOptions,
                    cancellationToken);
            }

            File.Move(temporaryFile, _settingsFilePath, overwrite: true);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
