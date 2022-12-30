using System;
using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using TAS.Avalonia.Editing;
using TextMateSharp.Grammars;

namespace TAS.Avalonia.Controls;

public partial class EditorControl : UserControl
{
    public static readonly StyledProperty<TextDocument> DocumentProperty = TextView.DocumentProperty.AddOwner<TextEditor>();
    public static readonly StyledProperty<TextViewPosition> CaretPositionProperty = AvaloniaProperty.Register<EditorControl, TextViewPosition>(nameof(CaretPosition));

    public TextViewPosition CaretPosition
    {
        get => GetValue(CaretPositionProperty);
        set => SetValue(CaretPositionProperty, value);
    }

    public TextDocument Document
    {
        get => GetValue(TextEditor.DocumentProperty);
        set => SetValue(TextEditor.DocumentProperty, value);
    }

    private ThemeName _currentTheme = ThemeName.SolarizedDark;
    private readonly RegistryOptions _registryOptions;
    private readonly TextMate.Installation _textMateInstallation;

    public EditorControl()
    {
        InitializeComponent();
        editor.ShowLineNumbers = true;
        editor.TextArea.IndentationStrategy = null;
        DocumentProperty.Changed.Subscribe(args => editor.Document = args.NewValue.Value);

        _registryOptions = new RegistryOptions(_currentTheme);
        _textMateInstallation = editor.InstallTextMate(_registryOptions);
        var csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
        editor.TextArea.ActiveInputHandler = new TASInputHandler(editor.TextArea);
        editor.TextArea.PushStackedInputHandler(new TASStackedInputHandler(editor.TextArea));
        editor.TextArea.Caret.PositionChanged += (_, _) => CaretPosition = editor.TextArea.Caret.Position;

        PropertyChanged += (_, e) =>
        {
            if (e.Property == CaretPositionProperty)
                editor.TextArea.Caret.Position = (TextViewPosition)e.NewValue!;
        };
    }
}
