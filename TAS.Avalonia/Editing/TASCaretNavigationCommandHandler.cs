using System.Collections.Immutable;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using DynamicData;
using TAS.Avalonia.Models;
using LogicalDirection = AvaloniaEdit.Document.LogicalDirection;

namespace TAS.Avalonia.Editing;

#nullable disable

internal static class TASCaretNavigationCommandHandler {
    private static readonly List<RoutedCommandBinding> CommandBindings = new List<RoutedCommandBinding>();
    private static readonly List<KeyBinding> KeyBindings = new List<KeyBinding>();

    internal static RoutedCommand SelectBlock { get; } = new(nameof(SelectBlock), new KeyGesture(Key.W, TASInputHandler.PlatformCommandKey));

    public static TextAreaInputHandler Create(TextArea textArea) {
        var areaInputHandler = new TextAreaInputHandler(textArea);
        areaInputHandler.CommandBindings.AddRange(CommandBindings);
        areaInputHandler.KeyBindings.AddRange(KeyBindings);
        return areaInputHandler;
    }

    private static void AddBinding(RoutedCommand command, EventHandler<ExecutedRoutedEventArgs> handler) {
        CommandBindings.Add(new RoutedCommandBinding(command, handler));
    }

    private static void AddBinding(RoutedCommand command, KeyModifiers modifiers, Key key, EventHandler<ExecutedRoutedEventArgs> handler) {
        AddBinding(command, new KeyGesture(key, modifiers), handler);
    }

    private static void AddBinding(RoutedCommand command, KeyGesture gesture, EventHandler<ExecutedRoutedEventArgs> handler) {
        AddBinding(command, handler);
        KeyBindings.Add(TASInputHandler.CreateKeyBinding(command, gesture));
    }

    static TASCaretNavigationCommandHandler() {
        var keymap = Application.Current.PlatformSettings.HotkeyConfiguration;

        AddBinding(EditingCommands.MoveLeftByCharacter, KeyModifiers.None, Key.Left, OnMoveCaret(CaretMovementType.CharLeft));
        AddBinding(EditingCommands.SelectLeftByCharacter, keymap.SelectionModifiers, Key.Left, OnMoveCaretExtendSelection(CaretMovementType.CharLeft));
        AddBinding(RectangleSelection.BoxSelectLeftByCharacter, KeyModifiers.Alt | keymap.SelectionModifiers, Key.Left, OnMoveCaretBoxSelection(CaretMovementType.CharLeft));
        AddBinding(EditingCommands.MoveRightByCharacter, KeyModifiers.None, Key.Right, OnMoveCaret(CaretMovementType.CharRight));
        AddBinding(EditingCommands.SelectRightByCharacter, keymap.SelectionModifiers, Key.Right, OnMoveCaretExtendSelection(CaretMovementType.CharRight));
        AddBinding(RectangleSelection.BoxSelectRightByCharacter, KeyModifiers.Alt | keymap.SelectionModifiers, Key.Right, OnMoveCaretBoxSelection(CaretMovementType.CharRight));

        AddBinding(EditingCommands.MoveLeftByWord, keymap.WholeWordTextActionModifiers, Key.Left, OnMoveCaret(CaretMovementType.WordLeft));
        AddBinding(EditingCommands.SelectLeftByWord, keymap.WholeWordTextActionModifiers | keymap.SelectionModifiers, Key.Left, OnMoveCaretExtendSelection(CaretMovementType.WordLeft));
        AddBinding(RectangleSelection.BoxSelectLeftByWord, keymap.WholeWordTextActionModifiers | KeyModifiers.Alt | keymap.SelectionModifiers, Key.Left, OnMoveCaretBoxSelection(CaretMovementType.WordLeft));
        AddBinding(EditingCommands.MoveRightByWord, keymap.WholeWordTextActionModifiers, Key.Right, OnMoveCaret(CaretMovementType.WordRight));
        AddBinding(EditingCommands.SelectRightByWord, keymap.WholeWordTextActionModifiers | keymap.SelectionModifiers, Key.Right, OnMoveCaretExtendSelection(CaretMovementType.WordRight));
        AddBinding(RectangleSelection.BoxSelectRightByWord, keymap.WholeWordTextActionModifiers | KeyModifiers.Alt | keymap.SelectionModifiers, Key.Right, OnMoveCaretBoxSelection(CaretMovementType.WordRight));

        AddBinding(EditingCommands.MoveUpByLine, KeyModifiers.None, Key.Up, OnMoveCaret(CaretMovementType.LineUp));
        AddBinding(EditingCommands.SelectUpByLine, keymap.SelectionModifiers, Key.Up, OnMoveCaretExtendSelection(CaretMovementType.LineUp));
        AddBinding(RectangleSelection.BoxSelectUpByLine, KeyModifiers.Alt | keymap.SelectionModifiers, Key.Up, OnMoveCaretBoxSelection(CaretMovementType.LineUp));
        AddBinding(EditingCommands.MoveDownByLine, KeyModifiers.None, Key.Down, OnMoveCaret(CaretMovementType.LineDown));
        AddBinding(EditingCommands.SelectDownByLine, keymap.SelectionModifiers, Key.Down, OnMoveCaretExtendSelection(CaretMovementType.LineDown));
        AddBinding(RectangleSelection.BoxSelectDownByLine, KeyModifiers.Alt | keymap.SelectionModifiers, Key.Down, OnMoveCaretBoxSelection(CaretMovementType.LineDown));

        AddBinding(EditingCommands.MoveDownByPage, KeyModifiers.None, Key.PageDown, OnMoveCaret(CaretMovementType.PageDown));
        AddBinding(EditingCommands.SelectDownByPage, keymap.SelectionModifiers, Key.PageDown, OnMoveCaretExtendSelection(CaretMovementType.PageDown));
        AddBinding(EditingCommands.MoveUpByPage, KeyModifiers.None, Key.PageUp, OnMoveCaret(CaretMovementType.PageUp));
        AddBinding(EditingCommands.SelectUpByPage, keymap.SelectionModifiers, Key.PageUp, OnMoveCaretExtendSelection(CaretMovementType.PageUp));

        AddBinding(RectangleSelection.BoxSelectToLineStart, KeyModifiers.Alt | keymap.SelectionModifiers, Key.Home, OnMoveCaretBoxSelection(CaretMovementType.LineStart));
        AddBinding(RectangleSelection.BoxSelectToLineEnd, KeyModifiers.Alt | keymap.SelectionModifiers, Key.End, OnMoveCaretBoxSelection(CaretMovementType.LineEnd));

        AddBinding(ApplicationCommands.SelectAll, OnSelectAll);
        AddBinding(SelectBlock, OnSelectBlock);

        foreach (KeyGesture gesture in keymap.MoveCursorToTheStartOfLine) {
            AddBinding(EditingCommands.MoveToLineStart, gesture, OnMoveCaret(CaretMovementType.LineStart));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheStartOfLineWithSelection) {
            AddBinding(EditingCommands.SelectToLineStart, gesture, OnMoveCaretExtendSelection(CaretMovementType.LineStart));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheEndOfLine) {
            AddBinding(EditingCommands.MoveToLineEnd, gesture, OnMoveCaret(CaretMovementType.LineEnd));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheEndOfLineWithSelection) {
            AddBinding(EditingCommands.SelectToLineEnd, gesture, OnMoveCaretExtendSelection(CaretMovementType.LineEnd));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheStartOfDocument) {
            AddBinding(EditingCommands.MoveToDocumentStart, gesture, OnMoveCaret(CaretMovementType.DocumentStart));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheStartOfDocumentWithSelection) {
            AddBinding(EditingCommands.SelectToDocumentStart, gesture, OnMoveCaretExtendSelection(CaretMovementType.DocumentStart));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheEndOfDocument) {
            AddBinding(EditingCommands.MoveToDocumentEnd, gesture, OnMoveCaret(CaretMovementType.DocumentEnd));
        }

        foreach (KeyGesture gesture in keymap.MoveCursorToTheEndOfDocumentWithSelection) {
            AddBinding(EditingCommands.SelectToDocumentEnd, gesture, OnMoveCaretExtendSelection(CaretMovementType.DocumentEnd));
        }
    }

    private static void OnSelectAll(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        args.Handled = true;
        textArea.Caret.Offset = textArea.Document.TextLength;
        textArea.Selection = Selection.Create(textArea, 0, textArea.Document.TextLength);
    }

    private static void OnSelectBlock(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        args.Handled = true;

        // Search first empty line above/below caret
        int above = textArea.Caret.Line;
        DocumentLine aboveLine = null;
        while (above >= 1 && !string.IsNullOrWhiteSpace(textArea.Document.GetText(aboveLine = textArea.Document.GetLineByNumber(above)))) {
            above--;
        }
        int below = textArea.Caret.Line;
        DocumentLine belowLine = null;
        while (below <= textArea.Document.LineCount && !string.IsNullOrWhiteSpace(textArea.Document.GetText(belowLine = textArea.Document.GetLineByNumber(below)))) {
            below++;
        }

        if (aboveLine == null || belowLine == null) return;

        textArea.Caret.Offset = aboveLine.Offset + 1;
        textArea.Selection = Selection.Create(textArea, aboveLine.Offset + 1, belowLine.EndOffset);
    }

    private static TextArea GetTextArea(object target) => target as TextArea;

    private static EventHandler<ExecutedRoutedEventArgs> OnMoveCaret(CaretMovementType direction) {
        return (target, args) => {
            TextArea textArea = GetTextArea(target);
            if (textArea?.Document == null)
                return;
            args.Handled = true;
            textArea.ClearSelection();
            MoveCaret(textArea, direction);
            textArea.Caret.BringCaretToView();
        };
    }

    private static EventHandler<ExecutedRoutedEventArgs> OnMoveCaretExtendSelection(CaretMovementType direction) {
        return (target, args) => {
            TextArea textArea = GetTextArea(target);
            if (textArea?.Document == null)
                return;
            args.Handled = true;
            TextViewPosition position = textArea.Caret.Position;
            MoveCaret(textArea, direction);
            textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(position, textArea.Caret.Position);
            textArea.Caret.BringCaretToView();
        };
    }

    private static EventHandler<ExecutedRoutedEventArgs> OnMoveCaretBoxSelection(CaretMovementType direction) {
        return (target, args) => {
            TextArea textArea = GetTextArea(target);
            if (textArea?.Document == null)
                return;
            args.Handled = true;
            if (textArea.Options.EnableRectangularSelection && !(textArea.Selection is RectangleSelection))
                textArea.Selection = textArea.Selection.IsEmpty ? new RectangleSelection(textArea, textArea.Caret.Position, textArea.Caret.Position) : (Selection) new RectangleSelection(textArea, textArea.Selection.StartPosition, textArea.Caret.Position);
            TextViewPosition position = textArea.Caret.Position;
            MoveCaret(textArea, direction);
            textArea.Selection = textArea.Selection.StartSelectionOrSetEndpoint(position, textArea.Caret.Position);
            textArea.Caret.BringCaretToView();
        };
    }

    // For movement without Ctrl
    internal static ImmutableHashSet<int> GetSoftSnapColumns(TASActionLine actionLine) {
        int leadingSpaces = TASActionLine.MaxFramesDigits - actionLine.Frames.Digits();

        HashSet<int> softSnapColumns = new();
        // Frame count
        softSnapColumns.AddRange(Enumerable.Range(leadingSpaces + 1, actionLine.Frames.Digits() + 1));
        // Actions
        foreach (var action in actionLine.Actions.Sorted()) {
            int column = GetColumnOfAction(actionLine, action);
            softSnapColumns.Add(column);

            if (action == TASAction.DashOnly)
                softSnapColumns.AddRange(Enumerable.Range(column, actionLine.Actions.GetDashOnly().Count() + 1));
            if (action == TASAction.MoveOnly)
                softSnapColumns.AddRange(Enumerable.Range(column, actionLine.Actions.GetMoveOnly().Count() + 1));
            if (action == TASAction.CustomBinding)
                softSnapColumns.AddRange(Enumerable.Range(column, actionLine.CustomBindings.Count + 1));
        }
        // Feather angle/magnitude
        if (actionLine.Actions.HasFlag(TASAction.FeatherAim)) {
            int featherColumn = GetColumnOfAction(actionLine, TASAction.FeatherAim);
            softSnapColumns.AddRange(Enumerable.Range(featherColumn, actionLine.ToString().Length + 2 - featherColumn));
        }

        return softSnapColumns.ToImmutableHashSet();
    }

    // For movement with Ctrl
    internal static ImmutableHashSet<int> GetHardSnapColumns(TASActionLine actionLine) {
        int leadingSpaces = TASActionLine.MaxFramesDigits - actionLine.Frames.Digits();

        HashSet<int> hardSnapColumns = new() {
            // Frame count
            leadingSpaces + 1,
            TASActionLine.MaxFramesDigits + 1,
            // Actions
            GetColumnOfAction(actionLine, actionLine.Actions.Sorted().Last()) + actionLine.CustomBindings.Count
        };

        // Feather angle/magnitude
        if (actionLine.Actions.HasFlag(TASAction.FeatherAim)) {
            int featherColumn = GetColumnOfAction(actionLine, TASAction.FeatherAim);
            string lineText = actionLine.ToString();

            int decimalColumn = featherColumn + 1;
            while (decimalColumn <= lineText.Length && lineText[decimalColumn - 1] != '.') {
                decimalColumn++;
            }
            hardSnapColumns.Add(decimalColumn);
            hardSnapColumns.Add(decimalColumn + 1);

            if (actionLine.FeatherMagnitude != null) {
                hardSnapColumns.Add(featherColumn + 1);
                int borderColumn = featherColumn + 1;
                while (borderColumn <= lineText.Length && lineText[borderColumn - 1] != ',') {
                    borderColumn++;
                }
                hardSnapColumns.Add(borderColumn);
                hardSnapColumns.Add(borderColumn + 1);

                decimalColumn = borderColumn + 1;
                while (decimalColumn <= lineText.Length && lineText[decimalColumn - 1] != '.') {
                    decimalColumn++;
                }
                hardSnapColumns.Add(decimalColumn);
                hardSnapColumns.Add(decimalColumn + 1);
            }
            hardSnapColumns.Add(lineText.Length + 1);
        }

        return hardSnapColumns.ToImmutableHashSet();
    }

    internal static int GetColumnOfAction(TASActionLine actionLine, TASAction action) {
        int index = actionLine.Actions.Sorted().IndexOf(action);
        if (index < 0) return -1;

        int dashOnlyIndex = actionLine.Actions.Sorted().IndexOf(TASAction.DashOnly);
        int moveOnlyIndex = actionLine.Actions.Sorted().IndexOf(TASAction.MoveOnly);
        int customBindingIndex = actionLine.Actions.Sorted().IndexOf(TASAction.CustomBinding);

        int additionalOffset = 0;

        if (dashOnlyIndex != -1 && index > dashOnlyIndex)
            additionalOffset += actionLine.Actions.GetDashOnly().Count();
        if (moveOnlyIndex != -1 && index > moveOnlyIndex)
            additionalOffset += actionLine.Actions.GetMoveOnly().Count();
        if (customBindingIndex != -1 && index > customBindingIndex)
            additionalOffset += actionLine.CustomBindings.Count;

        return TASActionLine.MaxFramesDigits + 1 + (index + 1) * 2 + additionalOffset;
    }

    internal static int SnapColumnToActionLine(TASActionLine actionLine, int column) {
        var lineText = actionLine.ToString();

        int leadingSpaces = lineText.Length - lineText.TrimStart().Length;
        int digitCount = actionLine.Frames.Digits();

        // Snap to the closest valid column
        int nextLeft = GetSoftSnapColumns(actionLine).Reverse().FirstOrDefault(c => c <= column, -1);
        int nextRight = GetSoftSnapColumns(actionLine).FirstOrDefault(c => c >= column, -1);

        if (nextLeft == column || nextRight == column) return column;

        if (nextLeft == -1 && nextRight == -1) return column;
        if (nextLeft == -1) return nextRight;
        if (nextRight == -1) return nextLeft;

        if (column - nextLeft < nextRight - column) return nextLeft;
        return nextRight;
    }

    internal static TASAction GetActionsFromColumn(TASActionLine actionLine, int column, CaretMovementType direction) {
        var lineText = actionLine.ToString();

        if ((column <= TASActionLine.MaxFramesDigits + 1) &&
            (direction == CaretMovementType.CharLeft || direction == CaretMovementType.Backspace || direction == CaretMovementType.WordLeft)) {
            return TASAction.None; // There are no actions to the left of the caret
        }
        if ((column <= TASActionLine.MaxFramesDigits || column >= lineText.Length) &&
            (direction == CaretMovementType.CharRight || direction == CaretMovementType.WordRight)) {
            return TASAction.None; // There are no actions to the right of the caret
        }

        if (direction == CaretMovementType.CharLeft || direction == CaretMovementType.Backspace) {
            //  15,R|,X => R
            return lineText[column - 2].ActionForChar();
        } else if (direction == CaretMovementType.CharRight) {
            //  15,R|,X => X
            return lineText[column].ActionForChar();
        } else if (direction == CaretMovementType.WordLeft) {
            //  15,R,D|,X => R,D
            TASAction actions = TASAction.None;
            while (column > TASActionLine.MaxFramesDigits + 1) {
                actions |= lineText[column - 2].ActionForChar();
                column -= 2;
            }
            return actions;
        } else {
            //  15,R|,D,X => D,X
            TASAction actions = TASAction.None;
            while (column < lineText.Length) {
                actions |= lineText[column].ActionForChar();
                column += 2;
            }
            return actions;
        }
    }

    internal static void MoveCaret(TextArea textArea, CaretMovementType direction) {
        double desiredXpos = textArea.Caret.DesiredXPos;

        var position = textArea.Caret.Position;
        var newPosition = position;

        // try to handle the movement by ourselves if it's a action line
        if (textArea.Document.GetLineByNumber(position.Line) is { } line &&
            textArea.Document.GetText(line) is { } lineText &&
            TASActionLine.TryParse(lineText, out var actionLine)) {
            position.Column = SnapColumnToActionLine(actionLine, position.Column);
            int leadingSpaces = TASActionLine.MaxFramesDigits - actionLine.Frames.Digits();

            newPosition = direction switch {
                CaretMovementType.CharLeft => new TextViewPosition(position.Line, GetSoftSnapColumns(actionLine).Reverse().FirstOrDefault(c => c < position.Column, position.Column)),
                CaretMovementType.CharRight => new TextViewPosition(position.Line, GetSoftSnapColumns(actionLine).FirstOrDefault(c => c > position.Column, position.Column)),
                CaretMovementType.WordLeft => new TextViewPosition(position.Line, GetHardSnapColumns(actionLine).Reverse().FirstOrDefault(c => c < position.Column, position.Column)),
                CaretMovementType.WordRight => new TextViewPosition(position.Line, GetHardSnapColumns(actionLine).FirstOrDefault(c => c > position.Column, position.Column)),
                CaretMovementType.LineStart => new TextViewPosition(position.Line, leadingSpaces + 1),
                CaretMovementType.LineEnd => new TextViewPosition(position.Line, line.Length + 1),
                _ => GetNewCaretPosition(textArea.TextView, position, direction, textArea.Selection.EnableVirtualSpace, ref desiredXpos),
            };

            if (direction is CaretMovementType.CharLeft or CaretMovementType.CharRight
                or CaretMovementType.WordLeft or CaretMovementType.WordRight
                or CaretMovementType.LineStart or CaretMovementType.LineEnd) {
                desiredXpos = double.NaN;
            }

        } else {
            // Standart text behaviour
            newPosition = GetNewCaretPosition(textArea.TextView, position, direction, textArea.Selection.EnableVirtualSpace, ref desiredXpos);
        }

        newPosition.Line = Math.Clamp(newPosition.Line, 1, textArea.Document.LineCount);
        if (textArea.Document.GetLineByNumber(newPosition.Line) is { } newLine &&
            textArea.Document.GetText(newLine) is { } newLineText &&
            TASActionLine.TryParse(newLineText, out var newActionLine)) {
            newPosition.Column = Math.Clamp(newPosition.Column, 1, newLine.Length + 1);
            newPosition.Column = SnapColumnToActionLine(newActionLine, newPosition.Column);
        }

        newPosition.VisualColumn = newPosition.Column - 1;
        textArea.Caret.Position = newPosition;
        textArea.Caret.DesiredXPos = desiredXpos;
    }

    internal static TextViewPosition GetNewCaretPosition(TextView textView, TextViewPosition caretPosition, CaretMovementType direction,
                                                         bool enableVirtualSpace, ref double desiredXPos) {
        switch (direction) {
            case CaretMovementType.None:
                return caretPosition;
            case CaretMovementType.DocumentStart:
                desiredXPos = double.NaN;
                return new TextViewPosition(0, 0);
            case CaretMovementType.DocumentEnd:
                desiredXPos = double.NaN;
                return new TextViewPosition(textView.Document.GetLocation(textView.Document.TextLength));
            default:
                DocumentLine lineByNumber = textView.Document.GetLineByNumber(caretPosition.Line);
                VisualLine constructVisualLine = textView.GetOrConstructVisualLine(lineByNumber);
                TextLine textLine = constructVisualLine.GetTextLine(caretPosition.VisualColumn, caretPosition.IsAtEndOfLine);
                switch (direction) {
                    case CaretMovementType.CharLeft:
                        desiredXPos = double.NaN;
                        return caretPosition.VisualColumn == 0 & enableVirtualSpace ? caretPosition : GetPrevCaretPosition(textView, caretPosition, constructVisualLine, CaretPositioningMode.Normal, enableVirtualSpace);
                    case CaretMovementType.CharRight:
                        desiredXPos = double.NaN;
                        return GetNextCaretPosition(textView, caretPosition, constructVisualLine, CaretPositioningMode.Normal, enableVirtualSpace);
                    case CaretMovementType.Backspace:
                        desiredXPos = double.NaN;
                        return GetPrevCaretPosition(textView, caretPosition, constructVisualLine, CaretPositioningMode.EveryCodepoint, enableVirtualSpace);
                    case CaretMovementType.WordLeft:
                        desiredXPos = double.NaN;
                        return GetPrevCaretPosition(textView, caretPosition, constructVisualLine, CaretPositioningMode.WordStart, enableVirtualSpace);
                    case CaretMovementType.WordRight:
                        desiredXPos = double.NaN;
                        return GetNextCaretPosition(textView, caretPosition, constructVisualLine, CaretPositioningMode.WordStart, enableVirtualSpace);
                    case CaretMovementType.LineUp:
                    case CaretMovementType.LineDown:
                    case CaretMovementType.PageUp:
                    case CaretMovementType.PageDown:
                        return GetUpDownCaretPosition(textView, caretPosition, direction, constructVisualLine, textLine, enableVirtualSpace, ref desiredXPos);
                    case CaretMovementType.LineStart:
                        desiredXPos = double.NaN;
                        return GetStartOfLineCaretPosition(caretPosition.VisualColumn, constructVisualLine, textLine, enableVirtualSpace);
                    case CaretMovementType.LineEnd:
                        desiredXPos = double.NaN;
                        return GetEndOfLineCaretPosition(constructVisualLine, textLine);
                    default:
                        throw new NotSupportedException(direction.ToString());
                }
        }
    }

    private static TextViewPosition GetStartOfLineCaretPosition(int oldVisualColumn, VisualLine visualLine, TextLine textLine, bool enableVirtualSpace) {
        int visualColumn = visualLine.GetTextLineVisualStartColumn(textLine);
        if (visualColumn == 0)
            visualColumn = visualLine.GetNextCaretPosition(visualColumn - 1, LogicalDirection.Forward, CaretPositioningMode.WordStart, enableVirtualSpace);
        if (visualColumn < 0)
            throw ThrowUtil.NoValidCaretPosition();
        if (visualColumn == oldVisualColumn)
            visualColumn = 0;
        return visualLine.GetTextViewPosition(visualColumn);
    }

    private static TextViewPosition GetEndOfLineCaretPosition(VisualLine visualLine, TextLine textLine) {
        int visualColumn = visualLine.GetTextLineVisualStartColumn(textLine) + textLine.Length - textLine.TrailingWhitespaceLength;
        return visualLine.GetTextViewPosition(visualColumn) with { IsAtEndOfLine = true };
    }

    private static TextViewPosition GetNextCaretPosition(TextView textView, TextViewPosition caretPosition, VisualLine visualLine,
                                                         CaretPositioningMode mode, bool enableVirtualSpace) {
        int nextCaretPosition1 = visualLine.GetNextCaretPosition(caretPosition.VisualColumn, LogicalDirection.Forward, mode, enableVirtualSpace);
        if (nextCaretPosition1 >= 0)
            return visualLine.GetTextViewPosition(nextCaretPosition1);
        DocumentLine nextLine = visualLine.LastDocumentLine.NextLine;
        if (nextLine != null) {
            VisualLine constructVisualLine = textView.GetOrConstructVisualLine(nextLine);
            int nextCaretPosition2 = constructVisualLine.GetNextCaretPosition(-1, LogicalDirection.Forward, mode, enableVirtualSpace);
            return nextCaretPosition2 >= 0 ? constructVisualLine.GetTextViewPosition(nextCaretPosition2) : throw ThrowUtil.NoValidCaretPosition();
        }

        Debug.Assert(visualLine.LastDocumentLine.Offset + visualLine.LastDocumentLine.TotalLength == textView.Document.TextLength);
        return new TextViewPosition(textView.Document.GetLocation(textView.Document.TextLength));
    }

    private static TextViewPosition GetPrevCaretPosition(TextView textView, TextViewPosition caretPosition, VisualLine visualLine,
                                                         CaretPositioningMode mode, bool enableVirtualSpace) {
        int nextCaretPosition1 = visualLine.GetNextCaretPosition(caretPosition.VisualColumn, LogicalDirection.Backward, mode, enableVirtualSpace);
        if (nextCaretPosition1 >= 0)
            return visualLine.GetTextViewPosition(nextCaretPosition1);
        DocumentLine previousLine = visualLine.FirstDocumentLine.PreviousLine;
        if (previousLine != null) {
            VisualLine constructVisualLine = textView.GetOrConstructVisualLine(previousLine);
            int nextCaretPosition2 = constructVisualLine.GetNextCaretPosition(constructVisualLine.VisualLength + 1, LogicalDirection.Backward, mode, enableVirtualSpace);
            return nextCaretPosition2 >= 0 ? constructVisualLine.GetTextViewPosition(nextCaretPosition2) : throw ThrowUtil.NoValidCaretPosition();
        }

        Debug.Assert(visualLine.FirstDocumentLine.Offset == 0);
        return new TextViewPosition(0, 0);
    }

    private static TextViewPosition GetUpDownCaretPosition(TextView textView, TextViewPosition caretPosition, CaretMovementType direction,
                                                           VisualLine visualLine, TextLine textLine, bool enableVirtualSpace, ref double xPos) {
        if (double.IsNaN(xPos))
            xPos = visualLine.GetTextLineVisualXPosition(textLine, caretPosition.VisualColumn);
        VisualLine visualLine1 = visualLine;
        int num = visualLine.TextLines.IndexOf(textLine);
        TextLine textLine1;
        switch (direction) {
            case CaretMovementType.LineUp:
                int number1 = visualLine.FirstDocumentLine.LineNumber - 1;
                if (num > 0) {
                    textLine1 = visualLine.TextLines[num - 1];
                    break;
                }

                if (number1 >= 1) {
                    DocumentLine lineByNumber = textView.Document.GetLineByNumber(number1);
                    visualLine1 = textView.GetOrConstructVisualLine(lineByNumber);
                    textLine1 = visualLine1.TextLines[visualLine1.TextLines.Count - 1];
                    break;
                }

                textLine1 = null;
                break;
            case CaretMovementType.LineDown:
                int number2 = visualLine.LastDocumentLine.LineNumber + 1;
                if (num < visualLine.TextLines.Count - 1) {
                    textLine1 = visualLine.TextLines[num + 1];
                    break;
                }

                if (number2 <= textView.Document.LineCount) {
                    DocumentLine lineByNumber = textView.Document.GetLineByNumber(number2);
                    visualLine1 = textView.GetOrConstructVisualLine(lineByNumber);
                    textLine1 = visualLine1.TextLines[0];
                    break;
                }

                textLine1 = null;
                break;
            case CaretMovementType.PageUp:
            case CaretMovementType.PageDown:
                double lineVisualYposition1 = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.LineMiddle);
                double visualTop = direction != CaretMovementType.PageUp ? lineVisualYposition1 + textView.Bounds.Height : lineVisualYposition1 - textView.Bounds.Height;
                DocumentLine documentLineByVisualTop = textView.GetDocumentLineByVisualTop(visualTop);
                visualLine1 = textView.GetOrConstructVisualLine(documentLineByVisualTop);
                textLine1 = visualLine1.GetTextLineByVisualYPosition(visualTop);
                break;
            default:
                throw new NotSupportedException(direction.ToString());
        }

        if (textLine1 == null) return caretPosition;

        double lineVisualYposition2 = visualLine1.GetTextLineVisualYPosition(textLine1, VisualYPosition.LineMiddle);
        int visualColumn = visualLine1.GetVisualColumn(new Point(xPos, lineVisualYposition2), enableVirtualSpace);
        int visualStartColumn = visualLine1.GetTextLineVisualStartColumn(textLine1);
        if (visualColumn >= visualStartColumn + textLine1.Length && visualColumn <= visualLine1.VisualLength) {
            visualColumn = visualStartColumn + textLine1.Length - 1;
        }

        return visualLine1.GetTextViewPosition(visualColumn);
    }
}
