using System;
using System.Reflection;
using Avalonia;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Text;

// ReSharper disable InconsistentNaming

namespace TAS.Avalonia.Editing;

#nullable disable

public static class ExposeExtensions
{
    #region TextArea

    private static readonly MethodInfo TextArea_RemoveSelectedText = typeof(TextArea).GetMethod("RemoveSelectedText", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo TextArea_ReplaceSelectionWithText = typeof(TextArea).GetMethod("ReplaceSelectionWithText", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo TextArea_GetDeletableSegments = typeof(TextArea).GetMethod("GetDeletableSegments", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo TextArea_OnTextCopied = typeof(TextArea).GetMethod("OnTextCopied", BindingFlags.Instance | BindingFlags.NonPublic);

    private static T Invoke<T>(MethodInfo methodInfo, TextArea textArea, params object[] args) =>
        (T)methodInfo.Invoke(textArea, args);

    public static void RemoveSelectedText(this TextArea self) =>
        Invoke<object>(TextArea_RemoveSelectedText, self);

    public static void ReplaceSelectionWithText(this TextArea self, string newText) =>
        Invoke<object>(TextArea_ReplaceSelectionWithText, self, newText);

    public static ISegment[] GetDeletableSegments(this TextArea self, ISegment segment) =>
        Invoke<ISegment[]>(TextArea_GetDeletableSegments, self, segment);

    public static void OnTextCopied(this TextArea self, TextEventArgs e) =>
        Invoke<object>(TextArea_OnTextCopied, self, e);

    #endregion

    #region VisualLine

    // copied from VisualLine since it's hard to reflect with an out parameter
    public static int GetVisualColumn(this VisualLine self, Point point, bool allowVirtualSpace, out bool isAtEndOfLine)
    {
        TextLine byVisualYposition = self.GetTextLineByVisualYPosition(point.Y);
        int visualColumn = self.GetVisualColumn(byVisualYposition, point.X, allowVirtualSpace);
        isAtEndOfLine = visualColumn >= self.GetTextLineVisualStartColumn(byVisualYposition) + byVisualYposition.Length;
        return visualColumn;
    }

    #endregion

    #region SimpleSelection

    public static SimpleSelection CreateSimpleSelection(TextArea textArea, TextViewPosition start, TextViewPosition end)
    {
        var cons = typeof(SimpleSelection).GetConstructor(new[]
        {
            typeof(TextArea),
            typeof(TextViewPosition),
            typeof(TextViewPosition),
        });
        return (SimpleSelection)cons?.Invoke(new object[] { textArea, start, end });
    }

    #endregion
}
