using PlayBuilder.Models;
using PlayBuilder.Services.Atlas;

namespace PlayBuilder.Tests.Atlas;

internal static class AtlasRuleTestHelpers
{
    public static AtlasCandidate Candidate(
        string fileName = "Example Game.zip",
        string region = "Unknown",
        IEnumerable<string>? languages = null,
        int revision = 0,
        Version? version = null,
        bool isVerifiedDump = false,
        bool isBadDump = false,
        bool isBeta = false,
        bool isPrototype = false,
        bool isDemo = false,
        bool isSample = false,
        bool isHack = false,
        bool isTranslation = false,
        bool isHomebrew = false,
        bool isUnlicensed = false,
        bool isPirate = false)
    {
        return new AtlasCandidate
        {
            Metadata = new FilenameMetadata
            {
                FileName = fileName,
                Title = "Example Game",
                Region = region,
                Languages = languages?.ToList() ?? [],
                Revision = revision,
                Version = version,
                IsVerifiedDump = isVerifiedDump,
                IsBadDump = isBadDump,
                IsBeta = isBeta,
                IsPrototype = isPrototype,
                IsDemo = isDemo,
                IsSample = isSample,
                IsHack = isHack,
                IsTranslation = isTranslation,
                IsHomebrew = isHomebrew,
                IsUnlicensed = isUnlicensed,
                IsPirate = isPirate
            }
        };
    }

    public static AtlasRuleContext Context(CollectionRuleOptions? options = null) =>
        new("Example Game", options ?? new CollectionRuleOptions());
}
