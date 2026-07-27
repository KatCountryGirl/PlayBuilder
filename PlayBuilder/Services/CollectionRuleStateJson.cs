using System.Text.Json;
using PlayBuilder.Models;

namespace PlayBuilder.Services;

public static class CollectionRuleStateJson
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public static CollectionRuleState Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new CollectionRuleState();
        }

        try
        {
            return JsonSerializer.Deserialize<CollectionRuleState>(json, JsonOptions) ?? new CollectionRuleState();
        }
        catch (JsonException)
        {
            return new CollectionRuleState();
        }
    }

    public static string Write(IEnumerable<string> selectedSystemKeys)
    {
        var state = new CollectionRuleState
        {
            SelectedSystemKeys = selectedSystemKeys
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(SystemIdentity.CanonicalKey)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        return JsonSerializer.Serialize(state, JsonOptions);
    }
}
