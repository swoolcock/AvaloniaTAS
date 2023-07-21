using Avalonia.Data;

namespace TAS.Avalonia.Models;

#nullable enable

public struct TASActionLine {
    public const int MaxFrames = 9999;
    public const int MaxFramesDigits = 4;

    public TASAction Actions;
    public int Frames;

    public string? FeatherAngle;
    public string? FeatherMagnitude;

    public static bool TryParse(string line, out TASActionLine value, bool ignoreInvalidFloats = true) {
        value = default;
        string[] tokens = line.Trim().Split(",", StringSplitOptions.TrimEntries);
        if (tokens.Length == 0) return false;

        if (!int.TryParse(tokens[0], out value.Frames)) return false;

        for (int i = 1; i < tokens.Length; i++) {
            if (string.IsNullOrWhiteSpace(tokens[i])) continue;

            var action = TASActionExtensions.ActionForChar(tokens[i][0]);
            value.Actions |= action;

            // Parse dash-only/move-only/custom bindings
            if (action is TASAction.DashOnly or TASAction.MoveOnly) {

            }

            // Parse feather angle/magnitude
            bool validAngle = true;
            if (action == TASAction.FeatherAim && i + 1 < tokens.Length && (validAngle = float.TryParse(tokens[i + 1], out var _))) {
                value.FeatherAngle = tokens[i + 1];
                i++;

                // Allow empty magnitude, so the comma won't get removed
                bool validMagnitude = true;
                if (i + 1 < tokens.Length && (string.IsNullOrWhiteSpace(tokens[i + 1]) || (validMagnitude = float.TryParse(tokens[i + 1], out var _)))) {
                    value.FeatherMagnitude = tokens[i + 1];
                    i++;
                } else if (!validMagnitude && !ignoreInvalidFloats) {
                    return false;
                }
            } else if (!validAngle && !ignoreInvalidFloats) {
                return false;
            }
        }

        return true;
    }

    public override string ToString() {
        string frames = Frames.ToString().PadLeft(MaxFramesDigits);
        string actions = Actions.Sorted().Aggregate("", (s, a) => $"{s},{a.CharForAction()}");
        string featherAngle = $"{(Actions.HasFlag(TASAction.FeatherAim) ? "," : "")}{FeatherAngle ?? ""}";
        string featherMagnitude = FeatherMagnitude != null ? $",{FeatherMagnitude}" : string.Empty;

        return $"{frames}{actions}{featherAngle}{featherMagnitude}";
    }

}
