using System;
using System.Collections.Generic;
using System.Linq;

namespace TAS.Avalonia.Models;

// ReSharper disable InconsistentNaming

[Flags]
public enum TASAction
{
    None = 0,

    Right = 1 << 0, // R
    Left = 1 << 1, // L
    RL = Right | Left,

    Up = 1 << 2, // U
    Down = 1 << 3, // D
    UD = Up | Down,

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
    GSQ = Grab | Pause | QuickRestart,

    JournalTalk2 = 1 << 13, // N
    Confirm2 = 1 << 14, // O
    DashOnlyDirection = 1 << 15, // A
    FeatherAim = 1 << 16, // F
    NOAF = JournalTalk2 | Confirm2 | DashOnlyDirection | FeatherAim,
}

public static class TASActionExtensions
{
    public static TASAction ActionForChar(char c) =>
        c.ToString().ToUpper()[0] switch
        {
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
            'A' => TASAction.DashOnlyDirection,
            'F' => TASAction.FeatherAim,
            _ => TASAction.None,
        };

    public static char CharForAction(this TASAction self) =>
        self switch
        {
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
            TASAction.DashOnlyDirection => 'A',
            TASAction.FeatherAim => 'F',
            _ => ' ',
        };

    public static TASAction ToggleAction(this TASAction self, TASAction other)
    {
        if (self.HasFlag(other))
            return self & ~other;
        return other switch
        {
            TASAction.Left or TASAction.Right => (self & ~TASAction.RL) | other,
            TASAction.Up or TASAction.Down => (self & ~TASAction.UD) | other,
            TASAction.JumpConfirm or TASAction.Jump2 => (self & ~TASAction.JK) | other,
            TASAction.DashTalkCancel or TASAction.Dash2Cancel2 or TASAction.CrouchDash or TASAction.CrouchDash2 => (self & ~TASAction.XCZV) | other,
            TASAction.Grab or TASAction.Pause or TASAction.QuickRestart => (self & ~TASAction.GSQ) | other,
            TASAction.JournalTalk2 or TASAction.Confirm2 or TASAction.DashOnlyDirection or TASAction.FeatherAim => (self & ~TASAction.NOAF) | other,
            _ => self,
        };
    }

    public static IEnumerable<TASAction> Sorted(this TASAction self) => new[]
    {
        TASAction.Right, TASAction.Left,
        TASAction.Up, TASAction.Down,
        TASAction.JumpConfirm, TASAction.Jump2,
        TASAction.DashTalkCancel, TASAction.Dash2Cancel2, TASAction.CrouchDash, TASAction.CrouchDash2,
        TASAction.Grab, TASAction.Pause, TASAction.QuickRestart,
        TASAction.JournalTalk2, TASAction.Confirm2, TASAction.DashOnlyDirection, TASAction.FeatherAim,
    }.Where(e => self.HasFlag(e));
}
