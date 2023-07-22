#define AUTOMATIC_CURSOR

using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TAS.Avalonia.Editing;
using TAS.Avalonia.Models;
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
            // if it's a small file, jump to the end (covers new documents), otherwise the start
            editor.TextArea.Caret.Position = new TextViewPosition(editor.Document.GetLocation(editor.Document.LineCount <= 10 ? editor.Document.TextLength : 0));
        });

        _registryOptions = new RegistryOptions(_currentTheme);
        _textMateInstallation = editor.InstallTextMate(_registryOptions);
        var csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
#if AUTOMATIC_CURSOR
        editor.TextArea.ActiveInputHandler = new TASInputHandler(editor.TextArea);
        editor.TextArea.PushStackedInputHandler(new TASStackedInputHandler(editor.TextArea));
        editor.TextArea.Caret.PositionChanged += (_, _) => CaretPosition = editor.TextArea.Caret.Position;
#endif
        PropertyChanged += (_, e) => {
            if (e.Property == CaretPositionProperty) {
                editor.TextArea.Caret.Position = (TextViewPosition) e.NewValue!;
            }
        };
    }
}
