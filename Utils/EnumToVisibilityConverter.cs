using System;
using System.Globalization;
using System.Windows.Data;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Converter that shows visibility when an enum value matches a parameter
    /// </summary>
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return System.Windows.Visibility.Collapsed;

            string valueStr = value.ToString() ?? "";
            string paramStr = parameter.ToString() ?? "";
            
            return valueStr.Equals(paramStr, StringComparison.InvariantCultureIgnoreCase) 
                ? System.Windows.Visibility.Visible 
                : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

