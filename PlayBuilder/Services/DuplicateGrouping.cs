using PlayBuilder.Data.Entities;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public static class DuplicateGrouping
{
    public static string CreateKey(string systemName, string normalizedTitle) =>
        $"{SystemIdentity.CanonicalKey(systemName)}|{normalizedTitle.Trim().ToLowerInvariant()}";

    public static Dictionary<string, int> BuildCounts(IEnumerable<Game> games) =>
        games
            .Where(game => !string.IsNullOrWhiteSpace(game.SortTitle))
            .GroupBy(game => CreateKey(game.System, game.SortTitle), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

    public static bool IsDuplicate(Game game, IReadOnlyDictionary<string, int> counts) =>
        counts.TryGetValue(CreateKey(game.System, game.SortTitle), out var count) && count > 1;

    public static List<DuplicateGroupSummary> BuildOneGameOneRomGroups(
        IEnumerable<(string SystemName, string NormalizedTitle, string DisplayTitle)> releases)
    {
        return releases
            .Where(release => !string.IsNullOrWhiteSpace(release.NormalizedTitle))
            .GroupBy(release => CreateKey(release.SystemName, release.NormalizedTitle), StringComparer.OrdinalIgnoreCase)
            .Select(group => ToSummary(group))
            .OrderBy(item => item.System, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static List<DuplicateGroupSummary> BuildDuplicateGroups(
        IEnumerable<(string SystemName, string NormalizedTitle, string DisplayTitle)> releases,
        int? take = null)
    {
        var query = releases
            .Where(release => !string.IsNullOrWhiteSpace(release.NormalizedTitle))
            .GroupBy(release => CreateKey(release.SystemName, release.NormalizedTitle), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(release => release.DisplayTitle).Distinct(StringComparer.OrdinalIgnoreCase).Skip(1).Any())
            .Select(group => ToSummary(group))
            .OrderByDescending(item => item.FileCount)
            .ThenBy(item => item.System, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase);

        return (take.HasValue ? query.Take(take.Value) : query).ToList();
    }

    private static DuplicateGroupSummary ToSummary(
        IGrouping<string, (string SystemName, string NormalizedTitle, string DisplayTitle)> group)
    {
        var first = group.First();
        return new DuplicateGroupSummary
        {
            Title = GameTitleIdentity.ToDisplayTitle(first.NormalizedTitle),
            System = first.SystemName,
            SystemKey = SystemIdentity.CanonicalKey(first.SystemName),
            GroupKey = group.Key,
            FileCount = group.Count(),
            Variants = group
                .Select(release => release.DisplayTitle)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

}
