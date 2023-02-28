using System.Reactive;
using ReactiveUI;

namespace TAS.Avalonia.ViewModels;

public partial class InputDialogWindowViewModel : ReactiveObject {
    public object Value { get; private set; }
    private int IntValue { get => (int)Value; set => Value = value; }
    private float FloatValue { get => (float)Value; set => Value = value; }

    public object MinValue { get; }
    public object MaxValue { get; }

    private object initialValue { get; }

    private bool isFloat { get; }

    public ReactiveCommand<Unit, object> ConfirmCommand { get; }
    public ReactiveCommand<Unit, object> CancelCommand { get; }

    public InputDialogWindowViewModel(object currentValue, object minValue, object maxValue) {
        this.Value = currentValue;
        this.MinValue = minValue;
        this.MaxValue = maxValue;

        this.initialValue = currentValue;
        this.isFloat = currentValue is float;

        this.ConfirmCommand = ReactiveCommand.Create(() => (object)Value);
        this.CancelCommand = ReactiveCommand.Create(() => (object)initialValue);
    }
}