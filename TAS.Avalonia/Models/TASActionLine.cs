namespace TAS.Avalonia.Models;

public struct TASActionLine {
    public const int MaxFrames = 9999;
    public const int MaxFramesDigits = 4;

    public TASAction Actions;
    public int Frames;

    public float? FeatherAngle;
    public float? FeatherMagnitude;
    public bool HasMagnitudeCommand;

    public static bool TryParse(string line, out TASActionLine value) {
        value = default;
        string[] tokens = line.Trim().Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return false;

        if (!int.TryParse(tokens[0], out value.Frames)) return false;

        for (int i = 1; i < tokens.Length; i++) {
            var action = TASActionExtensions.ActionForChar(tokens[i][0]);

            if (action == TASAction.FeatherAim && i + 1 < tokens.Length && float.TryParse(tokens[i + 1], out var angle)) {
                value.FeatherAngle = angle;
                i++;

                if (i + 1 < tokens.Length && float.TryParse(tokens[i + 1], out var magnitude)) {
                    value.FeatherMagnitude = magnitude;
                    i++;
                }
            }

            value.Actions |= action;
        }

        return true;
    }

    public override string ToString() {
        string frames = Frames.ToString().PadLeft(MaxFramesDigits);
        string actions = Actions.Sorted().Aggregate("", (s, a) => $"{s},{a.CharForAction()}");
        string featherAngle = FeatherAngle.HasValue ? $",{FeatherAngle.Value}" : string.Empty;
        string featherMagnitude = FeatherMagnitude.HasValue ? $",{FeatherMagnitude.Value}" : string.Empty;

        return $"{frames}{actions}{featherAngle}{featherMagnitude}";
    }

}
