using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dialogs.Avalonia;
using Dialogs.Buttons;
using TAS.Avalonia.Views;

namespace TAS.Avalonia.Services;

public class DialogService : BaseService, IDialogService
{
    public MainWindow? MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

    public async Task<bool> ShowConfirmDialogAsync(string message, string? title = null)
    {
        var dialog = new SimpleDialog { Title = title ?? string.Empty, Description = message };
        var result = await dialog.ShowAsync().ConfigureAwait(true);
        return !result.IsCancel;
    }

    public async Task ShowDialogAsync(string message, string? title = null)
    {
        var dialog = new Dialog { Title = title ?? string.Empty, Description = message };
        dialog.Buttons.AddButton(DefaultButtons.OkButton);
        await dialog.ShowAsync().ConfigureAwait(true);
    }

    public async Task<string[]?> ShowOpenFileDialogAsync(string name, params string[] extensions)
    {
        var dialog = new OpenFileDialog();
        dialog.Filters!.Add(new FileDialogFilter { Name = name, Extensions = extensions.ToList() });
        return await dialog.ShowAsync(MainWindow!).ConfigureAwait(true);
    }

    public async Task<string?> ShowSaveFileDialogAsync(string name, params string[] extensions)
    {
        var dialog = new SaveFileDialog();
        dialog.Filters!.Add(new FileDialogFilter { Name = name, Extensions = extensions.ToList() });
        return await dialog.ShowAsync(MainWindow!).ConfigureAwait(true);
    }
}
