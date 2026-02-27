using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WindowsKontrolMerkezi.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color c)
                return new SolidColorBrush(c);
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush b)
                return b.Color;
            return Binding.DoNothing;
        }
    }
}