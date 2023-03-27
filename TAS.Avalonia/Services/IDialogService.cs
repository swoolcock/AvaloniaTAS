using Avalonia.Platform.Storage;
namespace TAS.Avalonia.Services;

public interface IDialogService : IService {
    Task<bool> ShowConfirmDialogAsync(string message, string title = null);
    Task ShowDialogAsync(string message, string title = null);
    Task<string[]> ShowOpenFileDialogAsync(string title, params FilePickerFileType[] fileTypes);
    Task<string> ShowSaveFileDialogAsync(string title, string defaultExtension, params FilePickerFileType[] fileTypes);
    Task<int> ShowIntInputDialogAsync(int currentValue, int minValue, int maxValue, string title = null);
    Task<float> ShowFloatInputDialogAsync(float currentValue, float minValue, float maxValue, string title = null);
}
