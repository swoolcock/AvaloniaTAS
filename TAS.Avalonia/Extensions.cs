using System;
using Avalonia.Input;
using TAS.Core.Models;

namespace TAS.Avalonia;

public static class Extensions
{
    public static TASAction ActionForKey(this Key self) =>
        self switch
        {
            Key.R => TASAction.Right,
            Key.L => TASAction.Left,
            Key.U => TASAction.Up,
            Key.D => TASAction.Down,
            Key.J => TASAction.JumpConfirm,
            Key.K => TASAction.Jump2,
            Key.X => TASAction.DashTalkCancel,
            Key.C => TASAction.Dash2Cancel2,
            Key.Z => TASAction.CrouchDash,
            Key.V => TASAction.CrouchDash2,
            Key.G => TASAction.Grab,
            Key.S => TASAction.Pause,
            Key.Q => TASAction.QuickRestart,
            Key.N => TASAction.JournalTalk2,
            Key.O => TASAction.Confirm2,
            Key.A => TASAction.DashOnlyDirection,
            Key.F => TASAction.FeatherAim,
            _ => TASAction.None,
        };

    public static int NumberForKey(this Key self) =>
        self switch
        {
            >= Key.D0 and <= Key.D9 => self - Key.D0,
            >= Key.NumPad0 and < Key.NumPad9 => self - Key.NumPad0,
            _ => -1,
        };

    public static int Digits(this int self) => Math.Abs(self).ToString().Length;
}
