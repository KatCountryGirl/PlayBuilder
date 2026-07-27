using PlayBuilder.Services;

namespace PlayBuilder.Tests.Services;

public sealed class SystemIdentityTests
{
    [Theory]
    [InlineData("snes", "Nintendo - Super Nintendo Entertainment System")]
    [InlineData("super", "Nintendo - Super Nintendo Entertainment System")]
    [InlineData("nes", "Nintendo - Nintendo Entertainment System")]
    [InlineData("genesis", "Sega Mega Drive / Genesis")]
    [InlineData("megadrive", "Sega Mega Drive / Genesis")]
    [InlineData("psp", "Sony PlayStation Portable")]
    [InlineData("SONY PLAYSTATION PORTABLE", "Sony PlayStation Portable")]
    public void MatchesSearch_UsesAliasesPartialMatchingAndCaseInsensitivity(string search, string systemName)
    {
        var systemKey = SystemIdentity.CanonicalKey(systemName);

        Assert.True(SystemIdentity.MatchesSearch(systemName, systemKey, search));
    }

    [Fact]
    public void MatchesSearch_BlankSearchRestoresAllSystems()
    {
        Assert.True(SystemIdentity.MatchesSearch("Nintendo - Super Nintendo Entertainment System", "snes", ""));
        Assert.True(SystemIdentity.MatchesSearch("Sega Genesis", "genesis", "   "));
    }

    [Fact]
    public void MatchesSearch_NesDoesNotExcludeExactAliasMatches()
    {
        Assert.True(SystemIdentity.MatchesSearch("Nintendo - Nintendo Entertainment System", "nes", "nes"));
    }

    [Theory]
    [InlineData("Capcom - CPS2 Arcade", "Arcade board")]
    [InlineData("Sony PlayStation Portable", "Optical disc")]
    [InlineData("Apple - II (WOZ)", "Floppy disk")]
    [InlineData("Acorn - Atom (Tapes)", "Cassette")]
    [InlineData("Nintendo - Super Nintendo Entertainment System", "Cartridge")]
    public void ResolveMediaType_ReturnsFutureSystemArtworkCategory(string systemName, string expected)
    {
        Assert.Equal(expected, SystemIdentity.ResolveMediaType(systemName, SystemIdentity.CanonicalKey(systemName)));
    }
}
