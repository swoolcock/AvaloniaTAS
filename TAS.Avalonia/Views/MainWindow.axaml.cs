using Avalonia.Controls;
using TAS.Avalonia.Controls;

namespace TAS.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        GotFocus += (_, _) => this.FindControl<EditorControl>("editor")?.editor.Focus();
    }
}
