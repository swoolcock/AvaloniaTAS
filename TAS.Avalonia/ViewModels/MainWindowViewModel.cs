using System;
using System.IO;
using System.Reactive;
using System.Runtime.InteropServices;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ReactiveUI;
using TAS.Core;
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
        set => this.RaiseAndSetIfChanged(ref _caretPosition, value);
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
}
