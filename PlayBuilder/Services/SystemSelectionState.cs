using PlayBuilder.Models;

namespace PlayBuilder.Services;

public sealed class SystemSelectionState
{
    private readonly List<CatalogSystemSummary> _systems = [];
    private readonly HashSet<string> _selectedKeys = new(StringComparer.OrdinalIgnoreCase);

    public string SearchText { get; set; } = string.Empty;
    public IReadOnlyList<CatalogSystemSummary> Systems => _systems;
    public IReadOnlySet<string> SelectedSystemKeys => _selectedKeys;
    public int SelectedCount => _selectedKeys.Count;

    public IReadOnlyList<CatalogSystemSummary> FilteredSystems =>
        _systems
            .Where(system => SystemIdentity.MatchesSearch(system.Name, system.SystemKey, SearchText))
            .OrderBy(system => system.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public void Load(IEnumerable<CatalogSystemSummary> systems)
    {
        _systems.Clear();
        _systems.AddRange(systems.OrderBy(system => system.Name, StringComparer.OrdinalIgnoreCase));
        SelectAll();
    }

    public void SelectAll()
    {
        _selectedKeys.Clear();
        foreach (var system in _systems)
        {
            _selectedKeys.Add(system.SystemKey);
        }
    }

    public void SelectNone() => _selectedKeys.Clear();

    public void SelectMatching()
    {
        _selectedKeys.Clear();
        foreach (var system in FilteredSystems)
        {
            _selectedKeys.Add(system.SystemKey);
        }
    }

    public void Toggle(string systemKey, bool selected)
    {
        if (selected)
        {
            _selectedKeys.Add(systemKey);
        }
        else
        {
            _selectedKeys.Remove(systemKey);
        }
    }

    public bool IsSelected(string systemKey) =>
        _selectedKeys.Contains(systemKey);
}
