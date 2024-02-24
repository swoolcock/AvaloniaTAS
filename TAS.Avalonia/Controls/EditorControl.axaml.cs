using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using TAS.Avalonia.Editing;
using TAS.Avalonia.Models;
using TAS.Avalonia.Rendering;
using TextMateSharp.Grammars;

namespace TAS.Avalonia.Controls;

public partial class EditorControl : UserControl {
    public static readonly StyledProperty<TASDocument> DocumentProperty = AvaloniaProperty.Register<EditorControl, TASDocument>(nameof(Document));
    public static readonly StyledProperty<TextViewPosition> CaretPositionProperty = AvaloniaProperty.Register<EditorControl, TextViewPosition>(nameof(CaretPosition));

    public TextViewPosition CaretPosition {
        get => GetValue(CaretPositionProperty);
        set => SetValue(CaretPositionProperty, value);
    }

    public TASDocument Document {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    private ThemeName _currentTheme = ThemeName.SolarizedDark;
    private readonly RegistryOptions _registryOptions;
    private readonly TextMate.Installation _textMateInstallation;

    public EditorControl() {
        InitializeComponent();
        editor.ShowLineNumbers = true;
        editor.TextArea.IndentationStrategy = null;
        DocumentProperty.Changed.Subscribe(args => {
            editor.Document = args.NewValue.Value.Document;
            editor.Document.Changing += Document_Changing;
            editor.Document.Changed += Document_Changed;
            editor.Document.TextChanged += Document_TextChanged;
            // if it's a small file, jump to the end (covers new documents), otherwise the start
            editor.TextArea.Caret.Position = new TextViewPosition(editor.Document.GetLocation(editor.Document.LineCount <= 10 ? editor.Document.TextLength : 0));
        });

        _registryOptions = new RegistryOptions(_currentTheme);
        _textMateInstallation = editor.InstallTextMate(_registryOptions);
        var csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
        editor.TextArea.ActiveInputHandler = new TASInputHandler(editor.TextArea);
        editor.TextArea.Caret.PositionChanged += (_, _) => CaretPosition = editor.TextArea.Caret.Position;
        editor.TextArea.AddHandler(TextInputEvent, HandleActionInput, RoutingStrategies.Tunnel);
        editor.TextArea.TextView.BackgroundRenderers.Add(new TASLineRenderer(editor.TextArea));

        ApplyTASLineNumbers();

        PropertyChanged += (_, e) => {
            if (e.Property == CaretPositionProperty) {
                editor.TextArea.Caret.Position = (TextViewPosition) e.NewValue!;
            }
        };

        int prevLine = 0;
        (Application.Current as App).CelesteService.Server.StateUpdated += state => {
            if (state.CurrentLine == prevLine) return;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                const int LinesBelow = 3;
                const int LinesAbove = 3;

                var view = editor.TextArea.TextView;
                editor.TextArea.Caret.Line = state.CurrentLine + 1;

                // Below
                {
                    var visualLine = view.GetOrConstructVisualLine(view.Document.GetLineByNumber(editor.TextArea.Caret.Line + LinesBelow));
                    var textLine = visualLine.GetTextLine(0, false);

                    double lineTop = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.TextTop);
                    double lineBottom = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.TextBottom);

                    view.MakeVisible(new Rect(0, lineTop, 0, lineBottom - lineTop));
                }

                // Above
                {
                    var visualLine = view.GetOrConstructVisualLine(view.Document.GetLineByNumber(editor.TextArea.Caret.Line - LinesAbove));
                    var textLine = visualLine.GetTextLine(0, false);

                    double lineTop = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.TextTop);
                    double lineBottom = visualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.TextBottom);

                    view.MakeVisible(new Rect(0, lineTop, 0, lineBottom - lineTop));
                }
            });
            prevLine = state.CurrentLine;
        };
    }

    private void ApplyTASLineNumbers() {
        var leftMargins = editor.TextArea.LeftMargins;
        for (int i = 0; i < leftMargins.Count; i++) {
            if (leftMargins[i] is LineNumberMargin) {
                leftMargins.RemoveAt(i);
                if (i < leftMargins.Count && DottedLineMargin.IsDottedLineMargin(leftMargins[i])) {
                    leftMargins.RemoveAt(i);
                }
                break;
            }
        }

        var lineNumbers = new TASLineNumberMargin();
        var line = (Line) DottedLineMargin.Create();
        leftMargins.Insert(0, lineNumbers);
        leftMargins.Insert(1, line);
        var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = editor };
        line.Bind(Shape.StrokeProperty, lineNumbersForeground);
        lineNumbers.Bind(ForegroundProperty, lineNumbersForeground);
    }

    private bool _discardUpdateEvents = false; // Avoid infinite loops
    private int _insertionLineStart, _insertionLineEnd;
    private int _removalLineStart, _removalLineEnd;
    private void Document_Changing(object sender, DocumentChangeEventArgs args) {
        if (_discardUpdateEvents || args.RemovalLength <= 0) return;

        _removalLineStart = editor.Document.GetLineByOffset(args.Offset).LineNumber;
        _removalLineEnd = editor.Document.GetLineByOffset(args.Offset + args.RemovalLength).LineNumber;
    }
    private void Document_Changed(object sender, DocumentChangeEventArgs args) {
        if (_discardUpdateEvents || args.InsertionLength <= 0) return;

        _insertionLineStart = editor.Document.GetLineByOffset(args.Offset).LineNumber;
        _insertionLineEnd = editor.Document.GetLineByOffset(args.Offset + args.InsertionLength).LineNumber;
    }
    private void Document_TextChanged(object sender, EventArgs args) {
        _discardUpdateEvents = true;

        if (_insertionLineStart > 0 && _insertionLineEnd > 0)
            TASEditingCommandHandler.AutoFormatActionLines(editor.TextArea, _insertionLineStart, Math.Min(_insertionLineEnd, editor.Document.LineCount));
        if (_removalLineStart > 0 && _removalLineEnd > 0)
            TASEditingCommandHandler.AutoFormatActionLines(editor.TextArea, _removalLineStart, Math.Min(_removalLineEnd, editor.Document.LineCount));

        _insertionLineStart = _insertionLineEnd = _removalLineStart = _removalLineEnd = 0;
        _discardUpdateEvents = false;
    }

    private static void HandleActionInput(TextArea textArea, TextInputEventArgs e) {
        // We can only handle single characters
        if (e.Text is not { Length: 1 }) return;

        // Get the text if possible
        var caretPosition = textArea.Caret.Position;
        if (textArea.Document is not { } document ||
            document.GetLineByNumber(caretPosition.Line) is not { } line ||
            document.GetText(line) is not { } lineText)
        {
            // Something exploded
            return;
        }

        char typedCharacter = char.ToUpper(e.Text[0]);
        int leadingSpaces = lineText.Length - lineText.TrimStart().Length;
        bool startOfLine = caretPosition.Column <= leadingSpaces + 1;

        // If it's a TAS action line, handle it ourselves
        if (TASActionLine.TryParse(lineText, out var actionLine)) {
            e.Handled = true;

            // Handle custom bindings
            int customBindStart = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.CustomBinding);
            int customBindEnd = customBindStart + actionLine.CustomBindings.Count;
            if (customBindStart != -1 && caretPosition.Column >= customBindStart && caretPosition.Column <= customBindEnd && typedCharacter is >= 'A' and <= 'Z') {
                if (actionLine.CustomBindings.Contains(typedCharacter)) {
                    actionLine.CustomBindings.Remove(typedCharacter);
                    caretPosition.Column = customBindEnd - 1;
                } else {
                    actionLine.CustomBindings.Add(typedCharacter);
                    caretPosition.Column = customBindEnd + 1;
                }

                goto FinishEdit; // Skip regular logic
            }

            var typedAction = typedCharacter.ActionForChar();

            // Handle feather inputs
            int featherStartColumn = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.FeatherAim);
            if (featherStartColumn >= 1 && caretPosition.Column > featherStartColumn && (typedCharacter is '.' or ',' or (>= '0' and <= '9'))) {
                lineText = lineText.Insert(caretPosition.Column - 1, typedCharacter.ToString());
                if (TASActionLine.TryParse(lineText, out var newActionLine, ignoreInvalidFloats: false)) {
                    actionLine = newActionLine;
                    caretPosition.Column++;
                }
            }
            // Handle dash-only/move-only/custom bindings
            else if (typedAction is TASAction.DashOnly or TASAction.MoveOnly or TASAction.CustomBinding) {
                actionLine.Actions = actionLine.Actions.ToggleAction(typedAction);
                caretPosition.Column = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, typedAction);
            }
            // Handle regular inputs
            else if (typedAction != TASAction.None) {
                int dashOnlyStart = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.DashOnly);
                int dashOnlyEnd = dashOnlyStart + actionLine.Actions.GetDashOnly().Count();
                if (dashOnlyStart != -1 && caretPosition.Column >= dashOnlyStart && caretPosition.Column <= dashOnlyEnd)
                    typedAction = typedAction.ToDashOnlyDirection();

                int moveOnlyStart = TASCaretNavigationCommandHandler.GetColumnOfAction(actionLine, TASAction.MoveOnly);
                int moveOnlyEnd = moveOnlyStart + actionLine.Actions.GetMoveOnly().Count();
                if (moveOnlyStart != -1 && caretPosition.Column >= moveOnlyStart && caretPosition.Column <= moveOnlyEnd)
                    typedAction = typedAction.ToMoveOnlyDirection();

                // Toggle it
                actionLine.Actions = actionLine.Actions.ToggleAction(typedAction);

                // Warp the cursor after the number
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
            // If the key we entered is a number
            else if (typedCharacter is >= '0' and <= '9') {
                int cursorPosition = caretPosition.Column - leadingSpaces - 1;

                // Entering a zero at the start should do nothing but format
                if (cursorPosition == 0 && typedCharacter == '0') {
                    caretPosition.Column = TASActionLine.MaxFramesDigits - actionLine.Frames.Digits() + 1;
                }
                // If we have a 0, just force the new number
                else if (actionLine.Frames == 0) {
                    actionLine.Frames = int.Parse(typedCharacter.ToString());
                    caretPosition.Column = TASActionLine.MaxFramesDigits + 1;
                } else {
                    // Jam the number into the current position
                    string leftOfCursor = lineText[..(caretPosition.Column - 1)];
                    string rightOfCursor = lineText[(caretPosition.Column - 1)..];
                    lineText = $"{leftOfCursor}{typedCharacter}{rightOfCursor}";

                    // Reparse
                    TASActionLine.TryParse(lineText, out actionLine);

                    // Cap at max frames
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
        // Start a TAS action line if we should
        else if (startOfLine && typedCharacter is >= '0' and <= '9') {
            e.Handled = true;

            string newLine = typedCharacter.ToString().PadLeft(TASActionLine.MaxFramesDigits);
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
    }
}
