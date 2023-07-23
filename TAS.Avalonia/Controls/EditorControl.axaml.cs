using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using TAS.Avalonia.Editing;
using TAS.Avalonia.Models;
using TAS.Avalonia.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Model;

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
        editor.TextArea.PushStackedInputHandler(new TASStackedInputHandler(editor.TextArea));
        editor.TextArea.Caret.PositionChanged += (_, _) => CaretPosition = editor.TextArea.Caret.Position;
        editor.TextArea.TextEntering += (_, e) => {
            if (e.Text.Length != 1 || e.Text[0] is not >= 'a' and <= 'z' or >= 'A' and <= 'Z') return;

            var key = e.Text[0] switch {
                >= 'a' and <= 'z' => e.Text[0] - 'a' + Key.A,
                >= 'A' and <= 'Z' => e.Text[0] - 'A' + Key.A,
                _ => Key.None,
            };

            e.Handled = TASStackedInputHandler.HandleActionInput(editor.TextArea, key);
        };
        editor.TextArea.TextView.BackgroundRenderers.Add(new TASLineRenderer(editor.TextArea));

        PropertyChanged += (_, e) => {
            if (e.Property == CaretPositionProperty) {
                editor.TextArea.Caret.Position = (TextViewPosition) e.NewValue!;
            }
        };
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
}
