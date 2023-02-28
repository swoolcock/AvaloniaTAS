using Avalonia.ReactiveUI;
using ReactiveUI;
using TAS.Avalonia.ViewModels;

namespace TAS.Avalonia.Views;

public partial class InputDialogWindow : ReactiveWindow<InputDialogWindowViewModel> {
    public InputDialogWindow() {
        InitializeComponent();

        this.WhenActivated(d => {
            d(ViewModel!.ConfirmCommand.Subscribe(this.Close));
            d(ViewModel!.CancelCommand.Subscribe(this.Close));
        });
    }
}