namespace ChatApp.Client;
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using ChatApp.Client.Models;

public class RoleToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ChatRoleType role && parameter is string expected)
        {
            if (Enum.TryParse<ChatRoleType>(expected, out var exp))
            {
                return role == exp;
            }
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}