using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using ReactiveUI;
using TAS.Avalonia.Models;
using TAS.Avalonia.Services;

namespace TAS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveFileAsCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleHitboxesCommand { get; }

    private TASDocument _document;
    public TASDocument Document
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

    public bool MenuVisible => true;//!RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private readonly ICelesteService _celesteService;

    private MenuModel[] MainMenu { get; }
    private MenuModel[] EditorContextMenu { get; }

    public MainWindowViewModel()
    {
        _celesteService = AvaloniaLocator.Current.GetService<ICelesteService>()!;

        // File
        NewFileCommand = ReactiveCommand.Create(NewFile);
        OpenFileCommand = ReactiveCommand.Create(OpenFile);
        SaveFileAsCommand = ReactiveCommand.Create(SaveFileAs);
        // Toggles
        ToggleHitboxesCommand = ReactiveCommand.Create(ToggleHitboxes);

        Document = TASDocument.Load("/Users/shane/Celeste/Celeste.tas") ?? TASDocument.CreateBlank();
        MainMenu = CreateMenu(MenuVisible);
        EditorContextMenu = CreateContextMenu();
    }

    private MenuModel[] CreateMenu(bool includeExit) => new[]
    {
        new MenuModel("_File", isEnabled: true)
        {
            new MenuModel("New File", NewFileCommand, gesture: new KeyGesture(Key.N, KeyModifiers.Meta)),
            MenuModel.Separator,
            new MenuModel("Open File...", OpenFileCommand, gesture: new KeyGesture(Key.O, KeyModifiers.Meta)),
            new MenuModel("Open Previous File"),
            new MenuModel("Open Recent")
            {
                new MenuModel("Celeste.tas"),
            },
            new MenuModel("Open Backup")
            {
                new MenuModel("Celeste.tas"),
            },
            MenuModel.Separator,
            new MenuModel("Save As...", SaveFileAsCommand, gesture: new KeyGesture(Key.S, KeyModifiers.Meta)),
            new MenuModel("Convert to LibTAS Inputs..."),
            new MenuModel(string.Empty, isVisible: includeExit),
            new MenuModel("Exit", isVisible: includeExit),
        },
        new MenuModel("Settings")
        {
            new MenuModel("Send Inputs to Celeste"),
            new MenuModel("Auto Remove Mutually Exclusive Actions"),
            new MenuModel("Show Game Info"),
            new MenuModel("Automatic Backup")
            {
                new MenuModel("Enabled"),
                new MenuModel("Backup Rate (minutes): 1"),
                new MenuModel("Backup File Count: 100"),
            },
            new MenuModel("Font..."),
            new MenuModel("Themes")
            {
                new MenuModel("Light"),
                new MenuModel("Dark"),
                new MenuModel("Custom"),
            },
        },
        new MenuModel("Toggles")
        {
            new MenuModel("Hitboxes", command: ToggleHitboxesCommand),
            new MenuModel("Trigger Hitboxes"),
            new MenuModel("Unloaded Rooms Hitboxes"),
            new MenuModel("Camera Hitboxes"),
            new MenuModel("Simplified Hitboxes"),
            new MenuModel("Actual Collide Hitboxes"),
            MenuModel.Separator,
            new MenuModel("Simplified Graphics"),
            new MenuModel("Gameplay"),
            MenuModel.Separator,
            new MenuModel("Center Camera"),
            MenuModel.Separator,
            new MenuModel("Info HUD"),
            new MenuModel("TAS Input Info"),
            new MenuModel("Game Info"),
            new MenuModel("Watch Entity Info"),
            new MenuModel("Custom Info"),
            new MenuModel("Subpixel Indicator"),
            MenuModel.Separator,
            new MenuModel("Position Decimals"),
            new MenuModel("Speed Decimals"),
            new MenuModel("Velocity Decimals"),
            new MenuModel("Custom Info Decimals"),
            new MenuModel("Subpixel Indicator Decimals"),
            new MenuModel("Unit of Speed"),
            MenuModel.Separator,
            new MenuModel("Fast Forward Speed"),
            new MenuModel("Slow Forward Speed"),
        },
    };

    private MenuModel[] CreateContextMenu() => new[]
    {
        new MenuModel("Insert/Remove Breakpoint"),
        new MenuModel("Insert/Remove Savestate Breakpoint"),
        new MenuModel("Remove All Uncommented Breakpoints"),
        new MenuModel("Remove All Breakpoints"),
        new MenuModel("Comment/Uncomment All Breakpoints"),
        MenuModel.Separator,
        new MenuModel("Comment/Uncomment Text"),
        new MenuModel("Insert Room Name"),
        new MenuModel("Insert Current In-Game Time"),
        new MenuModel("Insert Mod Info"),
        new MenuModel("Insert Console Load Command"),
        new MenuModel("Insert Simple Console Load Command"),
        new MenuModel("Insert Other Command")
        {
            new MenuModel("EnforceLegal"),
            new MenuModel("Unsafe"),
            new MenuModel("Safe"),
            MenuModel.Separator,
            new MenuModel("Read"),
            new MenuModel("Play"),
            MenuModel.Separator,
            new MenuModel("Repeat"),
            new MenuModel("EndRepeat"),
            MenuModel.Separator,
            new MenuModel("Set"),
            MenuModel.Separator,
            new MenuModel("AnalogMode"),
            MenuModel.Separator,
            new MenuModel("StartExportGameInfo"),
            new MenuModel("FinishExportGameInfo"),
            MenuModel.Separator,
            new MenuModel("StartExportRoomInfo"),
            new MenuModel("FinishExportRoomInfo"),
            MenuModel.Separator,
            new MenuModel("Add"),
            new MenuModel("Skip"),
            new MenuModel("StartExportLibTAS"),
            new MenuModel("FinishExportLibTAS"),
            MenuModel.Separator,
            new MenuModel("CompleteInfo"),
            new MenuModel("Record Count"),
            new MenuModel("File Time"),
            new MenuModel("Chapter Time"),
            MenuModel.Separator,
            new MenuModel("Exit Game"),
        },
        MenuModel.Separator,
        new MenuModel("Swap Selected C and X"),
        new MenuModel("Swap Selected J and K"),
        new MenuModel("Combine Consecutive Same Inputs"),
        new MenuModel("Force Combine Inputs Frames"),
        new MenuModel("Convert Dash to Demo Dash"),
        MenuModel.Separator,
        new MenuModel("Open Read File / Go to Play Line"),
    };

    private void ToggleHitboxes()
    {
        _celesteService.ToggleHitboxes();
    }

    private async Task<bool> ConfirmDiscardChangesAsync()
    {
        if (!Document.Dirty) return true;
        var dialogService = AvaloniaLocator.Current.GetService<IDialogService>()!;
        return await dialogService.ShowConfirmDialogAsync("You have unsaved changes. Are you sure?");
    }

    private async void NewFile()
    {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        if (!await ConfirmDiscardChangesAsync()) return;
        Document = TASDocument.CreateBlank();
    }

    private async void OpenFile()
    {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        if (!await ConfirmDiscardChangesAsync()) return;
        var dialogService = AvaloniaLocator.Current.GetService<IDialogService>()!;
        var results = await dialogService.ShowOpenFileDialogAsync("Celeste TAS", "tas");
    }

    private async void SaveFileAs()
    {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        var dialogService = AvaloniaLocator.Current.GetService<IDialogService>()!;
        var results = await dialogService.ShowSaveFileDialogAsync("Celeste TAS", "tas");
    }
}
