using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;

namespace TAS.Avalonia.Editing;

public class TASLineNumberMargin : LineNumberMargin {
    private readonly IBrush _playingLineBrush;

    public static readonly Color PlayingLineColor = Color.FromArgb(255, 241, 250, 140);

    public TASLineNumberMargin() {
        (Application.Current as App).CelesteService.Server.StateUpdated += _ => {
            Dispatcher.UIThread.Invoke(() => InvalidateVisual());
        };
        _playingLineBrush = new ImmutableSolidColorBrush(PlayingLineColor);
    }

    public override void Render(DrawingContext drawingContext) {
        int currentLine = (Application.Current as App).CelesteService.CurrentLine;
        if (currentLine >= 0) {
            var pixelSize = PixelSnapHelpers.GetPixelSize(TextView);
            var rect = new Rect(0, TextView.DefaultLineHeight * currentLine - TextView.ScrollOffset.Y, Bounds.Width, TextView.DefaultLineHeight);

            drawingContext.FillRectangle(_playingLineBrush, PixelSnapHelpers.PixelAlign(rect, pixelSize));
        }

        base.Render(drawingContext);
    }
}
