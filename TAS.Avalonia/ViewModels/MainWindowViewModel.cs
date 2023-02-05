using System.Reactive;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;
using AvaloniaEdit;
using ReactiveUI;
using TAS.Avalonia.Models;
using TAS.Avalonia.Services;

namespace TAS.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveFileCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveFileAsCommand { get; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleCommentsCommand { get; }

    public ReactiveCommand<Unit, Unit> ToggleHitboxesCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowTriggerHitboxesCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowUnloadedRoomsHitboxesCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowCameraHitboxesCommand { get; }
    public ReactiveCommand<Unit, Unit> SimplifiedHitboxesCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowActualCollideHitboxesCommand { get; }

    public ReactiveCommand<Unit, Unit> SimplifiedGraphicsCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowGameplayCommand { get; }

    public ReactiveCommand<Unit, Unit> CenterCameraCommand { get; }

    public ReactiveCommand<Unit, Unit> InfoHudCommand { get; }
    public ReactiveCommand<Unit, Unit> InfoTasInputCommand { get; }
    public ReactiveCommand<Unit, Unit> InfoGameCommand { get; }
    public ReactiveCommand<Unit, Unit> InfoWatchEntityCommand { get; }
    public ReactiveCommand<Unit, Unit> InfoCustomCommand { get; }
    public ReactiveCommand<Unit, Unit> InfoSubpixelIndicatorCommand { get; }

    private TASDocument _document;
    public TASDocument Document {
        get => _document;
        set => this.RaiseAndSetIfChanged(ref _document, value);
    }

    private TextViewPosition _caretPosition;
    public TextViewPosition CaretPosition {
        get => _caretPosition;
        set => this.RaiseAndSetIfChanged(ref _caretPosition, value);
    }

    public bool MenuVisible => true; //!RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    private readonly ICelesteService _celesteService;

    private MenuModel[] MainMenu { get; }
    private MenuModel[] EditorContextMenu { get; }

    public MainWindowViewModel() {
        _celesteService = AvaloniaLocator.Current.GetService<ICelesteService>()!;

        // File
        NewFileCommand = ReactiveCommand.Create(NewFile);
        OpenFileCommand = ReactiveCommand.Create(OpenFile);
        SaveFileCommand = ReactiveCommand.Create(SaveFile);
        SaveFileAsCommand = ReactiveCommand.Create(SaveFileAs);
        ExitCommand = ReactiveCommand.Create(Exit);

        // Toggles
        ToggleHitboxesCommand = ReactiveCommand.Create(ToggleHitboxes);
        ShowTriggerHitboxesCommand = ReactiveCommand.Create(ShowTriggerHitboxes);
        ShowUnloadedRoomsHitboxesCommand = ReactiveCommand.Create(ShowUnloadedRoomsHitboxes);
        ShowCameraHitboxesCommand = ReactiveCommand.Create(ShowCameraHitboxes);
        SimplifiedHitboxesCommand = ReactiveCommand.Create(SimplifiedHitboxes);
        ShowActualCollideHitboxesCommand = ReactiveCommand.Create(ShowActualCollideHitboxes);

        SimplifiedGraphicsCommand = ReactiveCommand.Create(SimplifiedGraphics);
        ShowGameplayCommand = ReactiveCommand.Create(ShowGameplay);

        CenterCameraCommand = ReactiveCommand.Create(CenterCamera);

        InfoHudCommand = ReactiveCommand.Create(InfoHud);
        InfoTasInputCommand = ReactiveCommand.Create(InfoTasInput);
        InfoGameCommand = ReactiveCommand.Create(InfoGame);
        InfoWatchEntityCommand = ReactiveCommand.Create(InfoWatchEntity);
        InfoCustomCommand = ReactiveCommand.Create(InfoCustom);
        InfoSubpixelIndicatorCommand = ReactiveCommand.Create(InfoSubpixelIndicator);

        // Context
        ToggleCommentsCommand = ReactiveCommand.Create(ToggleComments);

        Document = TASDocument.Load("/Users/shane/Celeste/Celeste.tas") ?? TASDocument.CreateBlank();
        MainMenu = CreateMenu(MenuVisible);
        EditorContextMenu = CreateContextMenu();
    }

    private MenuModel[] CreateMenu(bool includeExit) => new[] {
        new MenuModel("_File", isEnabled: true) {
            new MenuModel("New File", NewFileCommand, gesture: new KeyGesture(Key.N, KeyModifiers.Meta)),
            MenuModel.Separator,
            new MenuModel("Open File...", OpenFileCommand, gesture: new KeyGesture(Key.O, KeyModifiers.Meta)),
            new MenuModel("Open Previous File"),
            new MenuModel("Open Recent") {
                new MenuModel("Celeste.tas"),
            },
            new MenuModel("Open Backup") {
                new MenuModel("Celeste.tas"),
            },
            MenuModel.Separator,
            new MenuModel("Save", SaveFileCommand, gesture: new KeyGesture(Key.S, KeyModifiers.Meta)),
            new MenuModel("Save As...", SaveFileAsCommand, gesture: new KeyGesture(Key.S, KeyModifiers.Meta | KeyModifiers.Shift)),
            new MenuModel("Convert to LibTAS Inputs..."),
            new MenuModel(string.Empty, isVisible: includeExit),
            new MenuModel("Exit", ExitCommand, isVisible: includeExit),
        },
        new MenuModel("Settings") {
            new MenuModel("Send Inputs to Celeste"),
            new MenuModel("Auto Remove Mutually Exclusive Actions"),
            new MenuModel("Show Game Info"),
            new MenuModel("Automatic Backup") {
                new MenuModel("Enabled"),
                new MenuModel("Backup Rate (minutes): 1"),
                new MenuModel("Backup File Count: 100"),
            },
            new MenuModel("Font..."),
            new MenuModel("Themes") {
                new MenuModel("Light"),
                new MenuModel("Dark"),
                new MenuModel("Custom"),
            },
        },
        new MenuModel("Toggles") {
            new MenuModel("Hitboxes", command: ToggleHitboxesCommand),
            new MenuModel("Trigger Hitboxes", command: ShowTriggerHitboxesCommand),
            new MenuModel("Unloaded Rooms Hitboxes", command: ShowUnloadedRoomsHitboxesCommand),
            new MenuModel("Camera Hitboxes", command: ShowCameraHitboxesCommand),
            new MenuModel("Simplified Hitboxes", command: SimplifiedHitboxesCommand),
            new MenuModel("Actual Collide Hitboxes", command: ShowActualCollideHitboxesCommand),
            MenuModel.Separator,
            new MenuModel("Simplified Graphics", command: SimplifiedGraphicsCommand),
            new MenuModel("Gameplay", command: ShowGameplayCommand),
            MenuModel.Separator,
            new MenuModel("Center Camera", command: CenterCameraCommand),
            MenuModel.Separator,
            new MenuModel("Info HUD", command: InfoHudCommand),
            new MenuModel("TAS Input Info", command: InfoTasInputCommand),
            new MenuModel("Game Info", command: InfoGameCommand),
            new MenuModel("Watch Entity Info", command: InfoWatchEntityCommand),
            new MenuModel("Custom Info", command: InfoCustomCommand),
            new MenuModel("Subpixel Indicator", command: InfoSubpixelIndicatorCommand),
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

    private MenuModel[] CreateContextMenu() => new[] {
        new MenuModel("Cut"),
        new MenuModel("Copy"),
        new MenuModel("Paste"),
        MenuModel.Separator,
        new MenuModel("Undo"),
        new MenuModel("Redo"),
        MenuModel.Separator,
        new MenuModel("Insert/Remove Breakpoint"),
        new MenuModel("Insert/Remove Savestate Breakpoint"),
        new MenuModel("Remove All Uncommented Breakpoints"),
        new MenuModel("Remove All Breakpoints"),
        new MenuModel("Comment/Uncomment All Breakpoints"),
        MenuModel.Separator,
        new MenuModel("Comment/Uncomment Text", command: ToggleCommentsCommand),
        new MenuModel("Insert Room Name"),
        new MenuModel("Insert Current In-Game Time"),
        new MenuModel("Insert Mod Info"),
        new MenuModel("Insert Console Load Command"),
        new MenuModel("Insert Simple Console Load Command"),
        new MenuModel("Insert Other Command") {
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

    private void ToggleHitboxes() => _celesteService.ToggleHitboxes();
    private void ShowTriggerHitboxes() => _celesteService.ShowTriggerHitboxes();
    private void ShowUnloadedRoomsHitboxes() => _celesteService.ShowUnloadedRoomsHitboxes();
    private void ShowCameraHitboxes() => _celesteService.ShowCameraHitboxes();
    private void SimplifiedHitboxes() => _celesteService.SimplifiedHitboxes();
    private void ShowActualCollideHitboxes() => _celesteService.ShowActualCollideHitboxes();
    private void SimplifiedGraphics() => _celesteService.SimplifiedGraphics();
    private void ShowGameplay() => _celesteService.ShowGameplay();
    private void CenterCamera() => _celesteService.CenterCamera();
    private void InfoHud() => _celesteService.InfoHud();
    private void InfoTasInput() => _celesteService.InfoTasInput();
    private void InfoGame() => _celesteService.InfoGame();
    private void InfoWatchEntity() => _celesteService.InfoWatchEntity();
    private void InfoCustom() => _celesteService.InfoCustom();
    private void InfoSubpixelIndicator() => _celesteService.InfoSubpixelIndicator();

    private async Task<bool> ConfirmDiscardChangesAsync() {
        if (!Document.Dirty) return true;
        var dialogService = AvaloniaLocator.Current.GetService<IDialogService>()!;
        bool result = await dialogService.ShowConfirmDialogAsync("You have unsaved changes. Are you sure?");
        if (result) await Task.Delay(TimeSpan.FromSeconds(0.1f));
        return result;
    }

    private async void NewFile() {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        if (!await ConfirmDiscardChangesAsync()) return;
        Document = TASDocument.CreateBlank();
    }

    private async void OpenFile() {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        if (!await ConfirmDiscardChangesAsync()) return;
        var dialogService = AvaloniaLocator.Current.GetService<IDialogService>()!;
        string[] results = await dialogService.ShowOpenFileDialogAsync("Celeste TAS", "tas");

        if (results?.FirstOrDefault() is not { } filepath) return;

        if (TASDocument.Load(filepath) is not { } doc) {
            await dialogService.ShowDialogAsync($"Error loading file: {filepath}");
            return;
        }

        Document = doc;
    }

    private async void SaveFile() {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        _celesteService.WriteWait();

        string filename = await SaveFileAsAsync(false);

        if (filename != null) _celesteService.SendPath(filename);
    }

    private async void SaveFileAs() {
        // delay to allow the UI to recover
        await Task.Delay(TimeSpan.FromSeconds(0.1f));

        _celesteService.WriteWait();

        string filename = await SaveFileAsAsync(true);

        if (filename != null)
            _celesteService.SendPath(filename);
    }

    private async Task<string> SaveFileAsAsync(bool force) {
        string filename = Document.Filename;
        if (force || filename == null) {
            var dialogService = AvaloniaLocator.Current.GetService<IDialogService>()!;
            filename = await dialogService.ShowSaveFileDialogAsync("Celeste TAS", "tas");
            if (filename != null && File.Exists(filename) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                // we don't need to confirm on macOS since the finder file dialog does it for us
                bool confirm = await dialogService.ShowConfirmDialogAsync("This file already exists. Are you sure you want to overwrite it?", "Celeste TAS");
                if (!confirm) return null;
            }
        }

        if (filename == null) return null;

        Document.Save(filename);

        return filename;
    }

    private void Exit() => Application.Current?.DesktopLifetime().Shutdown();

    private void ToggleComments() {
    }
}
