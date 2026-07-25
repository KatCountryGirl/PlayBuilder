using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface ISettingsService
{
    Task<PlayBuilderSettings> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(PlayBuilderSettings settings, CancellationToken cancellationToken = default);
}
