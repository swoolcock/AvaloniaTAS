using Avalonia.Input;
using AvaloniaEdit;

namespace TAS.Avalonia.Editing;

public enum CaretMovementType
{
    None,
    CharLeft,
    CharRight,
    Backspace,
    WordLeft,
    WordRight,
    LineUp,
    LineDown,
    PageUp,
    PageDown,
    LineStart,
    LineEnd,
    DocumentStart,
    DocumentEnd,
}

public static class CaretMovementTypeExtensions
{
    public static CaretMovementType ForKeyBinding(KeyBinding binding)
    {
        var command = binding.Command;
        if (command == EditingCommands.MoveLeftByCharacter)
            return CaretMovementType.CharLeft;
        if (command == EditingCommands.MoveRightByCharacter)
            return CaretMovementType.CharRight;
        if (command == EditingCommands.Backspace)
            return CaretMovementType.Backspace;
        if (command == EditingCommands.MoveLeftByWord)
            return CaretMovementType.WordLeft;
        if (command == EditingCommands.MoveRightByWord)
            return CaretMovementType.WordRight;
        if (command == EditingCommands.MoveUpByLine)
            return CaretMovementType.LineUp;
        if (command == EditingCommands.MoveDownByLine)
            return CaretMovementType.LineDown;
        if (command == EditingCommands.MoveUpByPage)
            return CaretMovementType.PageUp;
        if (command == EditingCommands.MoveDownByPage)
            return CaretMovementType.PageDown;
        if (command == EditingCommands.MoveToLineStart)
            return CaretMovementType.LineStart;
        if (command == EditingCommands.MoveToLineEnd)
            return CaretMovementType.LineEnd;
        if (command == EditingCommands.MoveToDocumentStart)
            return CaretMovementType.DocumentStart;
        if (command == EditingCommands.MoveToDocumentEnd)
            return CaretMovementType.DocumentEnd;
        return CaretMovementType.None;
    }
}
