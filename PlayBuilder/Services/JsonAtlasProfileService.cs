using System.Text.Json;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class JsonAtlasProfileService : IAtlasProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _profilePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonAtlasProfileService(IHostEnvironment environment)
    {
        var configuredPath = Environment.GetEnvironmentVariable("PLAYBUILDER_CONFIG_PATH");
        var configDirectory = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(environment.ContentRootPath, "config")
            : configuredPath;

        _profilePath = Path.Combine(configDirectory, "atlas-profiles.json");
    }

    public JsonAtlasProfileService(string profilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profilePath);
        _profilePath = profilePath;
    }

    public async Task<AtlasProfileStore> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var store = await ReadStoreAsync(cancellationToken);
            await WriteStoreAsync(store, cancellationToken);
            return store;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<AtlasProfile> GetActiveProfileAsync(CancellationToken cancellationToken = default)
    {
        var store = await LoadAsync(cancellationToken);
        return Clone(store.ActiveProfile);
    }

    public async Task<AtlasProfile> CreateProfileAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await MutateAsync(store =>
        {
            var profile = CreateDefaultProfile(name.Trim());
            store.Profiles.Add(profile);
            store.ActiveProfileId = profile.Id;
            return profile;
        }, cancellationToken);
    }

    public async Task<AtlasProfile> RenameProfileAsync(string profileId, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await MutateAsync(store =>
        {
            var profile = FindProfile(store, profileId);
            profile.Name = name.Trim();
            return profile;
        }, cancellationToken);
    }

    public async Task DeleteProfileAsync(string profileId, CancellationToken cancellationToken = default)
    {
        await MutateAsync<AtlasProfile?>(store =>
        {
            if (store.Profiles.Count <= 1)
            {
                throw new InvalidOperationException("At least one Atlas profile is required.");
            }

            var profile = FindProfile(store, profileId);
            store.Profiles.Remove(profile);
            if (store.ActiveProfileId.Equals(profile.Id, StringComparison.OrdinalIgnoreCase))
            {
                store.ActiveProfileId = store.Profiles[0].Id;
            }

            return null;
        }, cancellationToken);
    }

    public async Task<AtlasProfile> DuplicateProfileAsync(string profileId, string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return await MutateAsync(store =>
        {
            var source = FindProfile(store, profileId);
            var duplicate = Clone(source);
            duplicate.Id = Guid.NewGuid().ToString("N");
            duplicate.Name = name.Trim();
            store.Profiles.Add(duplicate);
            store.ActiveProfileId = duplicate.Id;
            return duplicate;
        }, cancellationToken);
    }

    public async Task<AtlasProfile> SaveProfileAsync(AtlasProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return await MutateAsync(store =>
        {
            var existing = FindProfile(store, profile.Id);
            var index = store.Profiles.IndexOf(existing);
            store.Profiles[index] = NormalizeProfile(Clone(profile));
            return store.Profiles[index];
        }, cancellationToken);
    }

    public async Task<AtlasProfile> SetActiveProfileAsync(string profileId, CancellationToken cancellationToken = default)
    {
        return await MutateAsync(store =>
        {
            var profile = FindProfile(store, profileId);
            store.ActiveProfileId = profile.Id;
            return profile;
        }, cancellationToken);
    }

    private async Task<TResult> MutateAsync<TResult>(
        Func<AtlasProfileStore, TResult> change,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var store = await ReadStoreAsync(cancellationToken);
            var result = change(store);
            NormalizeStore(store);
            await WriteStoreAsync(store, cancellationToken);
            return result is AtlasProfile profile ? (TResult)(object)Clone(profile) : result;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<AtlasProfileStore> ReadStoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_profilePath))
        {
            return CreateDefaultStore();
        }

        try
        {
            await using var stream = File.OpenRead(_profilePath);
            var store = await JsonSerializer.DeserializeAsync<AtlasProfileStore>(
                stream,
                JsonOptions,
                cancellationToken);
            return NormalizeStore(store ?? CreateDefaultStore());
        }
        catch (JsonException)
        {
            return CreateDefaultStore();
        }
    }

    private async Task WriteStoreAsync(AtlasProfileStore store, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_profilePath)
            ?? throw new InvalidOperationException("The Atlas profile location is invalid.");
        Directory.CreateDirectory(directory);

        var temporaryPath = $"{_profilePath}.tmp";
        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, store, JsonOptions, cancellationToken);
        }

        File.Move(temporaryPath, _profilePath, overwrite: true);
    }

    private static AtlasProfileStore NormalizeStore(AtlasProfileStore store)
    {
        if (store.Profiles.Count == 0)
        {
            store = CreateDefaultStore();
        }

        for (var index = 0; index < store.Profiles.Count; index++)
        {
            store.Profiles[index] = NormalizeProfile(store.Profiles[index]);
        }

        if (string.IsNullOrWhiteSpace(store.ActiveProfileId) ||
            store.Profiles.All(profile => !profile.Id.Equals(store.ActiveProfileId, StringComparison.OrdinalIgnoreCase)))
        {
            store.ActiveProfileId = store.Profiles[0].Id;
        }

        return store;
    }

    private static AtlasProfile NormalizeProfile(AtlasProfile profile)
    {
        profile.RuleEnabled ??= new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        profile.RegionPriority ??= [];
        profile.LanguagePriority ??= [];
        profile.ReleaseTypePriority ??= [];
        profile.DumpQualityPriority ??= [];

        if (string.IsNullOrWhiteSpace(profile.Id))
        {
            profile.Id = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            profile.Name = "Personal";
        }

        var defaults = CreateDefaultProfile(profile.Name);
        if (profile.RegionPriority.Count == 0) profile.RegionPriority = defaults.RegionPriority;
        if (profile.LanguagePriority.Count == 0) profile.LanguagePriority = defaults.LanguagePriority;
        if (profile.ReleaseTypePriority.Count == 0) profile.ReleaseTypePriority = defaults.ReleaseTypePriority;
        if (profile.DumpQualityPriority.Count == 0) profile.DumpQualityPriority = defaults.DumpQualityPriority;

        foreach (var ruleName in AtlasProfile.DefaultRuleNames)
        {
            if (!profile.RuleEnabled.ContainsKey(ruleName))
            {
                profile.RuleEnabled[ruleName] = true;
            }
        }

        return profile;
    }

    private static AtlasProfileStore CreateDefaultStore()
    {
        var profiles = new[]
        {
            CreateDefaultProfile("No-Intro"),
            CreateDefaultProfile("Redump"),
            CreateDefaultProfile("Arcade"),
            CreateDefaultProfile("Translation"),
            CreateDefaultProfile("Personal")
        };

        return new AtlasProfileStore
        {
            ActiveProfileId = profiles[0].Id,
            Profiles = profiles.ToList()
        };
    }

    private static AtlasProfile CreateDefaultProfile(string name)
    {
        var options = new CollectionRuleOptions();
        return new AtlasProfile
        {
            Name = name,
            RuleEnabled = AtlasProfile.DefaultRuleNames.ToDictionary(rule => rule, _ => true, StringComparer.OrdinalIgnoreCase),
            RegionPriority = options.RegionPriority.ToList(),
            LanguagePriority = options.LanguagePriority.ToList(),
            ReleaseTypePriority =
            [
                "Standard retail",
                "Translation",
                "Homebrew",
                "Demo",
                "Beta",
                "Prototype",
                "Sample",
                "Hack",
                "Unlicensed",
                "Pirate"
            ],
            DumpQualityPriority =
            [
                "Verified good dump",
                "Neutral",
                "Known bad dump"
            ],
            PreferNewestRevision = true,
            PreferNewestVersion = true
        };
    }

    private static AtlasProfile FindProfile(AtlasProfileStore store, string profileId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        return store.Profiles.FirstOrDefault(profile => profile.Id.Equals(profileId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The Atlas profile could not be found.");
    }

    private static AtlasProfile Clone(AtlasProfile profile)
    {
        return new AtlasProfile
        {
            Id = profile.Id,
            Name = profile.Name,
            RuleEnabled = new Dictionary<string, bool>(profile.RuleEnabled, StringComparer.OrdinalIgnoreCase),
            RegionPriority = profile.RegionPriority.ToList(),
            LanguagePriority = profile.LanguagePriority.ToList(),
            ReleaseTypePriority = profile.ReleaseTypePriority.ToList(),
            DumpQualityPriority = profile.DumpQualityPriority.ToList(),
            PreferNewestRevision = profile.PreferNewestRevision,
            PreferNewestVersion = profile.PreferNewestVersion
        };
    }
}
