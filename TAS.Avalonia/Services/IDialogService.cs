namespace TAS.Avalonia.Services;

public interface IDialogService : IService
{
    Task<bool> ShowConfirmDialogAsync(string message, string title = null);
    Task ShowDialogAsync(string message, string title = null);
    Task<string[]> ShowOpenFileDialogAsync(string name, params string[] extensions);
    Task<string> ShowSaveFileDialogAsync(string name, params string[] extensions);
}
