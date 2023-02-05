using ReactiveUI;

namespace TAS.Avalonia.ViewModels;

public class ViewModelBase : ReactiveObject//INotifyPropertyChanged, INotifyPropertyChanging
{
    // public event PropertyChangingEventHandler? PropertyChanging;
    // public event PropertyChangedEventHandler? PropertyChanged;
    //
    // [NotifyPropertyChangedInvocator]
    // protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
    //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //
    // protected virtual void OnPropertyChanging([CallerMemberName] string? propertyName = null) =>
    //     PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    //
    // protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    // {
    //     var comparer = EqualityComparer<T>.Default;
    //     if (comparer.Equals(storage, value)) return;
    //     OnPropertyChanging(propertyName);
    //     storage = value;
    //     OnPropertyChanged(propertyName);
    // }
}
