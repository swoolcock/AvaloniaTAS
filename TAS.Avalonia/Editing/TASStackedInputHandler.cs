using Avalonia.Input;
using AvaloniaEdit.Editing;
using TAS.Avalonia.Models;

namespace TAS.Avalonia.Editing;

public class TASStackedInputHandler : TextAreaStackedInputHandler {
    public TASStackedInputHandler(TextArea textArea) : base(textArea) {
    }

    public override void OnPreviewKeyDown(KeyEventArgs e) {
        // let bindings handle it first
        var inputHandler = (TASInputHandler) TextArea.ActiveInputHandler;
        if (inputHandler.AllKeyBindings.FirstOrDefault(b => b.Gesture.Matches(e)) is { } _ ||
            inputHandler.AllCommandBindings.FirstOrDefault(b => b.Command.Gesture?.Matches(e) ?? false) is { } _) {
            // matched, skip for now
            return;
        }

        // get the text if possible
        var caretPosition = TextArea.Caret.Position;
        if (TextArea.Document is not { } document ||
            document.GetLineByNumber(caretPosition.Line) is not { } line ||
            document.GetText(line) is not { } lineText) {
            // something exploded
            return;
        }

        int leadingSpaces = lineText.Length - lineText.TrimStart().Length;
        var gesture = new KeyGesture(e.Key, e.KeyModifiers);
        int numberForKey = e.Key.NumberForKey();
        bool validForAction = gesture.ValidForAction();
        bool startOfLine = caretPosition.Column <= leadingSpaces + 1;

        // if it's a TAS action line, handle it ourselves
        if (TASActionLine.TryParse(lineText, out var actionLine)) {
            e.Handled = true;

            // allow comments anywhere (hardcoded to shift+3 for now, we'll move to TextEntering later)
            if (e.KeyModifiers == KeyModifiers.Shift && e.Key == Key.D3) {
                document.Insert(line.Offset, "#");
                return;
            }

            // break if it's not a valid key
            if (!validForAction) return;

            // if we entered an action
            var typedAction = e.Key.ActionForKey();
            if (typedAction != TASAction.None) {
                // toggle it
                actionLine.Actions = actionLine.Actions.ToggleAction(typedAction);
                // warp the cursor after the number
                caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
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

            document.Replace(line, actionLine.ToString());
        }
        // start a TAS action line if we should
        else if (startOfLine && validForAction && numberForKey >= 0) {
            e.Handled = true;
            string newLine = numberForKey.ToString().PadLeft(TASActionLine.MaxFramesDigits);
            if (lineText.Trim().Length == 0)
                document.Replace(line, newLine);
            else
                document.Insert(line.Offset, newLine + "\n");
            caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
        }

        caretPosition.VisualColumn = caretPosition.Column - 1;
        TextArea.Caret.Position = caretPosition;
    }
}
