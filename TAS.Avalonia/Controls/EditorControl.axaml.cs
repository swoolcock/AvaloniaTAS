using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace TAS.Avalonia.Controls;

public partial class EditorControl : UserControl
{
    public static readonly StyledProperty<TextDocument> DocumentProperty = TextView.DocumentProperty.AddOwner<TextEditor>();
    public static readonly StyledProperty<Action<KeyEventArgs>?> KeyDownActionProperty = AvaloniaProperty.Register<EditorControl, Action<KeyEventArgs>?>(nameof(KeyDownAction));
    public static readonly StyledProperty<Action<KeyEventArgs>?> KeyUpActionProperty = AvaloniaProperty.Register<EditorControl, Action<KeyEventArgs>?>(nameof(KeyUpAction));
    public static readonly StyledProperty<TextViewPosition> CaretPositionProperty = AvaloniaProperty.Register<EditorControl, TextViewPosition>(nameof(CaretPosition));

    public TextViewPosition CaretPosition
    {
        get => GetValue(CaretPositionProperty);
        set => SetValue(CaretPositionProperty, value);
    }

    public Action<KeyEventArgs>? KeyDownAction
    {
        get => GetValue(KeyDownActionProperty);
        set => SetValue(KeyDownActionProperty, value);
    }

    public Action<KeyEventArgs>? KeyUpAction
    {
        get => GetValue(KeyUpActionProperty);
        set => SetValue(KeyUpActionProperty, value);
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
        editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options);
        DocumentProperty.Changed.Subscribe(args => editor.Document = args.NewValue.Value);

        _registryOptions = new RegistryOptions(_currentTheme);
        _textMateInstallation = editor.InstallTextMate(_registryOptions);
        var csharpLanguage = _registryOptions.GetLanguageByExtension(".cs");
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(csharpLanguage.Id));
        editor.TextArea.PushStackedInputHandler(new CustomInputHandler(editor.TextArea, this));
        editor.TextArea.Caret.PositionChanged += (_, _) => CaretPosition = editor.TextArea.Caret.Position;

        PropertyChanged += (_, e) =>
        {
            if (e.Property == CaretPositionProperty)
                editor.TextArea.Caret.Position = (TextViewPosition)e.NewValue!;
        };
    }

    private class CustomInputHandler : TextAreaStackedInputHandler
    {
        private readonly EditorControl _parent;

        public CustomInputHandler(TextArea textArea, EditorControl parent) : base(textArea)
        {
            _parent = parent;
        }

        public override void OnPreviewKeyDown(KeyEventArgs e)
        {
            _parent.KeyDownAction?.Invoke(e);
        }

        public override void OnPreviewKeyUp(KeyEventArgs e)
        {
            _parent.KeyUpAction?.Invoke(e);
        }
    }
}
