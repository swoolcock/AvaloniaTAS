using System.Collections;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using DynamicData;
using ReactiveUI;

#pragma warning disable CS8601

namespace TAS.Avalonia.Models;

public class MenuModel : IEnumerable<MenuModel> {
    public static readonly MenuModel Separator = new MenuModel(string.Empty);

    public string Header { get; init; }
    public ICommand Command { get; init; }
    public object CommandParameter { get; init; }
    public KeyGesture Gesture { get; init; }
    public List<MenuModel> Items { get; init; } = new List<MenuModel>();
    public bool? IsEnabled { get; init; }
    public bool IsChecked { get; init; }
    public bool IsVisible { get; init; } = true;

    public MenuModel(string header, RoutedCommand routedCommand, TextArea textArea, object commandParameter = null, bool? isEnabled = null, bool isChecked = false, bool isVisible = true) {
        Header = header;
        Command = ReactiveCommand.Create<object>(param => routedCommand.Execute(param, textArea));
        CommandParameter = commandParameter;
        Gesture = routedCommand.Gesture;
        IsEnabled = isEnabled;
        IsChecked = isChecked;
        IsVisible = isVisible;
    }

    public MenuModel(string header, ICommand command = null, object commandParameter = null, KeyGesture gesture = null, bool? isEnabled = null, bool isChecked = false, bool isVisible = true) {
        Header = header;
        Command = command;
        CommandParameter = commandParameter;
        Gesture = gesture;
        IsEnabled = isEnabled;
        IsChecked = isChecked;
        IsVisible = isVisible;
    }

    public NativeMenuItemBase ToNativeMenuItem() {
        if (!IsVisible) return null;
        if (string.IsNullOrEmpty(Header)) return new NativeMenuItemSeparator();

        var nativeMenuItem = new NativeMenuItem(Header?.Replace("_", "")) {
            Command = Command,
            CommandParameter = CommandParameter,
            Gesture = Gesture,
            IsEnabled = IsEnabled ?? (Items.Any() || Command?.CanExecute(CommandParameter) == true),
            IsChecked = IsChecked,
        };

        if (Items.Any()) nativeMenuItem.Menu = Items.ToNativeMenu();

        return nativeMenuItem;
    }

    public MenuItem ToMenuItem() {
        var menuItem = new MenuItem {
            Header = Header,
            Command = Command,
            CommandParameter = CommandParameter,
            HotKey = Gesture,
            InputGesture = Gesture,
            IsEnabled = IsEnabled ?? (Items.Any() || Command?.CanExecute(CommandParameter) == true),
            IsVisible = IsVisible,
        };

        if (Items.Any())
            menuItem.ItemsSource = Items.ToMenu();

        return menuItem;
    }

    public IEnumerator<MenuModel> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(MenuModel menuModel) => Items.Add(menuModel);
}
public static class MenuModelExtensions {
    public static NativeMenu ToNativeMenu(this IEnumerable<MenuModel> self) {
        var menu = new NativeMenu();
        menu.Items.AddRange(self.Select(x => x.ToNativeMenuItem()).Where(x => x is not null));
        return menu;
    }

    public static IEnumerable<Control> ToMenu(this IEnumerable<MenuModel> self) =>
        self.Select(x => string.IsNullOrEmpty(x.Header)
            ? new Separator {
                IsVisible = x.IsVisible
            } as Control
            : x.ToMenuItem()).ToArray();

    public static ContextMenu ToContextMenu(this IEnumerable<MenuModel> self) =>
        new ContextMenu {
            ItemsSource = self.ToMenu()
        };
}
