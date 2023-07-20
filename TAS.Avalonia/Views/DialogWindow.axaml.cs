using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using TAS.Avalonia.Controls;
using TAS.Avalonia.ViewModels;

namespace TAS.Avalonia.Views;

public partial class DialogWindow : Window {
    public DialogButton ResultButton { get; private set; }

    public DialogWindow() {
        this.InitializeComponent();

        if (Owner == null && WindowStartupLocation == WindowStartupLocation.CenterOwner) {
            Window owner = null;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                owner = lifetime.Windows.FirstOrDefault(w => w.IsActive);

            Owner = owner;
            if (owner != null)
                this.Icon = owner.Icon;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
            Close();
    }

    public DialogWindow(DialogWindowViewModel viewModel) : this() {
        DataContext = viewModel;
        var sub = viewModel.Buttons.Clicked
          .Where(b => b.CloseAfterClick)
          .Subscribe(args => {
              ResultButton = args.Button;
              Close();
          });
        Closed += (sender, args) => {
            sub?.Dispose();
            if (ResultButton == null)
                ResultButton = viewModel.Buttons.CancelButton;
        };
    }
}
