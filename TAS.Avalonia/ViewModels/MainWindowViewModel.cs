using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ReactiveUI;
using TAS.Core;
using TAS.Core.Models;
using TAS.Core.Services;

namespace TAS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleHitboxesCommand { get; }

    private Action<KeyEventArgs>? _keyDownAction;
    public Action<KeyEventArgs>? KeyDownAction
    {
        get => _keyDownAction;
        set => this.RaiseAndSetIfChanged(ref _keyDownAction, value);
    }

    private Action<KeyEventArgs>? _keyUpAction;
    public Action<KeyEventArgs>? KeyUpAction
    {
        get => _keyUpAction;
        set => this.RaiseAndSetIfChanged(ref _keyUpAction, value);
    }

    private IDocument? _document;
    public IDocument? Document
    {
        get => _document;
        set => this.RaiseAndSetIfChanged(ref _document, value);
    }

    private TextViewPosition _caretPosition;
    public TextViewPosition CaretPosition
    {
        get => _caretPosition;
        set
        {
            var oldValue = _caretPosition;
            this.RaiseAndSetIfChanged(ref _caretPosition, value);
            if (_caretPosition.Line != oldValue.Line) HandleLineChange();
        }
    }

    private readonly ICelesteService _celesteService;

    public MainWindowViewModel()
    {
        _celesteService = TinyIoCContainer.Current.Resolve<ICelesteService>();
        NewFileCommand = ReactiveCommand.Create(NewFile);
        ToggleHitboxesCommand = ReactiveCommand.Create(ToggleHitboxes);

        var file = File.ReadAllText("/Users/shane/Celeste/Celeste.tas");
        Document = new TextDocument(file);

        KeyDownAction = KeyDown;
        KeyUpAction = KeyUp;
    }

    private void ToggleHitboxes()
    {
        _celesteService.ToggleHitboxes();
    }

    private void NewFile()
    {
        Console.WriteLine("New File!");
    }

    private void KeyUp(KeyEventArgs args)
    {
        // NOOP
    }

    private void KeyDown(KeyEventArgs args)
    {
        var caretPosition = CaretPosition;
        if (Document is not { } document) return;
        if (document.GetLineByNumber(caretPosition.Line) is not { } line) return;
        if (document.GetText(line) is not { } lineText) return;

        var numberForKey = args.Key.NumberForKey();
        var firstChar = lineText.TrimStart().FirstOrDefault();

        // if it starts with a number, it's a TAS input
        if (lineText.Length > 0 && firstChar >= '0' && firstChar <= '9')
        {
            var tokens = lineText.Trim().Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var frames = int.TryParse(tokens[0], out var val) ? val : 0;

            TASAction actions = tokens.Skip(1).Aggregate(TASAction.None, (a, s) =>
            {
                if (string.IsNullOrWhiteSpace(s)) return a;
                var actionForChar = TASActionExtensions.ActionForChar(s[0]);
                return a | actionForChar;
            });

            // if we entered an action
            var typedAction = args.Key.ActionForKey();
            if (typedAction != TASAction.None)
            {
                // toggle it
                actions = actions.ToggleAction(typedAction);
                // warp the cursor after the number
                caretPosition.Column = 5;
            }
            // if the key we entered is a number
            else if (numberForKey >= 0)
            {
                // if the caret is after the number, or before the number and we typed 0, warp the cursor
                if (caretPosition.Column > 5 || caretPosition.Column <= 5 - frames.Digits() && numberForKey == 0)
                    caretPosition.Column = 5;
                // else update the number
                else if (caretPosition.Column <= 5)
                {
                    var cursorIndex = (5 - caretPosition.Column);
                    var rightOfCursor = tokens[0][^cursorIndex..];
                    var leftOfCursor = tokens[0][..^cursorIndex];
                    frames = int.Parse($"{leftOfCursor}{numberForKey}{rightOfCursor}");

                    // if we've made the frames over 9999 then cap it and warp
                    if (frames >= 10000)
                    {
                        frames = 9999;
                        caretPosition.Column = 5;
                    }
                }
            }
            // otherwise we should just NOOP (for now)
            else
                return;

            // we've handled the input, so format the line and warp the cursor
            args.Handled = true;
            var newLine = actions.Sorted().Aggregate(frames.ToString().PadLeft(4), (s, a) => s + "," + a.CharForAction());
            document.Replace(line.Offset, line.Length, newLine);
            CaretPosition = caretPosition;
        }
        // if we're typing a number, check to see if we're starting a new action line
        else if (numberForKey >= 0 && caretPosition.Column <= lineText.Length - lineText.TrimStart().Length)
        {
            args.Handled = true;
            document.Replace(line.Offset, line.Length, numberForKey.ToString().PadLeft(4));
            caretPosition.Column = 5;
            CaretPosition = caretPosition;
        }
    }

    private void HandleLineChange()
    {
        var caretPosition = CaretPosition;
        if (Document is not { } document) return;
        if (document.GetLineByNumber(caretPosition.Line) is not { } line) return;
        if (document.GetText(line) is not { } lineText) return;
        var firstChar = lineText.TrimStart().FirstOrDefault();

        if (lineText.Length > 0 && firstChar >= '0' && firstChar <= '9')
        {
            caretPosition.Column = 5;
            CaretPosition = caretPosition;
        }
    }
}
