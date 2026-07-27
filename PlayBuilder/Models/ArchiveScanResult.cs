namespace PlayBuilder.Models;

public sealed class ArchiveScanResult
{
    public string ArchivePath { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public long RecognizedFileCount { get; set; }
    public long IgnoredFileCount { get; set; }
    public long TotalRecognizedBytes { get; set; }
    public bool IncludesFileSizes { get; set; }
    public List<SystemScanSummary> Systems { get; set; } = [];
    public List<ExtensionScanSummary> FileTypes { get; set; } = [];
    public List<RegionScanSummary> Regions { get; set; } = [];
    public List<MetadataScanSummary> Languages { get; set; } = [];
    public List<MetadataScanSummary> SpecialTags { get; set; } = [];
    public List<DuplicateGroupSummary> OneGameOneRomGroups { get; set; } = [];
    public List<DuplicateGroupSummary> DuplicateGroups { get; set; } = [];
    public List<MultiDiscGroupSummary> MultiDiscGroups { get; set; } = [];
    public List<string> Warnings { get; set; } = [];

    public TimeSpan Duration => CompletedAt > StartedAt
        ? CompletedAt - StartedAt
        : TimeSpan.Zero;

    public long DuplicateFileCount => DuplicateGroups.Sum(group => group.FileCount);
    public long TaggedFileCount => SpecialTags.Sum(tag => tag.FileCount);

    public int CollectionHealthScore
    {
        get
        {
            if (RecognizedFileCount <= 0) return 0;

            var duplicatePenalty = Math.Min(35d, DuplicateGroups.Count * 100d / RecognizedFileCount * 4d);
            var unknownRegion = Regions.FirstOrDefault(region => region.Region == "Unknown")?.FileCount ?? 0;
            var unknownPenalty = Math.Min(20d, unknownRegion * 100d / RecognizedFileCount * .35d);
            var specialPenalty = Math.Min(15d, TaggedFileCount * 100d / RecognizedFileCount * .2d);

            return Math.Clamp((int)Math.Round(100d - duplicatePenalty - unknownPenalty - specialPenalty), 0, 100);
        }
    }
}

public sealed class SystemScanSummary
{
    public string Name { get; set; } = string.Empty;
    public long FileCount { get; set; }
    public long TotalBytes { get; set; }
}

public sealed class ExtensionScanSummary
{
    public string Extension { get; set; } = string.Empty;
    public long FileCount { get; set; }
    public long TotalBytes { get; set; }
}

public sealed class RegionScanSummary
{
    public string Region { get; set; } = string.Empty;
    public long FileCount { get; set; }
}

public sealed class MetadataScanSummary
{
    public string Name { get; set; } = string.Empty;
    public long FileCount { get; set; }
}

public sealed class DuplicateGroupSummary
{
    public string Title { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public string SystemKey { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty;
    public long FileCount { get; set; }
    public List<string> Variants { get; set; } = [];
}

public sealed class MultiDiscGroupSummary
{
    public string Title { get; set; } = string.Empty;
    public int DiscCount { get; set; }
    public List<string> Files { get; set; } = [];
}

public sealed record ArchiveScanProgress(
    long FilesChecked,
    long RecognizedFiles,
    string CurrentFolder,
    string CurrentFile,
    TimeSpan Elapsed,
    string Phase = "Reading filenames");

public enum CatalogScanMode
{
    AddOrUpdate,
    ReplaceEntireCatalog
}

public sealed record CatalogSystemSummary(
    string Name,
    string SystemKey,
    int ReleaseCount);

public sealed record RemoveSystemsResult(
    int SystemsRemoved,
    int ReleasesRemoved);
