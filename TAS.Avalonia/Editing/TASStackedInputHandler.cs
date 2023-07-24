using Avalonia.Input;
using AvaloniaEdit.Editing;
using TAS.Avalonia.Models;

namespace TAS.Avalonia.Editing;

public class TASStackedInputHandler : TextAreaStackedInputHandler {
    public TASStackedInputHandler(TextArea textArea) : base(textArea) {
    }

    public override void OnPreviewKeyDown(KeyEventArgs e) {
        var gesture = new KeyGesture(e.Key, e.KeyModifiers);
        if (gesture.ValidForAction())
            e.Handled = HandleActionInput(TextArea, e.Key);
    }

    public static bool HandleActionInput(TextArea textArea, Key key) {
        // get the text if possible
        var caretPosition = textArea.Caret.Position;
        if (textArea.Document is not { } document ||
            document.GetLineByNumber(caretPosition.Line) is not { } line ||
            document.GetText(line) is not { } lineText) {
            // something exploded
            return false;
        }

        int leadingSpaces = lineText.Length - lineText.TrimStart().Length;

        int numberForKey = key.NumberForKey();
        bool startOfLine = caretPosition.Column <= leadingSpaces + 1;

        bool handled = false;

        // if it's a TAS action line, handle it ourselves
        if (TASActionLine.TryParse(lineText, out var actionLine)) {
            handled = true;

            int customBindStart = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.CustomBinding);
            int customBindEnd = customBindStart + actionLine.CustomBindings.Count;
            char typedCharacter = key.CharacterForKey();
            if (customBindStart != -1 && caretPosition.Column >= customBindStart && caretPosition.Column <= customBindEnd && typedCharacter != '\0') {
                if (actionLine.CustomBindings.Contains(typedCharacter)) {
                    actionLine.CustomBindings.Remove(typedCharacter);
                    caretPosition.Column = customBindEnd - 1;
                } else {
                    actionLine.CustomBindings.Add(typedCharacter);
                    caretPosition.Column = customBindEnd + 1;
                }

                goto FinishEdit; // Skip regular logic
            }

            var typedAction = key.ActionForKey();

            // Handle feather inputs
            var featherStartColumn = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.FeatherAim);
            if (featherStartColumn >= 1 && caretPosition.Column > featherStartColumn && (key is Key.OemPeriod or Key.OemComma || numberForKey != -1)) {
                string text = key switch {
                    Key.OemPeriod => ".",
                    Key.OemComma => ",",
                    _ => numberForKey.ToString(),
                };

                lineText = lineText.Insert(caretPosition.Column - 1, text);
                if (TASActionLine.TryParse(lineText, out var newActionLine, ignoreInvalidFloats: false)) {
                    actionLine = newActionLine;
                    caretPosition.Column++;
                }
            } else if (typedAction is TASAction.DashOnly or TASAction.MoveOnly or TASAction.CustomBinding) {
                actionLine.Actions = actionLine.Actions.ToggleAction(typedAction);
                caretPosition.Column = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, typedAction);
            }
            // if we entered an action
            else if (typedAction != TASAction.None) {
                int dashOnlyStart = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.DashOnly);
                int dashOnlyEnd = dashOnlyStart + actionLine.Actions.GetDashOnly().Count();
                if (dashOnlyStart != -1 && caretPosition.Column >= dashOnlyStart && caretPosition.Column <= dashOnlyEnd)
                    typedAction = typedAction.ToDashOnlyDirection();

                int moveOnlyStart = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.MoveOnly);
                int moveOnlyEnd = moveOnlyStart + actionLine.Actions.GetMoveOnly().Count();
                if (moveOnlyStart != -1 && caretPosition.Column >= moveOnlyStart && caretPosition.Column <= moveOnlyEnd)
                    typedAction = typedAction.ToMoveOnlyDirection();

                // toggle it
                actionLine.Actions = actionLine.Actions.ToggleAction(typedAction);
                // warp the cursor after the number
                if (typedAction == TASAction.FeatherAim && actionLine.Actions.HasFlag(TASAction.FeatherAim)) {
                    caretPosition.Column = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.FeatherAim) + 1;
                } else if (typedAction == TASAction.FeatherAim && !actionLine.Actions.HasFlag(TASAction.FeatherAim)) {
                    actionLine.FeatherAngle = null;
                    actionLine.FeatherMagnitude = null;
                    caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
                } else if (typedAction is TASAction.LeftDashOnly or TASAction.RightDashOnly or TASAction.UpDashOnly or TASAction.DownDashOnly) {
                    caretPosition.Column = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.DashOnly) + actionLine.Actions.GetDashOnly().Count();
                } else if (typedAction is TASAction.LeftMoveOnly or TASAction.RightMoveOnly or TASAction.UpMoveOnly or TASAction.DownMoveOnly) {
                    caretPosition.Column = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.MoveOnly) + actionLine.Actions.GetMoveOnly().Count();
                } else {
                    caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
                }
            }
              // if the key we entered is a number
              else if (numberForKey >= 0) {
                int cursorPosition = caretPosition.Column - leadingSpaces - 1;

                // entering a zero at the start should do nothing but format
                if (cursorPosition == 0 && numberForKey == 0)
                    caretPosition.Column = TASActionLine.MaxFramesDigits - actionLine.Frames.Digits() + 1;
                // if we have a 0, just force the new number
                else if (actionLine.Frames == 0) {
                    actionLine.Frames = numberForKey;
                    caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
                } else {
                    // jam the number into the current position
                    string leftOfCursor = lineText[..(caretPosition.Column - 1)];
                    string rightOfCursor = lineText[(caretPosition.Column - 1)..];
                    lineText = $"{leftOfCursor}{numberForKey}{rightOfCursor}";

                    // reparse
                    TASActionLine.TryParse(lineText, out actionLine);

                    // cap at max frames
                    if (actionLine.Frames > TASActionLine.MaxFrames) {
                        actionLine.Frames = TASActionLine.MaxFrames;
                        caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
                    } else {
                        caretPosition.Column = TASActionLine.MaxFramesDigits - actionLine.Frames.Digits() + cursorPosition + 2;
                    }
                }
            }

            FinishEdit:
            document.Replace(line, actionLine.ToString());
        }
        // start a TAS action line if we should
        else if (startOfLine && numberForKey >= 0) {
            handled = true;
            string newLine = numberForKey.ToString().PadLeft(TASActionLine.MaxFramesDigits);
            if (lineText.Trim().Length == 0)
                document.Replace(line, newLine);
            else
                document.Insert(line.Offset, newLine + "\n");
            caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
        }

        line = document.GetLineByNumber(caretPosition.Line);
        caretPosition.Column = Math.Clamp(caretPosition.Column, 1, line.Length + 1);
        caretPosition.VisualColumn = caretPosition.Column - 1;
        textArea.Caret.Position = caretPosition;

        return handled;
    }
}
