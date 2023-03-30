using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Dialogs.Avalonia;
using Dialogs.Buttons;
using TAS.Avalonia.Views;
using TAS.Avalonia.ViewModels;

namespace TAS.Avalonia.Services;

public class DialogService : BaseService, IDialogService {
    public MainWindow MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

    public async Task<bool> ShowConfirmDialogAsync(string message, string title = null) {
        var dialog = new SimpleDialog {
            Title = title ?? string.Empty, Description = message
        };
        try {
            var result = await dialog.ShowAsync().ConfigureAwait(true);
            return !result.IsCancel;
        } catch (FileNotFoundException ex) {
            // Probably a bug in Dialogs.Avalonia
            Console.Error.WriteLine($"Failed showing confirm dialog: {ex}");
            return false;
        }
    }

    public async Task ShowDialogAsync(string message, string title = null) {
        var dialog = new Dialog {
            Title = title ?? string.Empty, Description = message
        };
        dialog.Buttons.AddButton(DefaultButtons.OkButton);
        await dialog.ShowAsync().ConfigureAwait(true);
    }

    public async Task<string[]> ShowOpenFileDialogAsync(string title, params FilePickerFileType[] fileTypes) {
        var files = await MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = fileTypes,
        }).ConfigureAwait(true);

        return files.Select(file => file.Path.AbsolutePath).ToArray();
    }

    public async Task<string> ShowSaveFileDialogAsync(string title, string defaultExtension, params FilePickerFileType[] fileTypes) {
        var file = await MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = title,
            ShowOverwritePrompt = true,
            DefaultExtension = defaultExtension,
            FileTypeChoices = fileTypes
        }).ConfigureAwait(true);
        return file.Path.AbsolutePath;
    }

    public async Task<int> ShowIntInputDialogAsync(int currentValue, int minValue, int maxValue, string title = null) {
        var dialog = new InputDialogWindow();
        dialog.DataContext = new InputDialogWindowViewModel(currentValue, minValue, maxValue);
        return (int)await dialog.ShowDialog<object>(MainWindow);
    }

    public async Task<float> ShowFloatInputDialogAsync(float currentValue, float minValue, float maxValue, string title = null) {
        var dialog = new InputDialogWindow();
        dialog.DataContext = new InputDialogWindowViewModel(currentValue, minValue, maxValue);
        return (float)await dialog.ShowDialog<object>(MainWindow);
    }
}
