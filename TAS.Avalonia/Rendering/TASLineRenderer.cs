using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;

namespace TAS.Avalonia.Rendering;

internal class TASLineRenderer : IBackgroundRenderer {
    private readonly TextArea _textArea;

    public KnownLayer Layer => KnownLayer.Background;

    public TASLineRenderer(TextArea textArea) {
        _textArea = textArea;
    }

    public void Draw(TextView textView, DrawingContext drawingContext) {
        var brush = new ImmutableSolidColorBrush(Themeing.CurrentLineColor);

        int line = _textArea.Caret.Line - 1;

        var pixelSize = PixelSnapHelpers.GetPixelSize(textView);
        var rect = new Rect(0, textView.DefaultLineHeight * line - textView.ScrollOffset.Y, textView.Bounds.Width, textView.DefaultLineHeight);

        drawingContext.FillRectangle(brush, PixelSnapHelpers.PixelAlign(rect, pixelSize));
    }
}
