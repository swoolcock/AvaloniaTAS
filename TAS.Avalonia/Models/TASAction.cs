namespace TAS.Avalonia.Models;

// ReSharper disable InconsistentNaming
[Flags]
public enum TASAction {
    None = 0,

    Left = 1 << 0, // L
    Right = 1 << 1, // R
    LRF = Right | Left | FeatherAim,

    Up = 1 << 2, // U
    Down = 1 << 3, // D
    UDF = Up | Down | FeatherAim,

    JumpConfirm = 1 << 4, // J
    Jump2 = 1 << 5, // K
    JK = JumpConfirm | Jump2,

    DashTalkCancel = 1 << 6, // X
    Dash2Cancel2 = 1 << 7, // C
    CrouchDash = 1 << 8, // Z
    CrouchDash2 = 1 << 9, // V
    XCZV = DashTalkCancel | Dash2Cancel2 | CrouchDash | CrouchDash2,

    Grab = 1 << 10, // G
    Pause = 1 << 11, // S
    QuickRestart = 1 << 12, // Q

    JournalTalk2 = 1 << 13, // N
    Confirm2 = 1 << 14, // O
    DashOnly = 1 << 15, // A
    MoveOnly = 1 << 16, // M
    CustomBinding = 1 << 17, // P
    FeatherAim = 1 << 18, // F

    LeftDashOnly = 1 << 19, // AL
    RightDashOnly = 1 << 20, // AR
    DashOnlyLR = LeftDashOnly | RightDashOnly,

    UpDashOnly = 1 << 21, // AU
    DownDashOnly = 1 << 22, // AD
    DashOnlyUD = UpDashOnly | DownDashOnly,

    LeftMoveOnly = 1 << 23, // ML
    RightMoveOnly = 1 << 24, // MR
    MoveOnlyLR = LeftMoveOnly | RightMoveOnly,

    UpMoveOnly = 1 << 25, // MU
    DownMoveOnly = 1 << 26, // MD
    MoveOnlyUD = UpMoveOnly | DownMoveOnly,
}

public static class TASActionExtensions {
    public static TASAction ActionForChar(char c) =>
        c.ToString().ToUpper()[0] switch {
            'R' => TASAction.Right,
            'L' => TASAction.Left,
            'U' => TASAction.Up,
            'D' => TASAction.Down,
            'J' => TASAction.JumpConfirm,
            'K' => TASAction.Jump2,
            'X' => TASAction.DashTalkCancel,
            'C' => TASAction.Dash2Cancel2,
            'Z' => TASAction.CrouchDash,
            'V' => TASAction.CrouchDash2,
            'G' => TASAction.Grab,
            'S' => TASAction.Pause,
            'Q' => TASAction.QuickRestart,
            'N' => TASAction.JournalTalk2,
            'O' => TASAction.Confirm2,
            'A' => TASAction.DashOnly,
            'M' => TASAction.MoveOnly,
            'P' => TASAction.CustomBinding,
            'F' => TASAction.FeatherAim,
            _ => TASAction.None,
        };

    public static char CharForAction(this TASAction self) =>
        self switch {
            TASAction.Right => 'R',
            TASAction.Left => 'L',
            TASAction.Up => 'U',
            TASAction.Down => 'D',
            TASAction.JumpConfirm => 'J',
            TASAction.Jump2 => 'K',
            TASAction.DashTalkCancel => 'X',
            TASAction.Dash2Cancel2 => 'C',
            TASAction.CrouchDash => 'Z',
            TASAction.CrouchDash2 => 'V',
            TASAction.Grab => 'G',
            TASAction.Pause => 'S',
            TASAction.QuickRestart => 'Q',
            TASAction.JournalTalk2 => 'N',
            TASAction.Confirm2 => 'O',
            TASAction.DashOnly => 'A',
            TASAction.MoveOnly => 'M',
            TASAction.CustomBinding => 'P',
            TASAction.FeatherAim => 'F',
            _ => ' ',
        };

    public static TASAction ToggleAction(this TASAction self, TASAction other) {
        if (self.HasFlag(other))
            return self & ~other;
        return other switch {
            TASAction.Left or TASAction.Right or TASAction.FeatherAim => (self & ~TASAction.LRF) | other,
            TASAction.Up or TASAction.Down or TASAction.FeatherAim => (self & ~TASAction.UDF) | other,
            TASAction.JumpConfirm or TASAction.Jump2 => (self & ~TASAction.JK) | other,
            TASAction.DashTalkCancel or TASAction.Dash2Cancel2 or TASAction.CrouchDash or TASAction.CrouchDash2 => (self & ~TASAction.XCZV) | other,
            TASAction.LeftDashOnly or TASAction.RightDashOnly => (self & ~TASAction.DashOnlyLR) | other,
            TASAction.UpDashOnly or TASAction.DownDashOnly => (self & ~TASAction.DashOnlyUD) | other,
            TASAction.LeftMoveOnly or TASAction.RightMoveOnly => (self & ~TASAction.MoveOnlyLR) | other,
            TASAction.UpMoveOnly or TASAction.DownMoveOnly => (self & ~TASAction.MoveOnlyUD) | other,
            _ => self,
        };
    }

    public static IEnumerable<TASAction> Sorted(this TASAction self) => new[] {
        TASAction.Left,
        TASAction.Right,
        TASAction.Up,
        TASAction.Down,
        TASAction.JumpConfirm,
        TASAction.Jump2,
        TASAction.DashTalkCancel,
        TASAction.Dash2Cancel2,
        TASAction.CrouchDash,
        TASAction.CrouchDash2,
        TASAction.Grab,
        TASAction.Pause,
        TASAction.QuickRestart,
        TASAction.JournalTalk2,
        TASAction.Confirm2,
        TASAction.DashOnly,
        TASAction.MoveOnly,
        TASAction.CustomBinding,
        TASAction.FeatherAim,
    }.Where(e => self.HasFlag(e));
}
