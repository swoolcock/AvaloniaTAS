using System.Windows.Input;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using System.Runtime.InteropServices;

namespace TAS.Avalonia.Editing;

#nullable disable

public class TASInputHandler : TextAreaInputHandler {
    internal static readonly KeyModifiers PlatformCommandKey = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? KeyModifiers.Meta : KeyModifiers.Control;

    internal static RoutedCommand Redo { get; } = new(nameof(Redo), new KeyGesture(Key.Z, PlatformCommandKey | KeyModifiers.Shift));

    public TextAreaInputHandler CaretNavigation { get; }
    public TextAreaInputHandler Editing { get; }
    public ITextAreaInputHandler MouseSelection { get; }

    private TASInputHandler InputHandler => TextArea.ActiveInputHandler as TASInputHandler;

    public IEnumerable<KeyBinding> AllKeyBindings =>
        KeyBindings.Concat(CaretNavigation.KeyBindings).Concat(Editing.KeyBindings);
    public IEnumerable<RoutedCommandBinding> AllCommandBindings =>
        CommandBindings.Concat(CaretNavigation.CommandBindings).Concat(Editing.CommandBindings);

    public TASInputHandler(TextArea textArea) : base(textArea) {
        NestedInputHandlers.Add(CaretNavigation = TASCaretNavigationCommandHandler.Create(textArea));
        NestedInputHandlers.Add(Editing = TASEditingCommandHandler.Create(textArea));
        NestedInputHandlers.Add(MouseSelection = new TASSelectionMouseHandler(textArea));
        AddBinding(ApplicationCommands.Undo, ExecuteUndo, CanExecuteUndo);
        AddBinding(Redo, ExecuteRedo, CanExecuteRedo);
    }

    private void AddBinding(RoutedCommand command, EventHandler<ExecutedRoutedEventArgs> handler, EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null) {
        CommandBindings.Add(new RoutedCommandBinding(command, handler, canExecuteHandler));
    }

    internal static KeyBinding CreateKeyBinding(ICommand command, KeyModifiers modifiers, Key key) {
        return CreateKeyBinding(command, new KeyGesture(key, modifiers));
    }

    internal static KeyBinding CreateKeyBinding(ICommand command, KeyGesture gesture) => new KeyBinding { Command = command, Gesture = gesture };

    private UndoStack GetUndoStack() => TextArea.Document?.UndoStack;

    private void ExecuteUndo(object sender, ExecutedRoutedEventArgs e) {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null) return;
        if (undoStack.CanUndo) {
            undoStack.Undo();
            TextArea.Caret.BringCaretToView();
        }

        e.Handled = true;
    }

    private void CanExecuteUndo(object sender, CanExecuteRoutedEventArgs e) {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null) return;
        e.Handled = true;
        e.CanExecute = undoStack.CanUndo;
    }

    private void ExecuteRedo(object sender, ExecutedRoutedEventArgs e) {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null) return;
        if (undoStack.CanRedo) {
            undoStack.Redo();
            TextArea.Caret.BringCaretToView();
        }

        e.Handled = true;
    }

    private void CanExecuteRedo(object sender, CanExecuteRoutedEventArgs e) {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null) return;
        e.Handled = true;
        e.CanExecute = undoStack.CanRedo;
    }
}
