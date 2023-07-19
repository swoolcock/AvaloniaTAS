using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ReactiveUI;
using TAS.Avalonia.Views;
using TAS.Avalonia.Controls;

namespace TAS.Avalonia.ViewModels;

public class DialogWindowViewModel : ViewModelBase {
    protected CancellationTokenSource DialogTokenSource;

    private string _title;
    public string Title {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _description;
    public string Description {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public DialogButtonCollection Buttons { get; }
    public ICollection<IDialogControl> Controls { get; }

    public virtual async Task<DialogButton> ShowAsync() {
        if (DialogTokenSource?.IsCancellationRequested == false)
            throw new InvalidOperationException("Windows already showed.");

        using (DialogTokenSource = new CancellationTokenSource()) {
            var dialog = new DialogWindow(this);
            dialog.Closed += (sender, args) => CloseImpl();
            DialogTokenSource.Token.Register(() => Dispatcher.UIThread.InvokeAsync(() => dialog.Close()));

            Window owner = null;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                owner = lifetime.Windows.FirstOrDefault(w => w.IsActive);
            if (owner != null) {
                await dialog.ShowDialog(owner).ConfigureAwait(true);
            } else {
                dialog.Show();
                Dispatcher.UIThread.MainLoop(DialogTokenSource.Token);
            }
            return dialog.ResultButton;
        }
    }

    public virtual Task CloseAsync() {
        this.CloseImpl();
        return Task.CompletedTask;
    }

    private void CloseImpl() {
        if (DialogTokenSource == null)
            return;

        if (!DialogTokenSource.IsCancellationRequested)
            DialogTokenSource.Cancel();
        DialogTokenSource.Dispose();
    }

    public DialogWindowViewModel() {
        Buttons = new DialogButtonCollection();
        Controls = new List<IDialogControl>();
    }
}
