using PlayBuilder.Models;

namespace PlayBuilder.Services.Atlas;

/// <summary>Applies ordered comparison rules. Atlas never totals or weights points.</summary>
public sealed class AtlasDecisionEngine
{
    private readonly IReadOnlyList<IAtlasRule> _rules;

    public AtlasDecisionEngine(IEnumerable<IAtlasRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.OrderBy(rule => rule.Priority).ThenBy(rule => rule.Name, StringComparer.Ordinal).ToList();
    }

    public AtlasDecision Evaluate(IEnumerable<AtlasCandidate> candidates, string title, CollectionRuleOptions options)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var eligible = candidates
            .Where(candidate => options.Mode != OneGameOneRomMode.EnglishOnly ||
                candidate.Metadata.Languages.Contains("English", StringComparer.OrdinalIgnoreCase))
            .DistinctBy(candidate => candidate.Metadata.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (eligible.Count == 0)
            throw new InvalidOperationException($"Atlas could not evaluate '{title}' because no eligible candidates remained.");

        var context = new AtlasRuleContext(title, options);
        eligible.Sort((left, right) => Compare(left, right, context));
        var winner = eligible[0];
        var reasons = BuildReasons(winner, eligible.Skip(1).FirstOrDefault(), context);

        return new AtlasDecision { Winner = winner, Candidates = eligible, Reasons = reasons };
    }

    private int Compare(AtlasCandidate left, AtlasCandidate right, AtlasRuleContext context)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Compare(left, right, context);
            if (result.Comparison != 0) return result.Comparison;
        }

        return StringComparer.OrdinalIgnoreCase.Compare(left.Metadata.FileName, right.Metadata.FileName);
    }

    private IReadOnlyList<AtlasReason> BuildReasons(AtlasCandidate winner, AtlasCandidate? runnerUp, AtlasRuleContext context)
    {
        if (runnerUp is null)
            return [new AtlasReason("Eligibility", "This was the only eligible candidate.")];

        var reasons = new List<AtlasReason>();
        foreach (var rule in _rules)
        {
            var result = rule.Compare(winner, runnerUp, context);
            if (result.Comparison < 0) reasons.Add(new AtlasReason(rule.Name, result.Description));
        }

        if (reasons.Count == 0)
            reasons.Add(new AtlasReason("Stable tie-breaker", "All configured rules tied, so the alphabetically first filename was selected."));

        return reasons;
    }
}
