using System.Globalization;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Utils;
using TAS.Avalonia.Models;

namespace TAS.Avalonia.Editing;

#nullable disable

internal class TASEditingCommandHandler {
    private static readonly List<RoutedCommandBinding> CommandBindings = new List<RoutedCommandBinding>();
    private static readonly List<KeyBinding> KeyBindings = new List<KeyBinding>();

    internal static RoutedCommand DeleteLine { get; } = new(nameof(DeleteLine), new KeyGesture(Key.Y, TASInputHandler.PlatformCommandKey));

    internal static RoutedCommand ToggleCommentInputs { get; } = new(nameof(ToggleCommentInputs), new KeyGesture(Key.K, TASInputHandler.PlatformCommandKey));
    internal static RoutedCommand ToggleCommentText { get; } = new(nameof(ToggleCommentText), new KeyGesture(Key.K, TASInputHandler.PlatformCommandKey | KeyModifiers.Shift));

    internal static RoutedCommand InsertRoomName { get; } = new(nameof(InsertRoomName), new KeyGesture(Key.R, TASInputHandler.PlatformCommandKey));
    internal static RoutedCommand InsertTime { get; } = new(nameof(InsertRoomName), new KeyGesture(Key.T, TASInputHandler.PlatformCommandKey));
    internal static RoutedCommand InsertCommand { get; } = new(nameof(InsertCommand));

    public static TextAreaInputHandler Create(TextArea textArea) {
        var areaInputHandler = new TextAreaInputHandler(textArea);
        areaInputHandler.CommandBindings.AddRange(CommandBindings);
        areaInputHandler.KeyBindings.AddRange(KeyBindings);
        return areaInputHandler;
    }

    private static void AddBinding(RoutedCommand command, KeyModifiers modifiers, Key key, EventHandler<ExecutedRoutedEventArgs> handler) {
        CommandBindings.Add(new RoutedCommandBinding(command, handler));
        KeyBindings.Add(TASInputHandler.CreateKeyBinding(command, modifiers, key));
    }

    private static void AddBinding(RoutedCommand command, EventHandler<ExecutedRoutedEventArgs> handler, EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler = null) {
        CommandBindings.Add(new RoutedCommandBinding(command, handler, canExecuteHandler));
    }

    static TASEditingCommandHandler() {
        // Editing commands
        AddBinding(EditingCommands.Delete, KeyModifiers.None, Key.Delete, OnDelete(CaretMovementType.CharRight));
        AddBinding(EditingCommands.DeleteNextWord, KeyModifiers.Control, Key.Delete, OnDelete(CaretMovementType.WordRight));
        AddBinding(EditingCommands.Backspace, KeyModifiers.None, Key.Back, OnDelete(CaretMovementType.Backspace));
        KeyBindings.Add(TASInputHandler.CreateKeyBinding(EditingCommands.Backspace, KeyModifiers.Shift, Key.Back));
        AddBinding(EditingCommands.DeletePreviousWord, KeyModifiers.Control, Key.Back, OnDelete(CaretMovementType.WordLeft));
        AddBinding(EditingCommands.EnterParagraphBreak, KeyModifiers.None, Key.Return, OnEnter);
        AddBinding(EditingCommands.EnterLineBreak, KeyModifiers.Shift, Key.Return, OnEnter);
        AddBinding(EditingCommands.TabForward, KeyModifiers.None, Key.Tab, OnTab);
        AddBinding(EditingCommands.TabBackward, KeyModifiers.Shift, Key.Tab, OnShiftTab);
        AddBinding(ApplicationCommands.Copy, OnCopy, CanCutOrCopy);
        AddBinding(ApplicationCommands.Cut, OnCut, CanCutOrCopy);
        AddBinding(ApplicationCommands.Paste, OnPaste, CanPaste);
        AddBinding(AvaloniaEditCommands.ToggleOverstrike, OnToggleOverstrike);
        AddBinding(DeleteLine, OnDeleteLine);
        AddBinding(AvaloniaEditCommands.RemoveLeadingWhitespace, OnRemoveLeadingWhitespace);
        AddBinding(AvaloniaEditCommands.RemoveTrailingWhitespace, OnRemoveTrailingWhitespace);
        AddBinding(AvaloniaEditCommands.ConvertToUppercase, OnConvertToUpperCase);
        AddBinding(AvaloniaEditCommands.ConvertToLowercase, OnConvertToLowerCase);
        AddBinding(AvaloniaEditCommands.ConvertToTitleCase, OnConvertToTitleCase);
        AddBinding(AvaloniaEditCommands.InvertCase, OnInvertCase);
        AddBinding(AvaloniaEditCommands.ConvertTabsToSpaces, OnConvertTabsToSpaces);
        AddBinding(AvaloniaEditCommands.ConvertSpacesToTabs, OnConvertSpacesToTabs);
        AddBinding(AvaloniaEditCommands.ConvertLeadingTabsToSpaces, OnConvertLeadingTabsToSpaces);
        AddBinding(AvaloniaEditCommands.ConvertLeadingSpacesToTabs, OnConvertLeadingSpacesToTabs);
        AddBinding(AvaloniaEditCommands.IndentSelection, OnIndentSelection);

        // TAS specific commands
        AddBinding(ToggleCommentInputs, OnToggleCommentInputs);
        AddBinding(ToggleCommentText, OnToggleCommentText);
        AddBinding(InsertRoomName, OnInsertTextAbove(static _ => $"#lvl_{(Application.Current as App)!.CelesteService.LevelName}"));
        AddBinding(InsertTime, OnInsertTextAbove(static _ => $"#{(Application.Current as App)!.CelesteService.ChapterTime}"));
        AddBinding(InsertCommand, OnInsertTextAbove(static command => (string)command));
    }

    // TAS specific commands

    private static void OnToggleCommentInputs(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;

        using (textArea.Document.RunUpdate()) {
            int startLine = textArea.Selection.StartPosition.Line;
            int endLine = textArea.Selection.EndPosition.Line;
            if (textArea.Selection.IsEmpty) {
                startLine = endLine = textArea.Caret.Line;
            }

            // Make sure that start <= end
            int tmp = startLine;
            startLine = Math.Min(startLine, endLine);
            endLine = Math.Max(tmp, endLine);

            for (int i = startLine; i <= endLine; i++) {
                var line = textArea.Document.GetLineByNumber(i);
                string lineText = textArea.Document.GetText(line);

                if (lineText.TrimStart().StartsWith('#')) {
                    int hashIdx = lineText.IndexOf('#');
                    textArea.Document.Replace(line, lineText.Remove(hashIdx, 1));
                } else {
                    textArea.Document.Replace(line, $"#{lineText}");
                }
            }
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private static void OnToggleCommentText(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;

        using (textArea.Document.RunUpdate()) {
            int startLine = textArea.Selection.StartPosition.Line;
            int endLine = textArea.Selection.EndPosition.Line;
            if (textArea.Selection.IsEmpty) {
                startLine = endLine = textArea.Caret.Line;
            }

            // Make sure that start <= end
            int tmp = startLine;
            startLine = Math.Min(startLine, endLine);
            endLine = Math.Max(tmp, endLine);

            // Only remove # when all lines start with it. Otherwise add another
            bool allCommented = true;
            for (int i = startLine; i <= endLine; i++) {
                var line = textArea.Document.GetLineByNumber(i);
                string lineText = textArea.Document.GetText(line);

                if (!lineText.TrimStart().StartsWith('#')) {
                    allCommented = false;
                    break;
                }
            }
            for (int i = startLine; i <= endLine; i++) {
                var line = textArea.Document.GetLineByNumber(i);
                string lineText = textArea.Document.GetText(line);

                if (allCommented) {
                    int hashIdx = lineText.IndexOf('#');
                    textArea.Document.Replace(line, lineText.Remove(hashIdx, 1));
                } else {
                    textArea.Document.Replace(line, $"#{lineText}");
                }
            }
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private static  EventHandler<ExecutedRoutedEventArgs> OnInsertTextAbove(Func<object,string> getText) => (target, args) => {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;

        using (textArea.Document.RunUpdate()) {
            var line = textArea.Document.GetLineByNumber(textArea.Caret.Line);
            textArea.Document.Insert(line.Offset, $"{getText(args.Parameter)}{Environment.NewLine}");
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    };

    // Editing commands

    private static TextArea GetTextArea(object target) => target as TextArea;

    internal static void AutoFormatActionLines(TextArea textArea, int lineStart, int lineEnd) {
        for (int i = lineStart; i <= lineEnd; i++) {
            var line = textArea.Document.GetLineByNumber(i);
            var lineText = textArea.Document.GetText(line);
            if (TASActionLine.TryParse(lineText, out var actionLine)) {
                // We don't want that auto-formatting messes with undo/redo
                var oldUndoStack = textArea.Document._undoStack;
                textArea.Document._undoStack = new UndoStack();

                textArea.Document.Replace(line, actionLine.ToString());

                textArea.Document._undoStack = oldUndoStack;
            }
        }
    }

    private static void TransformSelectedLines(Action<TextArea, DocumentLine> transformLine, object target, ExecutedRoutedEventArgs args, DefaultSegmentType defaultSegmentType) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        using (textArea.Document.RunUpdate()) {
            DocumentLine documentLine1;
            DocumentLine documentLine2;
            if (textArea.Selection.IsEmpty) {
                switch (defaultSegmentType) {
                    case DefaultSegmentType.WholeDocument:
                        documentLine2 = textArea.Document.Lines.First();
                        documentLine1 = textArea.Document.Lines.Last();
                        break;
                    case DefaultSegmentType.CurrentLine:
                        documentLine2 = documentLine1 = textArea.Document.GetLineByNumber(textArea.Caret.Line);
                        break;
                    default:
                        documentLine2 = documentLine1 = null;
                        break;
                }
            } else {
                ISegment surroundingSegment = textArea.Selection.SurroundingSegment;
                documentLine2 = textArea.Document.GetLineByOffset(surroundingSegment.Offset);
                documentLine1 = textArea.Document.GetLineByOffset(surroundingSegment.EndOffset);
                if (documentLine2 != documentLine1 && documentLine1.Offset == surroundingSegment.EndOffset) documentLine1 = documentLine1.PreviousLine;
            }

            if (documentLine2 != null) {
                transformLine(textArea, documentLine2);
                while (documentLine2 != documentLine1) {
                    documentLine2 = documentLine2.NextLine;
                    transformLine(textArea, documentLine2);
                }
            }
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private static void TransformSelectedSegments(Action<TextArea, ISegment> transformSegment, object target, ExecutedRoutedEventArgs args, DefaultSegmentType defaultSegmentType) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        using (textArea.Document.RunUpdate()) {
            IEnumerable<ISegment> source;
            if (textArea.Selection.IsEmpty) {
                switch (defaultSegmentType) {
                    case DefaultSegmentType.WholeDocument:
                        source = textArea.Document.Lines;
                        break;
                    case DefaultSegmentType.CurrentLine:
                        source = new ISegment[1] { textArea.Document.GetLineByNumber(textArea.Caret.Line) };
                        break;
                    default:
                        source = null;
                        break;
                }
            } else
                source = textArea.Selection.Segments;

            if (source != null) {
                foreach (ISegment segment1 in source.Reverse()) {
                    foreach (ISegment segment2 in textArea.GetDeletableSegments(segment1).Reverse()) transformSegment(textArea, segment2);
                }
            }
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private static void OnEnter(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea == null || !textArea.IsFocused) return;

        var caretPosition = textArea.Caret.Position;
        if (textArea.Document is not { } document ||
            document.GetLineByNumber(caretPosition.Line) is not { } line ||
            document.GetText(line) is not { } lineText) {
            textArea.PerformTextInput(Environment.NewLine);
            args.Handled = true;
            return;
        }

        // don't split frame count and action
        document.Insert(line.EndOffset, Environment.NewLine);

        caretPosition.Line++;
        textArea.Caret.Position = caretPosition;

        args.Handled = true;
    }

    private static void OnTab(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        using (textArea.Document.RunUpdate()) {
            if (textArea.Selection.IsMultiline) {
                ISegment surroundingSegment = textArea.Selection.SurroundingSegment;
                DocumentLine lineByOffset = textArea.Document.GetLineByOffset(surroundingSegment.Offset);
                DocumentLine documentLine1 = textArea.Document.GetLineByOffset(surroundingSegment.EndOffset);
                if (lineByOffset != documentLine1 && documentLine1.Offset == surroundingSegment.EndOffset) documentLine1 = documentLine1.PreviousLine;
                DocumentLine documentLine2 = lineByOffset;
                while (true) {
                    int offset = documentLine2.Offset;
                    if (textArea.ReadOnlySectionProvider.CanInsert(offset)) textArea.Document.Replace(offset, 0, textArea.Options.IndentationString, OffsetChangeMappingType.KeepAnchorBeforeInsertion);
                    if (documentLine2 != documentLine1)
                        documentLine2 = documentLine2.NextLine;
                    else
                        break;
                }
            } else {
                string indentationString = textArea.Options.GetIndentationString(textArea.Caret.Column);
                textArea.ReplaceSelectionWithText(indentationString);
            }
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private static void OnShiftTab(object target, ExecutedRoutedEventArgs args) => TransformSelectedLines((textArea, line) => {
        int offset = line.Offset;
        ISegment indentationSegment = TextUtilities.GetSingleIndentationSegment(textArea.Document, offset, textArea.Options.IndentationSize);
        if (indentationSegment.Length <= 0) return;
        ISegment segment = textArea.GetDeletableSegments(indentationSegment).FirstOrDefault();
        if (segment != null && segment.Length > 0) textArea.Document.Remove(segment.Offset, segment.Length);
    }, target, args, DefaultSegmentType.CurrentLine);

    private static EventHandler<ExecutedRoutedEventArgs> OnDelete(CaretMovementType direction) => (target, args) => {
        var textArea = GetTextArea(target);
        if (textArea?.Document == null) return;

        if (textArea.Selection.IsEmpty) {
            double desiredXpos = textArea.Caret.DesiredXPos;

            var position = textArea.Caret.Position;
            var newPosition = position;

            if (textArea.Document.GetLineByNumber(position.Line) is { } line &&
                textArea.Document.GetText(line) is { } lineText &&
                TASActionLine.TryParse(lineText, out var actionLine)) {
                position.Column = TASCaretNavigationCommandHandler.SnapColumnToActionLine(actionLine, position.Column);

                var lineStartPosition = new TextViewPosition(position.Line, 1);
                var lineEndPosition = new TextViewPosition(position.Line, line.Length + 1);

                // Handle frame count
                if (position.Column == TASActionLine.MaxFramesDigits + 1 && direction is CaretMovementType.WordLeft or CaretMovementType.Backspace ||
                    position.Column < TASActionLine.MaxFramesDigits + 1) {
                    int leadingSpaces = lineText.Length - lineText.TrimStart().Length;
                    int cursorIndex = Math.Clamp(position.Column - leadingSpaces - 1, 0, actionLine.Frames.Digits());

                    string framesString = actionLine.Frames.ToString();
                    string leftOfCursor = framesString[..cursorIndex];
                    string rightOfCursor = framesString[cursorIndex..];

                    if (actionLine.Frames == 0) {
                        lineText = string.Empty;
                    } else if (leftOfCursor.Length == 0 && direction is CaretMovementType.WordLeft or CaretMovementType.Backspace ||
                        rightOfCursor.Length == 0 && direction is CaretMovementType.WordRight or CaretMovementType.CharRight) {
                        lineText = string.Empty;
                    } else {
                        string newFramesString = string.Empty;
                        if (direction == CaretMovementType.WordLeft) {
                            newFramesString = rightOfCursor;
                            cursorIndex = 0;
                        } else if (direction == CaretMovementType.WordRight) {
                            newFramesString = leftOfCursor;
                        } else if (direction == CaretMovementType.Backspace) {
                            newFramesString = $"{leftOfCursor[..^1]}{rightOfCursor}";
                            cursorIndex--;
                        } else if (direction == CaretMovementType.CharRight) {
                            newFramesString = $"{leftOfCursor}{rightOfCursor[1..]}";
                        }

                        actionLine.Frames = Math.Clamp(int.TryParse(newFramesString, out int value) ? value : 0, 0, TASActionLine.MaxFrames);
                        lineText = actionLine.ToString();
                        position.Column = actionLine.Frames == 0
                            ? TASActionLine.MaxFramesDigits + 1
                            : TASActionLine.MaxFramesDigits - actionLine.Frames.Digits() + cursorIndex + 1;
                    }
                    goto FinishDeletion; // Skip regular deletion behaviour
                }

                // Handle feather angle/magnitude
                int featherColumn = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.FeatherAim);
                if (featherColumn != -1 && position.Column >= featherColumn) {
                    int angleMagnitudeCommaColumn = featherColumn + 2;
                    while (angleMagnitudeCommaColumn <= lineText.Length + 1 && lineText[angleMagnitudeCommaColumn - 2] != ',') {
                        angleMagnitudeCommaColumn++;
                    }

                    if (position.Column == featherColumn + 1 && direction is CaretMovementType.Backspace or CaretMovementType.WordLeft) {
                        var actions = TASCaretNavigationCommandHandler.GetActionsFromColumn(actionLine, position.Column - 1, direction);
                        actionLine.Actions &= ~actions;
                        lineText = actionLine.ToString();
                        goto FinishDeletion;
                    } else if (position.Column == featherColumn && direction is CaretMovementType.CharRight or CaretMovementType.WordRight ||
                        position.Column == angleMagnitudeCommaColumn && direction is CaretMovementType.Backspace or CaretMovementType.WordLeft) {
                        actionLine.FeatherAngle = actionLine.FeatherMagnitude;
                        actionLine.FeatherMagnitude = null;
                        position.Column = featherColumn;
                        lineText = actionLine.ToString();
                        goto FinishDeletion;
                    } else if (position.Column == angleMagnitudeCommaColumn - 1 && direction is CaretMovementType.CharRight or CaretMovementType.WordRight) {
                        actionLine.FeatherMagnitude = null;
                        lineText = actionLine.ToString();
                        goto FinishDeletion;
                    }
                }

                int newColumn = direction switch {
                    CaretMovementType.Backspace => TASCaretNavigationCommandHandler.GetSoftSnapColumns(actionLine).Reverse().FirstOrDefault(c => c < position.Column, position.Column),
                    CaretMovementType.CharRight => TASCaretNavigationCommandHandler.GetSoftSnapColumns(actionLine).FirstOrDefault(c => c > position.Column, position.Column),
                    CaretMovementType.WordLeft => TASCaretNavigationCommandHandler.GetHardSnapColumns(actionLine).Reverse().FirstOrDefault(c => c < position.Column, position.Column),
                    CaretMovementType.WordRight => TASCaretNavigationCommandHandler.GetHardSnapColumns(actionLine).FirstOrDefault(c => c > position.Column, position.Column),
                    _ => position.Column,
                };
                lineText = lineText.Remove(Math.Min(newColumn, position.Column) - 1, Math.Abs(newColumn - position.Column));
                position.Column = Math.Min(newColumn, position.Column);

                FinishDeletion:
                if (TASActionLine.TryParse(lineText, out var newActionLine)) {
                    lineText = newActionLine.ToString();
                } else if (string.IsNullOrWhiteSpace(lineText)) {
                    lineText = string.Empty;
                    position = lineStartPosition;
                }

                textArea.Selection.StartSelectionOrSetEndpoint(lineStartPosition, lineEndPosition)
                                  .ReplaceSelectionWithText(lineText);

                if (textArea.Caret.Position.Column != position.Column)
                    position.VisualColumn = position.Column - 1;

                textArea.Caret.Position = position;
            } else {
                bool enableVirtualSpace = textArea.Options.EnableVirtualSpace;
                if (direction == CaretMovementType.CharRight)
                    enableVirtualSpace = false;

                var endPosition = TASCaretNavigationCommandHandler.GetNewCaretPosition(textArea.TextView, position, direction, enableVirtualSpace, ref desiredXpos);

                if (endPosition.Line < 1 || endPosition.Column < 1)
                    endPosition = new TextViewPosition(Math.Max(endPosition.Line, 1), Math.Max(endPosition.Column, 1));
                if (textArea.Selection is RectangleSelection && position.Line != endPosition.Line)
                    return;
                textArea.Selection.StartSelectionOrSetEndpoint(position, endPosition).ReplaceSelectionWithText(string.Empty);
            }
        } else {
            textArea.RemoveSelectedText();
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    };

    private static void CanDelete(object target, CanExecuteRoutedEventArgs args) {
        if (GetTextArea(target)?.Document == null) return;
        args.CanExecute = true;
        args.Handled = true;
    }

    private static void CanCutOrCopy(object target, CanExecuteRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        args.CanExecute = textArea.Options.CutCopyWholeLine || !textArea.Selection.IsEmpty;
        args.Handled = true;
    }

    private static void OnCopy(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        if (textArea.Selection.IsEmpty && textArea.Options.CutCopyWholeLine) {
            DocumentLine lineByNumber = textArea.Document.GetLineByNumber(textArea.Caret.Line);
            CopyWholeLine(textArea, lineByNumber);
        } else {
            CopySelectedText(textArea);
        }

        args.Handled = true;
    }

    private static void OnCut(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        if (textArea.Selection.IsEmpty && textArea.Options.CutCopyWholeLine) {
            DocumentLine lineByNumber = textArea.Document.GetLineByNumber(textArea.Caret.Line);
            if (CopyWholeLine(textArea, lineByNumber)) {
                ISegment[] deletableSegments = textArea.GetDeletableSegments((ISegment) new SimpleSegment(lineByNumber.Offset, lineByNumber.TotalLength));
                for (int index = deletableSegments.Length - 1; index >= 0; --index) textArea.Document.Remove(deletableSegments[index]);
            }
        } else if (CopySelectedText(textArea)) textArea.RemoveSelectedText();

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private static bool CopySelectedText(TextArea textArea) {
        string text = TextUtilities.NormalizeNewLines(textArea.Selection.GetText(), Environment.NewLine);
        SetClipboardText(text, textArea);
        textArea.OnTextCopied(new TextEventArgs(text));
        return true;
    }

    private static void SetClipboardText(string text, Visual visual) {
        try {
            TopLevel.GetTopLevel(visual)?.Clipboard?.SetTextAsync(text).GetAwaiter().GetResult();
        } catch (Exception) {
            // Apparently this exception sometimes happens randomly.
            // The MS controls just ignore it, so we'll do the same.
        }
    }

    private static bool CopyWholeLine(TextArea textArea, DocumentLine line) {
        ISegment wholeLine = new SimpleSegment(line.Offset, line.TotalLength);
        var text = textArea.Document.GetText(wholeLine);
        // Ignore empty line copy
        if (string.IsNullOrEmpty(text)) return false;
        // Ensure we use the appropriate newline sequence for the OS
        text = TextUtilities.NormalizeNewLines(text, Environment.NewLine);

        SetClipboardText(text, textArea);
        textArea.OnTextCopied(new TextEventArgs(text));
        return true;
    }

    private static void CanPaste(object target, CanExecuteRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        args.CanExecute = textArea.ReadOnlySectionProvider.CanInsert(textArea.Caret.Offset);
        args.Handled = true;
    }

    private static async void OnPaste(object target, ExecutedRoutedEventArgs args) {
        var textArea = GetTextArea(target);
        if (textArea?.Document == null) return;

        textArea.Document.BeginUpdate();

        string text = null;
        try {
            text = await TopLevel.GetTopLevel(textArea)?.Clipboard?.GetTextAsync();
        } catch (Exception) {
            textArea.Document.EndUpdate();
            return;
        }

        if (text == null)
            return;

        text = GetTextToPaste(text, textArea);

        int lineStart = textArea.Caret.Position.Line;
        if (!string.IsNullOrEmpty(text)) {
            textArea.ReplaceSelectionWithText(text);
        }
        int lineEnd = textArea.Caret.Position.Line;

        AutoFormatActionLines(textArea, lineStart, lineEnd);

        textArea.Caret.BringCaretToView();
        args.Handled = true;

        textArea.Document.EndUpdate();
    }

    internal static string GetTextToPaste(string text, TextArea textArea) {
        try {
            string lineFromDocument = TextUtilities.GetNewLineFromDocument(textArea.Document, textArea.Caret.Line);
            text = TextUtilities.NormalizeNewLines(text, lineFromDocument);
            text = textArea.Options.ConvertTabsToSpaces ? text.Replace("\t", new string(' ', textArea.Options.IndentationSize)) : text;
            return text;
        } catch (OutOfMemoryException) {
            return null;
        }
    }

    private static void OnToggleOverstrike(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea == null || !textArea.Options.AllowToggleOverstrikeMode) return;
        textArea.OverstrikeMode = !textArea.OverstrikeMode;
    }

    private static void OnDeleteLine(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null) return;
        int number1;
        int number2;
        if (textArea.Selection.Length == 0) {
            number2 = number1 = textArea.Caret.Line;
        } else {
            TextViewPosition textViewPosition = textArea.Selection.StartPosition;
            int line1 = textViewPosition.Line;
            textViewPosition = textArea.Selection.EndPosition;
            int line2 = textViewPosition.Line;
            number2 = Math.Min(line1, line2);
            textViewPosition = textArea.Selection.StartPosition;
            int line3 = textViewPosition.Line;
            textViewPosition = textArea.Selection.EndPosition;
            int line4 = textViewPosition.Line;
            number1 = Math.Max(line3, line4);
        }

        DocumentLine lineByNumber1 = textArea.Document.GetLineByNumber(number2);
        DocumentLine lineByNumber2 = textArea.Document.GetLineByNumber(number1);
        textArea.Selection = Selection.Create(textArea, lineByNumber1.Offset, lineByNumber2.Offset + lineByNumber2.TotalLength);
        textArea.RemoveSelectedText();
        args.Handled = true;
    }

    private static void OnRemoveLeadingWhitespace(object target, ExecutedRoutedEventArgs args) => TransformSelectedLines((textArea, line) =>
        textArea.Document.Remove(TextUtilities.GetLeadingWhitespace(textArea.Document, line)), target, args, DefaultSegmentType.WholeDocument);

    private static void OnRemoveTrailingWhitespace(object target, ExecutedRoutedEventArgs args) => TransformSelectedLines((textArea, line) =>
        textArea.Document.Remove(TextUtilities.GetTrailingWhitespace(textArea.Document, line)), target, args, DefaultSegmentType.WholeDocument);

    private static void OnConvertTabsToSpaces(object target, ExecutedRoutedEventArgs args) =>
        TransformSelectedSegments(ConvertTabsToSpaces, target, args, DefaultSegmentType.WholeDocument);

    private static void OnConvertLeadingTabsToSpaces(object target, ExecutedRoutedEventArgs args) =>
        TransformSelectedLines((textArea, line) => ConvertTabsToSpaces(textArea, TextUtilities.GetLeadingWhitespace(textArea.Document, line)), target, args, DefaultSegmentType.WholeDocument);

    private static void ConvertTabsToSpaces(TextArea textArea, ISegment segment) {
        TextDocument document = textArea.Document;
        int endOffset = segment.EndOffset;
        string text = new string(' ', textArea.Options.IndentationSize);
        for (int offset = segment.Offset; offset < endOffset; ++offset) {
            if (document.GetCharAt(offset) == '\t') {
                document.Replace(offset, 1, text, OffsetChangeMappingType.CharacterReplace);
                endOffset += text.Length - 1;
            }
        }
    }

    private static void OnConvertSpacesToTabs(object target, ExecutedRoutedEventArgs args) =>
        TransformSelectedSegments(ConvertSpacesToTabs, target, args, DefaultSegmentType.WholeDocument);

    private static void OnConvertLeadingSpacesToTabs(object target, ExecutedRoutedEventArgs args) =>
        TransformSelectedLines((textArea, line) => ConvertSpacesToTabs(textArea, TextUtilities.GetLeadingWhitespace(textArea.Document, line)), target, args, DefaultSegmentType.WholeDocument);

    private static void ConvertSpacesToTabs(TextArea textArea, ISegment segment) {
        TextDocument document = textArea.Document;
        int endOffset = segment.EndOffset;
        int indentationSize = textArea.Options.IndentationSize;
        int num = 0;
        for (int offset = segment.Offset; offset < endOffset; ++offset) {
            if (document.GetCharAt(offset) == ' ') {
                ++num;
                if (num == indentationSize) {
                    document.Replace(offset - (indentationSize - 1), indentationSize, "\t", OffsetChangeMappingType.CharacterReplace);
                    num = 0;
                    offset -= indentationSize - 1;
                    endOffset -= indentationSize - 1;
                }
            } else
                num = 0;
        }
    }

    private static void ConvertCase(Func<string, string> transformText, object target, ExecutedRoutedEventArgs args) {
        TransformSelectedSegments((textArea, segment) => {
            string text = transformText(textArea.Document.GetText(segment));
            textArea.Document.Replace(segment.Offset, segment.Length, text, OffsetChangeMappingType.CharacterReplace);
        }, target, args, DefaultSegmentType.WholeDocument);
    }

    private static void OnConvertToUpperCase(object target, ExecutedRoutedEventArgs args) => ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToUpper, target, args);

    private static void OnConvertToLowerCase(object target, ExecutedRoutedEventArgs args) => ConvertCase(CultureInfo.CurrentCulture.TextInfo.ToLower, target, args);

    private static void OnConvertToTitleCase(object target, ExecutedRoutedEventArgs args) => throw new NotSupportedException();

    private static void OnInvertCase(object target, ExecutedRoutedEventArgs args) => ConvertCase(InvertCase, target, args);

    private static string InvertCase(string text) {
        char[] charArray = text.ToCharArray();
        for (int index = 0; index < charArray.Length; ++index) {
            char c = charArray[index];
            charArray[index] = char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);
        }

        return new string(charArray);
    }

    private static void OnIndentSelection(object target, ExecutedRoutedEventArgs args) {
        TextArea textArea = GetTextArea(target);
        if (textArea?.Document == null || textArea.IndentationStrategy == null) return;
        using (textArea.Document.RunUpdate()) {
            int beginLine;
            int endLine;
            if (textArea.Selection.IsEmpty) {
                beginLine = 1;
                endLine = textArea.Document.LineCount;
            } else {
                beginLine = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.Offset).LineNumber;
                endLine = textArea.Document.GetLineByOffset(textArea.Selection.SurroundingSegment.EndOffset).LineNumber;
            }

            textArea.IndentationStrategy.IndentLines(textArea.Document, beginLine, endLine);
        }

        textArea.Caret.BringCaretToView();
        args.Handled = true;
    }

    private enum DefaultSegmentType {
        WholeDocument,
        CurrentLine,
    }
}
