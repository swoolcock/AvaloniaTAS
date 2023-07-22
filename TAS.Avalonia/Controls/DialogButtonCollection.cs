using System.Collections;
using System.Reactive.Subjects;
using ReactiveUI;

namespace TAS.Avalonia.Controls;

public class DialogButtonCollection : ReactiveObject, IEnumerable {
    private readonly ICollection<DialogButton> buttons = new List<DialogButton>();

    public DialogButton DefaultButton {
        get { return buttons.SingleOrDefault(b => b.IsDefault); }
        set {
            ChangeDefault(value);
            this.RaisePropertyChanged();
        }
    }

    public DialogButton CancelButton {
        get { return buttons.SingleOrDefault(b => b.IsCancel); }
        set {
            ChangeCancel(value);
            this.RaisePropertyChanged();
        }
    }

    public IObservable<DialogButton.ClickArguments> Clicked => clickedSubject;
    private readonly Subject<DialogButton.ClickArguments> clickedSubject = new Subject<DialogButton.ClickArguments>();

    public void AddButton(DialogButton button) {
        if (buttons.Contains(button))
            return;

        button.Clicked.Subscribe(args => clickedSubject.OnNext(args));

        if (button.IsDefault)
            ChangeDefault(button);

        if (button.IsCancel)
            ChangeCancel(button);

        buttons.Add(button);
    }

    public DialogButton AddButton(string buttonName) {
        var newButton = new DialogButton();
        newButton.Name = buttonName;
        this.AddButton(newButton);
        return newButton;
    }

    public void AddOk() => AddButton(new DialogButton {
        Name = "Ok",
        IsDefault = true,
    });
    public void AddCancel() => AddButton(new DialogButton {
        Name = "Cancel",
        IsCancel = true,
    });
    public void AddOkCancel() {
        AddOk();
        AddCancel();
    }

    public int Count { get => buttons.Count; }
    public IEnumerator<DialogButton> GetEnumerator() => buttons.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => (buttons as IEnumerable).GetEnumerator();

    private void ChangeDefault(DialogButton newButton) {
        var old = buttons.SingleOrDefault(b => b.IsDefault);
        if (old != null)
            old.IsDefault = false;

        newButton.IsDefault = true;
    }

    private void ChangeCancel(DialogButton newButton) {
        var old = buttons.SingleOrDefault(b => b.IsCancel);
        if (old != null)
            old.IsCancel = false;

        newButton.IsCancel = true;
    }
}
