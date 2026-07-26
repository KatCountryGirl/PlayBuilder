using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PlayBuilder.Services;

public sealed class WindowsFolderPickerService : IFolderPickerService
{
    public Task<string?> PickFolderAsync(
        string title,
        string initialPath,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<string?>(null);
        }

        var completion = new TaskCompletionSource<string?>();
        var thread = new Thread(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
#pragma warning disable CA1416
                completion.SetResult(PickFolderOnStaThread(title, initialPath));
#pragma warning restore CA1416
            }
            catch (OperationCanceledException)
            {
                completion.SetCanceled(cancellationToken);
            }
            catch (Exception)
            {
                completion.SetResult(null);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        return completion.Task;
    }

    [SupportedOSPlatform("windows")]
    private static string? PickFolderOnStaThread(string title, string initialPath)
    {
        var shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null)
        {
            return null;
        }

        object? shell = null;
        object? folder = null;

        try
        {
            shell = Activator.CreateInstance(shellType);
            if (shell is null)
            {
                return null;
            }

            dynamic shellApplication = shell;
            folder = shellApplication.BrowseForFolder(
                0,
                title,
                0x00000041,
                string.IsNullOrWhiteSpace(initialPath) ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) : initialPath);

            if (folder is null)
            {
                return null;
            }

            dynamic selectedFolder = folder;
            return selectedFolder.Self.Path as string;
        }
        finally
        {
            if (folder is not null && Marshal.IsComObject(folder))
            {
                Marshal.ReleaseComObject(folder);
            }

            if (shell is not null && Marshal.IsComObject(shell))
            {
                Marshal.ReleaseComObject(shell);
            }
        }
    }
}
