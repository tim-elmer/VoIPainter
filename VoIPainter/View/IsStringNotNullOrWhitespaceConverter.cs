using System;
using System.Globalization;
using System.Windows.Data;

namespace VoIPainter.View
{
    /// <summary>
    /// Return if a string is not null or whitespace
    /// </summary>
    [ValueConversion(typeof(string), typeof(bool))]
    public class IsStringNotNullOrWhitespaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
                return null;
            else if (value is null)
                return false;
            else if (value is string)
                return !string.IsNullOrWhiteSpace(value as string);
            else
                return !string.IsNullOrWhiteSpace(value.ToString());

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
