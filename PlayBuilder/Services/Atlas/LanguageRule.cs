namespace PlayBuilder.Services.Atlas.Rules;

public sealed class LanguageRule : IAtlasRule
{
    public string Name => "Language priority";
    public int Priority => 100;

    public AtlasRuleResult Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        var leftValue = Best(left.Metadata.Languages, context.Options.LanguagePriority);
        var rightValue = Best(right.Metadata.Languages, context.Options.LanguagePriority);
        if (leftValue.Index == rightValue.Index) return AtlasRuleResult.Tie("Both candidates have the same language priority.");

        var description = $"{(leftValue.Index < rightValue.Index ? leftValue.Language : rightValue.Language)} is the highest configured language preference available.";
        return leftValue.Index < rightValue.Index ? AtlasRuleResult.PreferLeft(description) : AtlasRuleResult.PreferRight(description);
    }

    private static (int Index, string Language) Best(IEnumerable<string> languages, IReadOnlyList<string> priority)
    {
        var result = languages.Select(language => (Index: IndexOf(priority, language), Language: language))
            .OrderBy(item => item.Index).FirstOrDefault();
        return result.Language is null ? (priority.Count + 1, "Unknown") : result;
    }

    private static int IndexOf(IReadOnlyList<string> priority, string value)
    {
        for (var i = 0; i < priority.Count; i++)
            if (priority[i].Equals(value, StringComparison.OrdinalIgnoreCase)) return i;
        return priority.Count + 1;
    }
}
