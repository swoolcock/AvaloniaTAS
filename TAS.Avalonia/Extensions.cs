using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace TAS.Avalonia;

public static class Extensions {
    public static int Digits(this int self) => Math.Abs(self).ToString().Length;

    public static IObservable<T> AsObservable<T>(this T self) => Observable.Return(self);
    public static IEnumerable<T> Yield<T>(this T self) => new[] { self };

    public static IClassicDesktopStyleApplicationLifetime DesktopLifetime(this Application self) => (IClassicDesktopStyleApplicationLifetime) self.ApplicationLifetime;

    public static string ReplaceRange(this string self, int startIndex, int count, string replacement) => self.Remove(startIndex, count).Insert(startIndex, replacement);
}
