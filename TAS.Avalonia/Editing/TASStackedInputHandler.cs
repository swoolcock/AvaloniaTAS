using Avalonia;
using Avalonia.Input;
using AvaloniaEdit.Editing;

namespace TAS.Avalonia.Editing;

public class TASStackedInputHandler : TextAreaStackedInputHandler {

    public TASStackedInputHandler(TextArea textArea) : base(textArea)
    { }

    public override void OnPreviewKeyDown(KeyEventArgs e) {
        (Application.Current as App).CelesteService.SendKeyPress(e.Key, e.KeyModifiers);
    }
}
