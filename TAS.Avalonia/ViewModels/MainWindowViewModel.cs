using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ReactiveUI;
using TAS.Avalonia.Controls;
using TAS.Core;
using TAS.Core.Models;
using TAS.Core.Services;

namespace TAS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleHitboxesCommand { get; }

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
    }

    private void ToggleHitboxes()
    {
        _celesteService.ToggleHitboxes();
    }

    private void NewFile()
    {
        Console.WriteLine("New File!");
    }

    // private void KeyUp(KeyEventArgs args, RoutedCommand? command)
    // {
    //     // NOOP
    // }
    //
    // private void KeyDown(KeyEventArgs args, RoutedCommand? command)
    // {
    //     try
    //     {
    //         // cache the current line
    //         var caretPosition = CaretPosition;
    //         if (!TryGetLine(caretPosition, out var document, out var line, out var lineText)) return;
    //
    //         var gesture = new KeyGesture(args.Key, args.KeyModifiers);
    //         var numberForKey = args.Key.NumberForKey();
    //
    //         // handle commands
    //         if (HandleControl(args, command))
    //         {
    //             return;
    //         }
    //
    //         // handle TAS lines
    //         if (TASActionLine.TryParse(lineText, out var actionLine))
    //         {
    //             if (args.KeyModifiers != KeyModifiers.None)
    //             {
    //                 // 3
    //             }
    //             // if we entered an action
    //             var typedAction = args.Key.ActionForKey();
    //             if (typedAction != TASAction.None)
    //             {
    //                 // toggle it
    //                 actionLine.Actions = actionLine.Actions.ToggleAction(typedAction);
    //                 // warp the cursor after the number
    //                 caretPosition.Column = 5;
    //             }
    //             // if the key we entered is a number and no key modifiers
    //             else if (numberForKey >= 0 && args.KeyModifiers == KeyModifiers.None)
    //             {
    //                 // if the caret is after the number, or before the number and we typed 0, warp the cursor
    //                 if (caretPosition.Column > 5 || caretPosition.Column <= 5 - actionLine.Frames.Digits() && numberForKey == 0)
    //                     caretPosition.Column = 5;
    //                 // else update the number
    //                 else if (caretPosition.Column <= 5)
    //                 {
    //                     var rightOfCursor = caretPosition.Column == 5 ? string.Empty : lineText[(caretPosition.Column - 1)..4];
    //                     var leftOfCursor = lineText[..(caretPosition.Column - 1)];
    //                     actionLine.Frames = int.Parse($"{leftOfCursor}{numberForKey}{rightOfCursor}");
    //
    //                     // if we've made the frames over 9999 then cap it and warp
    //                     if (actionLine.Frames >= 10000)
    //                     {
    //                         actionLine.Frames = 9999;
    //                         caretPosition.Column = 5;
    //                     }
    //                 }
    //             }
    //             // otherwise we should just NOOP (for now)
    //             else
    //                 return;
    //
    //             // we've handled the input, so format the line and warp the cursor
    //             args.Handled = true;
    //             document.Replace(line.Offset, line.Length, actionLine.ToString());
    //             CaretPosition = caretPosition;
    //         }
    //         // if we're typing a number, check to see if we're starting a new action line
    //         else if (numberForKey >= 0 && caretPosition.Column <= lineText.Length - lineText.TrimStart().Length + 1)
    //         {
    //             args.Handled = true;
    //             var newLine = numberForKey.ToString().PadLeft(4);
    //             if (!string.IsNullOrWhiteSpace(lineText)) newLine += "\n";
    //             document.Insert(line.Offset, newLine);
    //             caretPosition.Column = 5;
    //             CaretPosition = caretPosition;
    //         }
    //     }
    //     catch(Exception e)
    //     {
    //         Console.WriteLine(e.StackTrace);
    //     }
    // }
    //
    // private bool HandleControl(KeyEventArgs args, RoutedCommand? command)
    // {
    //     // if no command, just handle arrow keys
    //     if (command == null) return HandleArrowKeys(args);
    //     // handle commands
    //     return true;
    // }
    //
    // private bool HandleArrowKeys(KeyEventArgs args)
    // {
    //     var caretPosition = CaretPosition;
    //     if (!TryGetLine(caretPosition, out var document, out var line, out var lineText)) return false;
    //
    //     var isActionLine = IsActionLine(lineText);
    //     var startOfLine = caretPosition.Column == 1 || isActionLine && lineText.ElementAtOrDefault(caretPosition.Column - 2) == ' ';
    //     var endOfLine = caretPosition.Column == lineText.Length + 1 || isActionLine && caretPosition.Column == 5;
    //     var lineLength = lineText.Length;
    //
    //     // handle enter
    //     if (args.Key is Key.Enter or Key.Return)
    //     {
    //         if (isActionLine && startOfLine)
    //         {
    //             document.Insert(line.Offset, "   0\n");
    //             caretPosition.Column = 5;
    //         }
    //         else if (isActionLine)
    //         {
    //             document.Insert(line.EndOffset, "\n   0");
    //             caretPosition.Line++;
    //             caretPosition.Column = 5;
    //         }
    //         else
    //         {
    //             document.Insert(line.Offset + caretPosition.Column - 1, "\n");
    //             caretPosition.Line++;
    //             caretPosition.Column = 1;
    //         }
    //     }
    //     // handle arrow keys
    //     else if (args.Key is Key.Up or Key.Down or Key.Left or Key.Right)
    //     {
    //         if ((startOfLine && args.Key == Key.Left || args.Key == Key.Up) && caretPosition.Line > 1)
    //             caretPosition.Line--;
    //         else if ((endOfLine && args.Key == Key.Right || args.Key == Key.Down) && caretPosition.Line < document.LineCount)
    //             caretPosition.Line++;
    //         else if (!startOfLine && args.Key == Key.Left)
    //             caretPosition.Column--;
    //         else if (!endOfLine && args.Key == Key.Right)
    //             caretPosition.Column++;
    //     }
    //     // handle backspace
    //     else if (args.Key == Key.Back)
    //     {
    //         if (isActionLine && startOfLine)
    //         {
    //
    //         }
    //     }
    //     else
    //     {
    //         return false;
    //     }
    //
    //     // update current line
    //     TryGetLine(caretPosition, out _, out line, out lineText);
    //
    //     if (caretPosition.Line != CaretPosition.Line)
    //     {
    //         var leadingSpaces = lineText.Length - lineText.TrimStart().Length;
    //         if (args.Key == Key.Left)
    //             caretPosition.Column = IsActionLine(lineText) ? 5 : line.Length + 1;
    //         else if (args.Key == Key.Right)
    //             caretPosition.Column = IsActionLine(lineText) ? leadingSpaces + 1 : 1;
    //         else
    //             caretPosition.Column = IsActionLine(lineText) ? Math.Clamp(caretPosition.Column, leadingSpaces + 1, 5) : Math.Clamp(caretPosition.Column, 1, line.Length + 1);
    //     }
    //
    //     args.Handled = true;
    //     caretPosition.IsAtEndOfLine = caretPosition.Column == line.Length + 1;
    //     CaretPosition = caretPosition;
    //
    //     return true;
    // }
    //
    // private static bool IsActionLine(string line)
    // {
    //     var c = line.TrimStart().FirstOrDefault();
    //     return c >= '0' && c <= '9';
    // }
    //
    // private bool IsActionLine(int lineNumber)
    // {
    //     if (Document is not { } document) return false;
    //     if (document.GetLineByNumber(lineNumber) is not { } line) return false;
    //     if (document.GetText(line) is not { } lineText) return false;
    //     var c = lineText.TrimStart().FirstOrDefault();
    //     return c >= '0' && c <= '9';
    // }
    //
    // private bool TryGetLine(TextViewPosition caretPosition, out IDocument document, out IDocumentLine line, out string lineText)
    // {
    //     document = Document;
    //     line = null;
    //     lineText = null;
    //     if (document == null) return false;
    //
    //     line = document.GetLineByNumber(caretPosition.Line);
    //     if (line == null) return false;
    //
    //     lineText = document.GetText(line);
    //     return lineText != null;
    // }
}
