using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Converter that generates tooltip text and URLs for properties.
    /// Uses ConverterParameter for property name.
    /// </summary>
    public class TooltipConverter : MarkupExtension, IValueConverter
    {
        public Type? PropertyType { get; set; }
        public Type? ParentType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var propertyName = parameter as string;
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            var tooltipInfo = TooltipInfoExtractor.GetTooltipInfo(propertyName, PropertyType, ParentType);
            
            if (!tooltipInfo.HasContent)
                return null;

            // Build tooltip text
            var tooltipText = tooltipInfo.Text ?? string.Empty;
            
            if (!string.IsNullOrWhiteSpace(tooltipInfo.DocumentationUrl))
            {
                if (!string.IsNullOrWhiteSpace(tooltipText))
                {
                    tooltipText += $"\n\nClick icon to open documentation";
                }
                else
                {
                    tooltipText = "Click icon to open documentation";
                }
            }

            return tooltipText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter that extracts documentation URL for properties.
    /// Uses ConverterParameter for property name.
    /// </summary>
    public class DocumentationUrlConverter : MarkupExtension, IValueConverter
    {
        public Type? PropertyType { get; set; }
        public Type? ParentType { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var propertyName = parameter as string;
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            var tooltipInfo = TooltipInfoExtractor.GetTooltipInfo(propertyName, PropertyType, ParentType);
            return tooltipInfo?.DocumentationUrl;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

