namespace TAS.Core.Models;

public struct TASActionLine
{
    public const int MaxFrames = 9999;
    public const int MaxFramesDigits = 4;

    public TASAction Actions;
    public int Frames;

    public static bool TryParse(string line, out TASActionLine value)
    {
        value = default;
        var tokens = line.Trim().Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return false;

        if (!int.TryParse(tokens[0], out var frames)) return false;
        TASAction actions = tokens.Skip(1).Aggregate(TASAction.None, (a, s) =>
        {
            if (string.IsNullOrWhiteSpace(s)) return a;
            var actionForChar = TASActionExtensions.ActionForChar(s[0]);
            return a | actionForChar;
        });

        value = new TASActionLine
        {
            Frames = frames,
            Actions = actions,
        };

        return true;
    }

    public override string ToString() =>
        Actions.Sorted().Aggregate(Frames.ToString().PadLeft(MaxFramesDigits), (s, a) => s + "," + a.CharForAction());
}
