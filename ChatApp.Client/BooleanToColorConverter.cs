using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ChatApp.Client;

public class BooleanToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            // Parameter format: "trueColor:falseColor"
            var colors = param.Split(':');
            if (colors.Length == 2)
            {
                var colorString = boolValue ? colors[0] : colors[1];
                if (Color.TryParse(colorString, out var color))
                {
                    return new SolidColorBrush(color);
                }
            }
        }
        // Default color
        return new SolidColorBrush(Color.Parse("#1A2742"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

