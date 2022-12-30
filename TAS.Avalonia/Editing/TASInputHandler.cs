using System;
using System.Windows.Input;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using TAS.Core.Models;

namespace TAS.Avalonia.Editing;

#nullable disable

public class TASInputHandler : TextAreaInputHandler
{
    public TextAreaInputHandler CaretNavigation { get; }

    public TextAreaInputHandler Editing { get; }

    public ITextAreaInputHandler MouseSelection { get; }

    private TASInputHandler InputHandler => TextArea.ActiveInputHandler as TASInputHandler;

    public TASInputHandler(TextArea textArea) : base(textArea)
    {
        NestedInputHandlers.Add(CaretNavigation = TASCaretNavigationCommandHandler.Create(textArea));
        NestedInputHandlers.Add(Editing = TASEditingCommandHandler.Create(textArea));
        NestedInputHandlers.Add(MouseSelection = new TASSelectionMouseHandler(textArea));
        AddBinding(ApplicationCommands.Undo, ExecuteUndo, CanExecuteUndo);
        AddBinding(ApplicationCommands.Redo, ExecuteRedo, CanExecuteRedo);
    }

    private void AddBinding(
        RoutedCommand command,
        EventHandler<ExecutedRoutedEventArgs> handler,
        EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null)
    {
        CommandBindings.Add(new RoutedCommandBinding(command, handler, canExecuteHandler));
    }

    internal static KeyBinding CreateKeyBinding(
        ICommand command,
        KeyModifiers modifiers,
        Key key)
    {
        return CreateKeyBinding(command, new KeyGesture(key, modifiers));
    }

    internal static KeyBinding CreateKeyBinding(ICommand command, KeyGesture gesture) => new KeyBinding
    {
        Command = command,
        Gesture = gesture
    };

    private UndoStack GetUndoStack() => TextArea.Document?.UndoStack;

    private void ExecuteUndo(object sender, ExecutedRoutedEventArgs e)
    {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null)
            return;
        if (undoStack.CanUndo)
        {
            undoStack.Undo();
            TextArea.Caret.BringCaretToView();
        }
        e.Handled = true;
    }

    private void CanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
    {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null)
            return;
        e.Handled = true;
        e.CanExecute = undoStack.CanUndo;
    }

    private void ExecuteRedo(object sender, ExecutedRoutedEventArgs e)
    {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null)
            return;
        if (undoStack.CanRedo)
        {
            undoStack.Redo();
            TextArea.Caret.BringCaretToView();
        }
        e.Handled = true;
    }

    private void CanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
    {
        UndoStack undoStack = GetUndoStack();
        if (undoStack == null)
            return;
        e.Handled = true;
        e.CanExecute = undoStack.CanRedo;
    }

    // public override void OnPreviewKeyDown(KeyEventArgs e)
    // {
    //     var caretPosition = TextArea.Caret.Position;
    //     var desiredXpos = TextArea.Caret.DesiredXPos;
    //     if (!TryGetLine(TextArea.Document, caretPosition, out var line, out var lineText)) return;
    //
    //     var caretKeyBinding = InputHandler.CaretNavigation.KeyBindings.FirstOrDefault(b => b.Gesture.Matches(e));
    //     if (caretKeyBinding != null)
    //     {
    //         var visualLine = TextArea.TextView.GetOrConstructVisualLine((DocumentLine)line);
    //         var visualTextLine = visualLine.GetTextLine(caretPosition.VisualColumn, caretPosition.IsAtEndOfLine);
    //         var isActionLine = TASActionLine.TryParse(lineText, out var actionLine);
    //         var lastColumn = isActionLine ? TASActionLine.MaxFramesDigits + 1 : line.TotalLength + 1;
    //         var firstColumn = isActionLine ? lastColumn - actionLine.Frames.Digits() : 1;
    //         var startOfLine = caretPosition.Column <= firstColumn;
    //         var endOfLine = caretPosition.Column >= lastColumn;
    //
    //         var movementType = CaretMovementTypeExtensions.ForKeyBinding(caretKeyBinding);
    //         // handle moving left
    //         if (caretKeyBinding.Command == EditingCommands.MoveLeftByCharacter || caretKeyBinding.Command == EditingCommands.MoveLeftByWord)
    //         {
    //             if (startOfLine && caretPosition.Line > 1)
    //             {
    //                 caretPosition.Line--;
    //                 caretPosition.VisualColumn = caretPosition.Column = LastColumn(caretPosition);
    //             }
    //             else if (!startOfLine && caretKeyBinding.Command == EditingCommands.MoveLeftByWord)
    //                 caretPosition.VisualColumn = caretPosition.Column = FirstColumn(caretPosition);
    //             else if (!startOfLine) caretPosition.VisualColumn = caretPosition.Column = (caretPosition.Column - 1);
    //
    //             TextArea.Caret.Position = caretPosition;
    //             TextArea.Caret.BringCaretToView();
    //             e.Handled = true;
    //         }
    //         // handle moving right
    //         else if (caretKeyBinding.Command == EditingCommands.MoveRightByCharacter || caretKeyBinding.Command == EditingCommands.MoveRightByWord)
    //         {
    //             if (endOfLine && caretPosition.Line < TextArea.Document.LineCount)
    //             {
    //                 caretPosition.Line++;
    //                 caretPosition.VisualColumn = caretPosition.Column = FirstColumn(caretPosition);
    //             }
    //             else if (!endOfLine && caretKeyBinding.Command == EditingCommands.MoveRightByWord)
    //                 caretPosition.VisualColumn = caretPosition.Column = LastColumn(caretPosition);
    //             else if (!endOfLine) caretPosition.VisualColumn = caretPosition.Column = (caretPosition.Column + 1);
    //
    //             TextArea.Caret.Position = caretPosition;
    //             TextArea.Caret.BringCaretToView();
    //             e.Handled = true;
    //         }
    //         // handle moving up
    //         else if (caretKeyBinding.Command == EditingCommands.MoveUpByLine || caretKeyBinding.Command == EditingCommands.MoveUpByPage)
    //         {
    //             if (caretPosition.Line == 1)
    //                 caretPosition.VisualColumn = caretPosition.Column = FirstColumn(caretPosition);
    //             else if (caretKeyBinding.Command == EditingCommands.MoveUpByLine)
    //             {
    //                 var lineVisualYPosition = visualLine.GetTextLineVisualYPosition(visualTextLine, VisualYPosition.LineMiddle);
    //                 var visualTop = lineVisualYPosition - TextArea.TextView.Bounds.Height;
    //                 var docLineByVisualTop = TextArea.TextView.GetDocumentLineByVisualTop(visualTop);
    //                 visualLine = TextArea.TextView.GetOrConstructVisualLine(docLineByVisualTop);
    //                 visualTextLine = visualLine.GetTextLineByVisualYPosition(visualTop);
    //             }
    //
    //             TextArea.Caret.Position = caretPosition;
    //             TextArea.Caret.BringCaretToView();
    //             e.Handled = true;
    //         }
    //     }
    // }

    private int FirstColumn(TextViewPosition caretPosition)
    {
        if (!TryGetLine(TextArea.Document, caretPosition, out _, out var lineText)) return 1;
        return TASActionLine.TryParse(lineText, out var actionLine) ? TASActionLine.MaxFramesDigits - actionLine.Frames.Digits() + 1 : 1;
    }

    private int LastColumn(TextViewPosition caretPosition)
    {
        if (!TryGetLine(TextArea.Document, caretPosition, out var line, out var lineText)) return 1;
        return TASActionLine.TryParse(lineText, out _) ? TASActionLine.MaxFramesDigits + 1 : line.TotalLength + 1;
    }

    private static bool TryGetLine(IDocument document, TextViewPosition caretPosition, out IDocumentLine line, out string lineText)
    {
        line = null;
        lineText = null;
        if (document == null) return false;

        line = document.GetLineByNumber(caretPosition.Line);
        if (line == null) return false;

        lineText = document.GetText(line);
        return lineText != null;
    }
}
