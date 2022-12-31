using System;
using System.IO;
using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ReactiveUI;
using TAS.Avalonia.Models;
using TAS.Avalonia.Services;

namespace TAS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleHitboxesCommand { get; }

    private TASDocument? _document;
    public TASDocument? Document
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
        _celesteService = AvaloniaLocator.Current.GetService<ICelesteService>()!;
        NewFileCommand = ReactiveCommand.Create(NewFile);
        ToggleHitboxesCommand = ReactiveCommand.Create(ToggleHitboxes);
        Document = TASDocument.Load("/Users/shane/Celeste/Celeste.tas") ?? TASDocument.CreateBlank();
    }

    private void ToggleHitboxes()
    {
        _celesteService.ToggleHitboxes();
    }

    private void NewFile()
    {
        Document = TASDocument.CreateBlank();
    }
}
