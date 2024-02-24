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
            app.CelesteService.SendKeyPress(e.Key, e.KeyModifiers);
    }
}
