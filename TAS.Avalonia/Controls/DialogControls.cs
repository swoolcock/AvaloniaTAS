using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using ReactiveUI;

namespace TAS.Avalonia.Controls;

public interface IDialogControl : INotifyPropertyChanged {
    string Name { get; set; }

    bool IsRequired { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }
}

public abstract class AbstractControl<T> : ReactiveObject, IDialogControl {
    private string _name;
    public string Name {
        get => _name;
        set {
            _name = value;
            OnPropertyChanged();
        }
    }

    private bool _isRequired;
    public bool IsRequired {
        get => _isRequired;
        set {
            _isRequired = value;
            OnPropertyChanged();
        }
    }

    private bool _isVisible;
    public bool IsVisible {
        get => _isVisible;
        set {
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    private bool _isEnabled;
    public bool IsEnabled {
        get => _isEnabled;
        set {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    private T _value;
    public T Value {
        get => _value;
        set {
            _value = value;
            OnPropertyChanged();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        if (Dispatcher.UIThread.CheckAccess())
            this.RaisePropertyChanged(propertyName);
        else
            Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(propertyName));
    }

    protected AbstractControl() {
        IsEnabled = true;
        IsVisible = true;
        IsRequired = false;
    }
}

// Styled with DataTemplates
public class BoolControl : AbstractControl<bool> { }
public class StringControl : AbstractControl<string> { }

public class IntRangeControl : AbstractControl<int> {
    public int MinValue { get; }
    public int MaxValue { get; }

    public IntRangeControl(int minValue, int maxValue) {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}

public class FloatRangeControl : AbstractControl<float> {
    public float MinValue { get; }
    public float MaxValue { get; }

    public FloatRangeControl(float minValue, float maxValue) {
        MinValue = minValue;
        MaxValue = maxValue;
    }
}
