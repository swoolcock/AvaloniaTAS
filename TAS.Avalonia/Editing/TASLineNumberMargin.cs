using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using System.Globalization;

namespace TAS.Avalonia.Editing;

public class TASLineNumberMargin : LineNumberMargin {
    public TASLineNumberMargin() {
        (Application.Current as App).CelesteService.Server.StateUpdated += _ => {
            Dispatcher.UIThread.InvokeAsync(() => InvalidateVisual());
        };
    }

    private const double SaveStateMinWidth = 5.0;

    public override void Render(DrawingContext drawingContext) {
        if (TextView is not { VisualLinesValid: true }) return;

        var pixelSize = PixelSnapHelpers.GetPixelSize(TextView);

        int playingLine = (Application.Current as App).CelesteService.CurrentLine;
        int saveStateLine = (Application.Current as App).CelesteService.SaveStateLine;

        var saveStateBgBrush = new ImmutableSolidColorBrush(Themeing.SaveStateBgColor);
        var saveStateFgBrush = new ImmutableSolidColorBrush(Themeing.SaveStateFgColor);
        var playingBgBrush = new ImmutableSolidColorBrush(Themeing.PlayingLineBgColor);
        var playingFgBrush = new ImmutableSolidColorBrush(Themeing.PlayingLineFgColor);
        var defaultFgBrush = GetValue(TextBlock.ForegroundProperty);

        foreach (var line in TextView.VisualLines) {
            int lineNumber = line.FirstDocumentLine.LineNumber;
            var text = new FormattedText(
                lineNumber.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                Typeface, EmSize, defaultFgBrush
            );
            double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);

            if (lineNumber == saveStateLine + 1 && lineNumber == playingLine + 1) {
                var saveStateRect = new Rect(0, TextView.DefaultLineHeight * saveStateLine - TextView.ScrollOffset.Y, Bounds.Width, TextView.DefaultLineHeight);
                var playingRect = new Rect(SaveStateMinWidth, TextView.DefaultLineHeight * playingLine - TextView.ScrollOffset.Y, Bounds.Width - SaveStateMinWidth, TextView.DefaultLineHeight);

                drawingContext.FillRectangle(saveStateBgBrush, PixelSnapHelpers.PixelAlign(saveStateRect, pixelSize));
                drawingContext.FillRectangle(playingBgBrush, PixelSnapHelpers.PixelAlign(playingRect, pixelSize));
                text.SetForegroundBrush(playingFgBrush);
            } else if (lineNumber == saveStateLine + 1) {
                var rect = new Rect(0, TextView.DefaultLineHeight * saveStateLine - TextView.ScrollOffset.Y, Bounds.Width, TextView.DefaultLineHeight);

                drawingContext.FillRectangle(saveStateBgBrush, PixelSnapHelpers.PixelAlign(rect, pixelSize));
                text.SetForegroundBrush(saveStateFgBrush);
            } else if (lineNumber == playingLine + 1) {
                var rect = new Rect(0, TextView.DefaultLineHeight * playingLine - TextView.ScrollOffset.Y, Bounds.Width, TextView.DefaultLineHeight);

                drawingContext.FillRectangle(playingBgBrush, PixelSnapHelpers.PixelAlign(rect, pixelSize));
                text.SetForegroundBrush(playingFgBrush);
            }

            drawingContext.DrawText(text, new Point(Bounds.Width - text.Width, y - TextView.VerticalOffset));
        }
    }
}
