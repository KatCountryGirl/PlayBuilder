namespace PlayBuilder.Models;

public sealed class PlayBuilderSettings
{
    public string ArchivePath { get; set; } = string.Empty;

    public string LibraryPath { get; set; } = string.Empty;

    public string Frontend { get; set; } = "RetroBat";

    public bool ProtectArchive { get; set; } = true;

    public DateTimeOffset? SetupCompletedAt { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ArchivePath) &&
        !string.IsNullOrWhiteSpace(LibraryPath) &&
        !string.IsNullOrWhiteSpace(Frontend);
}
