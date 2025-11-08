using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Converts HEX strings into SolidColorBrush instances for previews.
    /// </summary>
    public class ColorHexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var hex = value as string;
                return ColorUtils.ToBrush(hex);
            }
            catch
            {
                var brush = new SolidColorBrush(Colors.Transparent);
                brush.Freeze();
                return brush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return $"#{brush.Color.A:X2}{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            }

            return "#FFFFFFFF";
        }
    }
}
