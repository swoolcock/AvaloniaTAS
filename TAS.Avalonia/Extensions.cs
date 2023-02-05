using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using TAS.Avalonia.Models;

namespace TAS.Avalonia;

#nullable disable

public static class Extensions {
    public static TASAction ActionForKey(this Key self) =>
        self switch {
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

    public static bool ValidForAction(this KeyGesture self) =>
        self.KeyModifiers == KeyModifiers.None && self.Key.NumberForKey() >= 0 ||
        self.KeyModifiers is KeyModifiers.None or KeyModifiers.Shift && self.Key.ActionForKey() != TASAction.None;

    public static int NumberForKey(this Key self) =>
        self switch {
            >= Key.D0 and <= Key.D9 => self - Key.D0,
            >= Key.NumPad0 and < Key.NumPad9 => self - Key.NumPad0,
            _ => -1,
        };

    public static int Digits(this int self) => Math.Abs(self).ToString().Length;

    public static IObservable<T> AsObservable<T>(this T self) => Observable.Return(self);
    public static IEnumerable<T> Yield<T>(this T self) => new[] { self };

    public static IClassicDesktopStyleApplicationLifetime DesktopLifetime(this Application self) => (IClassicDesktopStyleApplicationLifetime) self.ApplicationLifetime;
}
