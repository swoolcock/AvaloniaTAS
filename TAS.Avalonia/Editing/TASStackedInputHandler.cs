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

    // This is also horrible because i could not fine a simple way of just checking if a key is down.
    // Only supports modifier keys, since everything else is even messier.
    // Using KeyModifiers loses the context if the left or right key was pressed.
    // That context is needed to properly support input forwarding.
    private readonly HashSet<Key> pressedModKeys = new();

    public override void OnPreviewKeyDown(KeyEventArgs e) {
        var app = (Application.Current as App)!;

        // If the key was released outside of the window, a KeyUp event wouldn't be raised.
        // So we need to check if the pressed keys
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
            pressedModKeys.Remove(Key.LeftShift);
            pressedModKeys.Remove(Key.RightShift);
        }
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
            pressedModKeys.Remove(Key.LeftCtrl);
            pressedModKeys.Remove(Key.RightCtrl);
        }
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Alt)) {
            pressedModKeys.Remove(Key.LeftAlt);
            pressedModKeys.Remove(Key.RightAlt);
        }

        if (e.Key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt)
            pressedModKeys.Add(e.Key);

        if (app.SettingsService.SendInputs) {
            e.Handled = app.CelesteService.SendKeyEvent(pressedModKeys.Concat([e.Key]), released: false);
            if (e.Handled)
                CancelNextTextInputEvent.Restart();
        }
    }

    public override void OnPreviewKeyUp(KeyEventArgs e) {
        var app = (Application.Current as App)!;
        if (app.SettingsService.SendInputs) {
            e.Handled = app.CelesteService.SendKeyEvent(pressedModKeys.Concat([e.Key]), released: true);
            if (e.Handled)
                CancelNextTextInputEvent.Restart();
        }

        if (e.Key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt)
            pressedModKeys.Add(e.Key);
    }
}
