namespace PlayBuilder.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync(
        string title,
        string initialPath,
        CancellationToken cancellationToken = default);
}
