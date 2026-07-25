using PlayBuilder.Models;

namespace PlayBuilder.Services;

public interface IAtlasProfileService
{
    Task<AtlasProfileStore> LoadAsync(CancellationToken cancellationToken = default);
    Task<AtlasProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default);
    Task<AtlasProfile> CreateProfileAsync(string name, CancellationToken cancellationToken = default);
    Task<AtlasProfile> RenameProfileAsync(string profileId, string name, CancellationToken cancellationToken = default);
    Task DeleteProfileAsync(string profileId, CancellationToken cancellationToken = default);
    Task<AtlasProfile> DuplicateProfileAsync(string profileId, string name, CancellationToken cancellationToken = default);
    Task<AtlasProfile> SaveProfileAsync(AtlasProfile profile, CancellationToken cancellationToken = default);
    Task<AtlasProfile> SetActiveProfileAsync(string profileId, CancellationToken cancellationToken = default);
}
