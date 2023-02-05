using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using TAS.Avalonia.Controls;
using TAS.Avalonia.Services;

namespace TAS.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_GotFocus(object sender, GotFocusEventArgs e)
    {
        this.FindControl<EditorControl>("editor")?.editor.Focus();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        var celesteService = AvaloniaLocator.Current.GetService<ICelesteService>()!;
        celesteService.SendPath(string.Empty);
    }
}
