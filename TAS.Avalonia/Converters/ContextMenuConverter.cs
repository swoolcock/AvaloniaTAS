using System.Globalization;
using Avalonia.Data.Converters;
using TAS.Avalonia.Models;

namespace TAS.Avalonia.Converters;

public class ContextMenuConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is IEnumerable<MenuModel> menuModel ? menuModel.ToContextMenu() : throw new ArgumentException("Not IEnumerable<MenuModel>", nameof(value));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
