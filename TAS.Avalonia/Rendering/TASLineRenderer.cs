using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;

namespace TAS.Avalonia.Rendering;

internal class TASLineRenderer : IBackgroundRenderer {
    private readonly TextArea _textArea;
    private readonly IBrush _brush;

    public KnownLayer Layer => KnownLayer.Background;

    public static readonly Color CurrentLineColor = Color.FromArgb(32, 180, 182, 199);

    public TASLineRenderer(TextArea textArea) {
        _textArea = textArea;
        _brush = new ImmutableSolidColorBrush(CurrentLineColor);
    }

    public void Draw(TextView textView, DrawingContext drawingContext) {
        int line = _textArea.Caret.Line - 1;

        var pixelSize = PixelSnapHelpers.GetPixelSize(textView);
        var rect = new Rect(0, textView.DefaultLineHeight * line - textView.ScrollOffset.Y, textView.Bounds.Width, textView.DefaultLineHeight);

        drawingContext.FillRectangle(_brush, PixelSnapHelpers.PixelAlign(rect, pixelSize));
    }
}
