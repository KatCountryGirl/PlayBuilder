using Microsoft.EntityFrameworkCore;
using PlayBuilder.Data.Entities;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public static class LibraryGameQuery
{
    public static IQueryable<Game> Apply(IQueryable<Game> query, LibraryGameFilters filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.System))
        {
            query = query.Where(game => game.System == filters.System);
        }

        if (!string.IsNullOrWhiteSpace(filters.Region))
        {
            query = query.Where(game => game.Region == filters.Region);
        }

        if (!string.IsNullOrWhiteSpace(filters.Language))
        {
            query = query.Where(game => game.Language == filters.Language);
        }

        if (!string.IsNullOrWhiteSpace(filters.Extension))
        {
            query = query.Where(game => game.Extension == filters.Extension);
        }

        if (!string.IsNullOrWhiteSpace(filters.SearchText))
        {
            var searchPattern = $"%{EscapeLikePattern(filters.SearchText.Trim())}%";

            query = query.Where(game =>
                EF.Functions.Like(game.Title, searchPattern, @"\") ||
                EF.Functions.Like(game.SourcePath, searchPattern, @"\") ||
                EF.Functions.Like(game.RelativePath, searchPattern, @"\") ||
                EF.Functions.Like(game.System, searchPattern, @"\"));
        }

        return query;
    }

    private static string EscapeLikePattern(string value) =>
        value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
}
