using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using System.Globalization;

namespace TAS.Avalonia.Rendering;

internal class TASLineRenderer : IBackgroundRenderer {
    private readonly TextArea _textArea;

    public KnownLayer Layer => KnownLayer.Background;

    private const double LineSuffixRightOffset = 5.0;

    public TASLineRenderer(TextArea textArea) {
        _textArea = textArea;
        (Application.Current as App).CelesteService.Server.StateUpdated += _ => {
            Dispatcher.UIThread.Invoke(() => _textArea.TextView.InvalidateVisual());
        };
    }

    public void Draw(TextView textView, DrawingContext drawingContext) {
        var currentLineBrush = new ImmutableSolidColorBrush(Themeing.CurrentLineColor);
        var playingFramerush = new ImmutableSolidColorBrush(Themeing.PlayingFrameColor);

        int currentLine = _textArea.Caret.Line - 1;
        int playingLine = (Application.Current as App).CelesteService.CurrentLine;

        var pixelSize = PixelSnapHelpers.GetPixelSize(textView);
        double currentLineY = textView.DefaultLineHeight * currentLine - textView.ScrollOffset.Y;
        var currentLineRect = new Rect(0, currentLineY, textView.Bounds.Width, textView.DefaultLineHeight);
        drawingContext.FillRectangle(currentLineBrush, PixelSnapHelpers.PixelAlign(currentLineRect, pixelSize));

        string suffix = (Application.Current as App).CelesteService.CurrentLineSuffix;
        double playingLineX = textView.Bounds.Width - textView.WideSpaceWidth * suffix.Length - LineSuffixRightOffset;
        double playingLineY = textView.DefaultLineHeight * playingLine - textView.ScrollOffset.Y;
        var typeface = textView.CreateTypeface();
        double emSize = TextElement.GetFontSize(textView);
        var text = new FormattedText(suffix, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                     typeface, emSize, playingFramerush);
        drawingContext.DrawText(text, new Point(playingLineX, playingLineY));
    }
}
