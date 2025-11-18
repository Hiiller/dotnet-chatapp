using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ChatApp.Client;

public class BooleanToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var colors = param.Split(':');
            if (colors.Length == 2)
            {
                var token = boolValue ? colors[0] : colors[1];
                var brush = ResolveBrush(token);
                if (brush != null)
                {
                    return brush;
                }
            }
        }

        return ResolveBrush("ThemeBrush.CardBackground") ?? new SolidColorBrush(Color.Parse("#1A2742"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static IBrush? ResolveBrush(string token)
    {
        token = token?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        if (token.StartsWith("#", StringComparison.OrdinalIgnoreCase))
        {
            if (Color.TryParse(token, out var parsed))
            {
                return new SolidColorBrush(parsed);
            }
            return null;
        }

        var app = Application.Current;
        if (app != null && app.Resources.TryGetResource(token, null, out var resource))
        {
            if (resource is IBrush brush)
            {
                return brush;
            }

            if (resource is Color color)
            {
                return new SolidColorBrush(color);
            }
        }

        return null;
    }
}

