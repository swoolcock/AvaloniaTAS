using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using TAS.Avalonia.Views;
using TAS.Avalonia.ViewModels;
using TAS.Avalonia.Controls;

namespace TAS.Avalonia.Services;

public class DialogService {
    private static MainWindow MainWindow => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as MainWindow;

    public async Task ShowDialogAsync(string message, string title = null) {
        var dialog = new DialogWindowViewModel {
            Title = title ?? string.Empty,
            Description = message,
        };
        dialog.Buttons.AddOk();

        await dialog.ShowAsync().ConfigureAwait(true);
    }

    public async Task<bool> ShowConfirmDialogAsync(string message, string title = null) {
        var dialog = new DialogWindowViewModel {
            Title = title ?? string.Empty,
            Description = message
        };
        dialog.Buttons.AddOkCancel();

        var result = await dialog.ShowAsync().ConfigureAwait(true);
        return !result.IsCancel;
    }

    public async Task<string[]> ShowOpenFileDialogAsync(string title, params FilePickerFileType[] fileTypes) {
        var files = await MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = fileTypes,
        }).ConfigureAwait(true);

        return files?.Select(file => file.Path.AbsolutePath)?.ToArray();
    }

    public async Task<string> ShowSaveFileDialogAsync(string title, string defaultExtension, params FilePickerFileType[] fileTypes) {
        var file = await MainWindow.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = title,
            ShowOverwritePrompt = true,
            DefaultExtension = defaultExtension,
            FileTypeChoices = fileTypes
        }).ConfigureAwait(true);

        return file?.Path.AbsolutePath;
    }

    public async Task<int> ShowIntInputDialogAsync(int currentValue, int minValue, int maxValue, string title = null) {
        var input = new IntRangeControl(minValue, maxValue) { Name = "My Range Control", Value = currentValue };
        var dialog = new DialogWindowViewModel {
            Title = title ?? string.Empty,
            Description = "Set a new value",
            Controls = { input },
        };
        dialog.Buttons.AddOkCancel();

        var result = await dialog.ShowAsync().ConfigureAwait(true);
        return result.IsCancel ? currentValue : input.Value;
    }

    public async Task<float> ShowFloatInputDialogAsync(float currentValue, float minValue, float maxValue, string title = null) {
        var input = new FloatRangeControl(minValue, maxValue) { Name = "My Range Control", Value = currentValue };
        var dialog = new DialogWindowViewModel {
            Title = title ?? string.Empty,
            Description = "Set a new value",
            Controls = { input },
        };
        dialog.Buttons.AddOkCancel();

        var result = await dialog.ShowAsync().ConfigureAwait(true);
        return result.IsCancel ? currentValue : input.Value;
    }
}
