using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Text;
using AvaloniaEdit.Utils;

namespace TAS.Avalonia.Editing;

#nullable disable

internal sealed class TASSelectionMouseHandler : ITextAreaInputHandler
{
    private SelectionMode _mode;
    private AnchorSegment _startWord;
    private Point _possibleDragStartMousePos;
    private bool _enableTextDragDrop;
    private const int MinimumHorizontalDragDistance = 2;
    private const int MinimumVerticalDragDistance = 2;

    public TASSelectionMouseHandler(TextArea textArea)
    {
        TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));
    }

    public TextArea TextArea { get; }

    public void Attach()
    {
        TextArea.PointerPressed += TextArea_MouseLeftButtonDown;
        TextArea.PointerMoved += TextArea_MouseMove;
        TextArea.PointerReleased += TextArea_MouseLeftButtonUp;
        TextArea.OptionChanged += TextArea_OptionChanged;
        _enableTextDragDrop = TextArea.Options.EnableTextDragDrop;
        if (!_enableTextDragDrop) return;
        AttachDragDrop();
    }

    public void Detach()
    {
        _mode = SelectionMode.None;
        TextArea.PointerPressed -= TextArea_MouseLeftButtonDown;
        TextArea.PointerMoved -= TextArea_MouseMove;
        TextArea.PointerReleased -= TextArea_MouseLeftButtonUp;
        TextArea.OptionChanged -= TextArea_OptionChanged;
        if (!_enableTextDragDrop) return;
        DetachDragDrop();
    }

    private void AttachDragDrop()
    {
    }

    private void DetachDragDrop()
    {
    }

    private void TextArea_OptionChanged(object sender, PropertyChangedEventArgs e)
    {
        bool enableTextDragDrop = TextArea.Options.EnableTextDragDrop;
        if (enableTextDragDrop == _enableTextDragDrop) return;
        _enableTextDragDrop = enableTextDragDrop;
        if (enableTextDragDrop)
            AttachDragDrop();
        else
            DetachDragDrop();
    }

    private void TextArea_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(TextArea).Properties.IsLeftButtonPressed)
        {
            if (!TextArea.RightClickMovesCaret || e.Handled) return;
            SetCaretOffsetToMousePosition(e);
            return;
        }

        TextArea.Cursor = Cursor.Parse("IBeam");
        PointerPoint currentPoint = e.GetCurrentPoint(TextArea);
        _mode = SelectionMode.None;

        if (e.Handled)
        {
            return;
        }

        KeyModifiers keyModifiers = e.KeyModifiers;
        bool flag = keyModifiers.HasFlag(KeyModifiers.Shift);
        if (_enableTextDragDrop && e.ClickCount == 1 && !flag && TextArea.Selection.Contains(GetOffsetFromMousePosition(e, out int _, out bool _)))
        {
            if (TextArea.CapturePointer(e.Pointer))
            {
                _mode = SelectionMode.PossibleDragStart;
                _possibleDragStartMousePos = e.GetPosition(TextArea);
            }

            e.Handled = true;
            return;
        }

        TextViewPosition position = TextArea.Caret.Position;
        SetCaretOffsetToMousePosition(e);
        if (!flag) TextArea.ClearSelection();
        if (!TextArea.CapturePointer(e.Pointer))
        {
            e.Handled = true;
            return;
        }

        if (keyModifiers.HasFlag(KeyModifiers.Alt) && TextArea.Options.EnableRectangularSelection)
        {
            _mode = SelectionMode.Rectangular;
            if (flag && TextArea.Selection is RectangleSelection) TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(position, TextArea.Caret.Position);
        }
        else if (keyModifiers.HasFlag(KeyModifiers.Control) && e.ClickCount == 1)
        {
            _mode = SelectionMode.WholeWord;
            if (flag && !(TextArea.Selection is RectangleSelection)) TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(position, TextArea.Caret.Position);
        }
        else if (currentPoint.Properties.IsLeftButtonPressed && e.ClickCount == 1)
        {
            _mode = SelectionMode.Normal;
            if (flag && !(TextArea.Selection is RectangleSelection)) TextArea.Selection = TextArea.Selection.StartSelectionOrSetEndpoint(position, TextArea.Caret.Position);
        }
        else
        {
            _mode = SelectionMode.WholeWord;
            GetWordAtMousePosition(e);
            SimpleSegment simpleSegment;
            if (e.ClickCount == 3)
            {
                _mode = SelectionMode.WholeLine;
                simpleSegment = GetLineAtMousePosition(e);
            }
            else
            {
                _mode = SelectionMode.WholeWord;
                simpleSegment = GetWordAtMousePosition(e);
            }

            if (simpleSegment == SimpleSegment.Invalid)
            {
                _mode = SelectionMode.None;
                TextArea.ReleasePointerCapture(e.Pointer);
                return;
            }

            if (flag && !TextArea.Selection.IsEmpty)
            {
                if (simpleSegment.Offset < TextArea.Selection.SurroundingSegment.Offset)
                    TextArea.Selection = TextArea.Selection.SetEndpoint(new TextViewPosition(TextArea.Document.GetLocation(simpleSegment.Offset)));
                else if (simpleSegment.EndOffset > TextArea.Selection.SurroundingSegment.EndOffset) TextArea.Selection = TextArea.Selection.SetEndpoint(new TextViewPosition(TextArea.Document.GetLocation(simpleSegment.EndOffset)));
                _startWord = new AnchorSegment(TextArea.Document, TextArea.Selection.SurroundingSegment);
            }
            else
            {
                TextArea.Selection = Selection.Create(TextArea, simpleSegment.Offset, simpleSegment.EndOffset);
                _startWord = new AnchorSegment(TextArea.Document, simpleSegment.Offset, simpleSegment.Length);
            }
        }

        e.Handled = true;
    }

    private SimpleSegment GetWordAtMousePosition(PointerEventArgs e)
    {
        TextView textView = TextArea.TextView;
        if (textView == null) return SimpleSegment.Invalid;
        Point point = e.GetPosition(textView);
        if (point.Y < 0.0) point = point.WithY(0.0);
        if (point.Y > textView.Bounds.Height) point = point.WithY(textView.Bounds.Height);
        point += textView.ScrollOffset;
        VisualLine lineFromVisualTop = textView.GetVisualLineFromVisualTop(point.Y);
        if (lineFromVisualTop == null) return SimpleSegment.Invalid;
        int visualColumn1 = lineFromVisualTop.GetVisualColumn(point, TextArea.Selection.EnableVirtualSpace);
        int visualColumn2 = lineFromVisualTop.GetNextCaretPosition(visualColumn1 + 1, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol, TextArea.Selection.EnableVirtualSpace);
        if (visualColumn2 == -1) visualColumn2 = 0;
        int visualColumn3 = lineFromVisualTop.GetNextCaretPosition(visualColumn2, LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol, TextArea.Selection.EnableVirtualSpace);
        if (visualColumn3 == -1) visualColumn3 = lineFromVisualTop.VisualLength;
        int offset1 = lineFromVisualTop.FirstDocumentLine.Offset;
        int offset2 = lineFromVisualTop.GetRelativeOffset(visualColumn2) + offset1;
        int num = lineFromVisualTop.GetRelativeOffset(visualColumn3) + offset1;
        return new SimpleSegment(offset2, num - offset2);
    }

    private SimpleSegment GetLineAtMousePosition(PointerEventArgs e)
    {
        TextView textView = TextArea.TextView;
        if (textView == null) return SimpleSegment.Invalid;
        Point point = e.GetPosition(textView);
        if (point.Y < 0.0) point = point.WithY(0.0);
        if (point.Y > textView.Bounds.Height) point = point.WithY(textView.Bounds.Height);
        point += textView.ScrollOffset;
        VisualLine lineFromVisualTop = textView.GetVisualLineFromVisualTop(point.Y);
        return lineFromVisualTop != null ? new SimpleSegment(lineFromVisualTop.StartOffset, lineFromVisualTop.LastDocumentLine.EndOffset - lineFromVisualTop.StartOffset) : SimpleSegment.Invalid;
    }

    private int GetOffsetFromMousePosition(PointerEventArgs e, out int visualColumn, out bool isAtEndOfLine)
    {
        return GetOffsetFromMousePosition(e.GetPosition(TextArea.TextView), out visualColumn, out isAtEndOfLine);
    }

    private int GetOffsetFromMousePosition(Point positionRelativeToTextView, out int visualColumn, out bool isAtEndOfLine)
    {
        visualColumn = 0;
        TextView textView = TextArea.TextView;
        Point point = positionRelativeToTextView;
        if (point.Y < 0.0) point = point.WithY(0.0);
        if (point.Y > textView.Bounds.Height) point = point.WithY(textView.Bounds.Height);
        point += textView.ScrollOffset;
        if (point.Y >= textView.DocumentHeight) point = point.WithY(textView.DocumentHeight - 0.01);
        VisualLine lineFromVisualTop = textView.GetVisualLineFromVisualTop(point.Y);
        if (lineFromVisualTop != null)
        {
            visualColumn = lineFromVisualTop.GetVisualColumn(point, TextArea.Selection.EnableVirtualSpace, out isAtEndOfLine);
            return lineFromVisualTop.GetRelativeOffset(visualColumn) + lineFromVisualTop.FirstDocumentLine.Offset;
        }

        isAtEndOfLine = false;
        return -1;
    }

    private int GetOffsetFromMousePositionFirstTextLineOnly(Point positionRelativeToTextView, out int visualColumn)
    {
        visualColumn = 0;
        TextView textView = TextArea.TextView;
        Point point = positionRelativeToTextView;
        if (point.Y < 0.0) point = point.WithY(0.0);
        if (point.Y > textView.Bounds.Height) point = point.WithY(textView.Bounds.Height);
        point += textView.ScrollOffset;
        if (point.Y >= textView.DocumentHeight) point = point.WithY(textView.DocumentHeight - 0.01);
        VisualLine lineFromVisualTop = textView.GetVisualLineFromVisualTop(point.Y);
        if (lineFromVisualTop == null) return -1;
        visualColumn = lineFromVisualTop.GetVisualColumn(lineFromVisualTop.TextLines.First<TextLine>(), point.X, TextArea.Selection.EnableVirtualSpace);
        return lineFromVisualTop.GetRelativeOffset(visualColumn) + lineFromVisualTop.FirstDocumentLine.Offset;
    }

    private void TextArea_MouseMove(object sender, PointerEventArgs e)
    {
        if (e.Handled) return;
        if (_mode == SelectionMode.Normal || _mode == SelectionMode.WholeWord || _mode == SelectionMode.WholeLine || _mode == SelectionMode.Rectangular)
        {
            e.Handled = true;
            if (!TextArea.TextView.VisualLinesValid) return;
            ExtendSelectionToMouse(e);
        }
        else
        {
            if (_mode != SelectionMode.PossibleDragStart) return;
            e.Handled = true;
            Vector vector = e.GetPosition(TextArea) - _possibleDragStartMousePos;
            if (Math.Abs(vector.X) <= 2.0 && Math.Abs(vector.Y) <= 2.0)
            {
            }
        }
    }

    private void SetCaretOffsetToMousePosition(PointerEventArgs e, ISegment allowedSegment = null)
    {
        int visualColumn;
        int offset;
        bool isAtEndOfLine;
        if (_mode == SelectionMode.Rectangular)
        {
            offset = GetOffsetFromMousePositionFirstTextLineOnly(e.GetPosition(TextArea.TextView), out visualColumn);
            isAtEndOfLine = true;
        }
        else
            offset = GetOffsetFromMousePosition(e, out visualColumn, out isAtEndOfLine);

        if (allowedSegment != null) offset = offset.CoerceValue(allowedSegment.Offset, allowedSegment.EndOffset);
        if (offset < 0) return;
        TextArea.Caret.Position = new TextViewPosition(TextArea.Document.GetLocation(offset), visualColumn)
        {
            IsAtEndOfLine = isAtEndOfLine
        };
        TextArea.Caret.DesiredXPos = double.NaN;
    }

    private void ExtendSelectionToMouse(PointerEventArgs e)
    {
        TextViewPosition position = TextArea.Caret.Position;
        if (_mode == SelectionMode.Normal || _mode == SelectionMode.Rectangular)
        {
            SetCaretOffsetToMousePosition(e);
            TextArea.Selection = _mode != SelectionMode.Normal || !(TextArea.Selection is RectangleSelection)
                ? (_mode != SelectionMode.Rectangular || TextArea.Selection is RectangleSelection ? TextArea.Selection.StartSelectionOrSetEndpoint(position, TextArea.Caret.Position) : new RectangleSelection(TextArea, position, TextArea.Caret.Position))
                : ExposeExtensions.CreateSimpleSelection(TextArea, position, TextArea.Caret.Position);
        }
        else if (_mode == SelectionMode.WholeWord || _mode == SelectionMode.WholeLine)
        {
            SimpleSegment simpleSegment = _mode == SelectionMode.WholeLine ? GetLineAtMousePosition(e) : GetWordAtMousePosition(e);
            if (simpleSegment != SimpleSegment.Invalid && _startWord != null)
            {
                TextArea.Selection = Selection.Create(TextArea, Math.Min(simpleSegment.Offset, _startWord.Offset), Math.Max(simpleSegment.EndOffset, _startWord.EndOffset));
                TextArea.Caret.Offset = simpleSegment.Offset < _startWord.Offset ? simpleSegment.Offset : Math.Max(simpleSegment.EndOffset, _startWord.EndOffset);
            }
        }

        TextArea.Caret.BringCaretToView(5.0);
    }

    private void TextArea_MouseLeftButtonUp(object sender, PointerEventArgs e)
    {
        if (_mode == SelectionMode.None || e.Handled) return;
        e.Handled = true;
        switch (_mode)
        {
            case SelectionMode.PossibleDragStart:
                SetCaretOffsetToMousePosition(e);
                TextArea.ClearSelection();
                break;
            case SelectionMode.Normal:
            case SelectionMode.WholeWord:
            case SelectionMode.WholeLine:
            case SelectionMode.Rectangular:
                ExtendSelectionToMouse(e);
                break;
        }

        _mode = SelectionMode.None;
        TextArea.ReleasePointerCapture(e.Pointer);
    }

    private enum SelectionMode
    {
        None,
        PossibleDragStart,
        Drag,
        Normal,
        WholeWord,
        WholeLine,
        Rectangular,
    }
}
