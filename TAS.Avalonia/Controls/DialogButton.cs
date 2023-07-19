using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;

namespace TAS.Avalonia.Controls;

public class DialogButton : ReactiveObject, ICommand {

    public class ClickArguments {
        public bool CloseAfterClick { get; set; }

        public DialogButton Button { get; }

        public ClickArguments(DialogButton button) {
            Button = button;
            CloseAfterClick = true;
        }
    }

    public string Name {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    private string name;

    public bool IsVisible {
        get => isVisible;
        set => this.RaiseAndSetIfChanged(ref isVisible, value);
    }

    private bool isVisible;

    public bool IsEnabled {
        get => isEnabled;
        set => this.RaiseAndSetIfChanged(ref isEnabled, value);
    }

    private bool isEnabled;

    public bool IsDefault {
        get => isDefault;
        set => this.RaiseAndSetIfChanged(ref isDefault, value);
    }

    private bool isDefault;

    public bool IsCancel {
        get => isCancel;
        set => this.RaiseAndSetIfChanged(ref isCancel, value);
    }

    private bool isCancel;

    public IObservable<ClickArguments> OnClick => onClickSubject;
    public IObservable<ClickArguments> Clicked => clickedSubject;

    private readonly Subject<ClickArguments> onClickSubject;
    private readonly Subject<ClickArguments> clickedSubject;

    public bool CanExecute(object parameter) {
        return IsEnabled && IsVisible;
    }

    public void Execute(object parameter) {
        var buttonArgs = new ClickArguments(this);
        onClickSubject?.OnNext(buttonArgs);
        clickedSubject?.OnNext(buttonArgs);
    }

    public event EventHandler CanExecuteChanged;

    protected virtual void OnCanExecuteChanged() {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public override bool Equals(object obj) {
        var button = obj as DialogButton;
        if (button != null)
            return button.GetType() == GetType() && Equals(button.Name, Name);
        return false;
    }

    public override int GetHashCode() {
        return GetType().GetHashCode() ^ Name.GetHashCode();
    }

    public static bool operator ==(DialogButton button1, DialogButton button2) {
        if (ReferenceEquals(button1, null)) {
            return ReferenceEquals(button2, null);
        }

        return button1.Equals(button2);
    }

    public static bool operator !=(DialogButton button1, DialogButton button2) {
        return !(button1 == button2);
    }

    public DialogButton() {
        IsEnabled = true;
        IsVisible = true;
        onClickSubject = new Subject<ClickArguments>();
        clickedSubject = new Subject<ClickArguments>();
    }
}
