using PlayBuilder.Models;

namespace PlayBuilder.Services.Atlas;

/// <summary>Read-only user preferences supplied to every Atlas rule.</summary>
public sealed class AtlasRuleContext
{
    public AtlasRuleContext(string title, CollectionRuleOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title;
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string Title { get; }
    public CollectionRuleOptions Options { get; }
}
