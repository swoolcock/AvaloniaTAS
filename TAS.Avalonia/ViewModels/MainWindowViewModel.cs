using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
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
            // if (value.Line != oldValue.Line)
            //     value = HandleLineChange(value);
            this.RaiseAndSetIfChanged(ref _caretPosition, value);
        }
    }

    public bool MenuVisible => !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private readonly ICelesteService _celesteService;

    public MainWindowViewModel()
    {
        _celesteService = TinyIoCContainer.Current.Resolve<ICelesteService>();
        NewFileCommand = ReactiveCommand.Create(NewFile);
        ToggleHitboxesCommand = ReactiveCommand.Create(ToggleHitboxes);

        try
        {
            var file = File.ReadAllText("/Users/shane/Celeste/Celeste.tas");
            Document = new TextDocument(file);
        }
        catch
        {
            Document = new TextDocument();
        }

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
        try
        {
            // cache the current line
            var caretPosition = CaretPosition;
            if (Document is not { } document) return;
            if (document.GetLineByNumber(caretPosition.Line) is not { } line) return;
            if (document.GetText(line) is not { } lineText) return;
            // var firstChar = lineText.TrimStart().FirstOrDefault();
            var numberForKey = args.Key.NumberForKey();

            // we handle arrow keys ourselves
            if (HandleArrowKeys(args)) return;

            // if it starts with a number, it's a TAS input
            if (IsActionLine(lineText))
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
            else if (numberForKey >= 0 && caretPosition.Column <= lineText.Length - lineText.TrimStart().Length + 1)
            {
                args.Handled = true;
                document.Insert(line.Offset, numberForKey.ToString().PadLeft(4) + "\n");
                caretPosition.Column = 5;
                CaretPosition = caretPosition;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }

    private bool HandleArrowKeys(KeyEventArgs args)
    {
        var caretPosition = CaretPosition;
        if (!TryGetLine(caretPosition, out var document, out var line, out var lineText)) return false;

        // ignore non-arrow keys
        if (args.Key is not (Key.Up or Key.Down or Key.Left or Key.Right)) return false;

        // consume event
        args.Handled = true;

        var startOfLine = caretPosition.Column == 1 || IsActionLine(lineText) && lineText.ElementAtOrDefault(caretPosition.Column - 2) == ' ';
        var endOfLine = caretPosition.Column == lineText.Length + 1 || IsActionLine(lineText) && caretPosition.Column == 5;
        var lineLength = lineText.Length;

        if ((startOfLine && args.Key == Key.Left || args.Key == Key.Up) && caretPosition.Line > 1)
            caretPosition.Line--;
        else if ((endOfLine && args.Key == Key.Right || args.Key == Key.Down) && caretPosition.Line < document.LineCount)
            caretPosition.Line++;
        else if (!startOfLine && args.Key == Key.Left)
            caretPosition.Column--;
        else if (!endOfLine && args.Key == Key.Right)
            caretPosition.Column++;

        if (caretPosition.Line != CaretPosition.Line && TryGetLine(caretPosition, out _, out _, out var newLineText))
        {
            var leadingSpaces = newLineText.Length - newLineText.TrimStart().Length;
            lineLength = newLineText.Length;
            if (args.Key == Key.Left)
                caretPosition.Column = IsActionLine(newLineText) ? 5 : lineLength + 1;
            else if (args.Key == Key.Right)
                caretPosition.Column = IsActionLine(newLineText) ? leadingSpaces + 1 : 1;
            else
                caretPosition.Column = IsActionLine(newLineText) ? Math.Clamp(caretPosition.Column, leadingSpaces + 1, 5) : Math.Clamp(caretPosition.Column, 1, lineLength + 1);
            caretPosition.IsAtEndOfLine = caretPosition.Column == lineLength;
        }

        CaretPosition = caretPosition;
        return true;
    }

    private static bool IsActionLine(string line)
    {
        var c = line.TrimStart().FirstOrDefault();
        return c >= '0' && c <= '9';
    }

    private bool IsActionLine(int lineNumber)
    {
        if (Document is not { } document) return false;
        if (document.GetLineByNumber(lineNumber) is not { } line) return false;
        if (document.GetText(line) is not { } lineText) return false;
        var c = lineText.TrimStart().FirstOrDefault();
        return c >= '0' && c <= '9';
    }

    private bool TryGetLine(TextViewPosition caretPosition, out IDocument document, out IDocumentLine line, out string lineText)
    {
        document = Document;
        line = null;
        lineText = null;
        if (document == null) return false;

        line = document.GetLineByNumber(caretPosition.Line);
        if (line == null) return false;

        lineText = document.GetText(line);
        return lineText != null;
    }
}
