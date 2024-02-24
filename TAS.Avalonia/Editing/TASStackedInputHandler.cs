using Avalonia;
using Avalonia.Input;
using AvaloniaEdit.Editing;
using System.Diagnostics;

namespace TAS.Avalonia.Editing;

public class TASStackedInputHandler : TextAreaStackedInputHandler {

    public TASStackedInputHandler(TextArea textArea) : base(textArea)
    { }

    // Pretty ugly solution to avoid typing characters when trying to use a hotkey.
    // Just cancelling the KeyDown event does not work. (see https://github.com/AvaloniaUI/Avalonia/issues/14108)
    // There is probably some better solution, however this works since the KeyDown event is fired before the TextInput event.
    // We apply a timeout, since some KeyDown events might not cause a TextInput event to reset it.
    internal static readonly TimeSpan CancellationTime = TimeSpan.FromMilliseconds(50);
    internal static readonly Stopwatch CancelNextTextInputEvent = new();

    public override void OnPreviewKeyDown(KeyEventArgs e) {
        var app = (Application.Current as App)!;
        if (app.SettingsService.SendInputs) {
            e.Handled = app.CelesteService.SendKeyEvent(e.Key, e.KeyModifiers, released: false);
            CancelNextTextInputEvent.Restart();
        }
    }

    public override void OnPreviewKeyUp(KeyEventArgs e) {
        var app = (Application.Current as App)!;
        if (app.SettingsService.SendInputs) {
            e.Handled = app.CelesteService.SendKeyEvent(e.Key, e.KeyModifiers, released: true);
            CancelNextTextInputEvent.Restart();
        }
    }
}
