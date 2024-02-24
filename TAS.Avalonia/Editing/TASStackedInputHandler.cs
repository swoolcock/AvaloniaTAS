using Avalonia;
using Avalonia.Input;
using AvaloniaEdit.Editing;

namespace TAS.Avalonia.Editing;

public class TASStackedInputHandler : TextAreaStackedInputHandler {

    public TASStackedInputHandler(TextArea textArea) : base(textArea)
    { }

    public override void OnPreviewKeyDown(KeyEventArgs e) {
        var app = (Application.Current as App)!;
        if (app.SettingsService.SendInputs)
            e.Handled = app.CelesteService.SendKeyEvent(e.Key, e.KeyModifiers, released: false);
    }

    public override void OnPreviewKeyUp(KeyEventArgs e) {
        var app = (Application.Current as App)!;
        if (app.SettingsService.SendInputs)
            e.Handled = app.CelesteService.SendKeyEvent(e.Key, e.KeyModifiers, released: true);
    }
}
