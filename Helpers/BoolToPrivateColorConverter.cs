using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace vengar.Helpers;

public class BoolToPrivateColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is true)
            return Brushes.OrangeRed;   // private
        if (value is false)
            return Brushes.MediumSpringGreen;     // public

        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
