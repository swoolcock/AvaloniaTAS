using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using TAS.Avalonia.Services;
using TAS.Avalonia.ViewModels;
using TAS.Avalonia.Views;

namespace TAS.Avalonia;

public partial class App : Application {

    public CelesteService CelesteService = new CelesteService();
    public DialogService DialogService = new DialogService();
    public SettingsService SettingsService = new SettingsService();

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext = new MainWindowViewModel((MainWindow)desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
