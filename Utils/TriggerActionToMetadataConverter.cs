using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Converter that converts a trigger's TargetAction string to TriggerMetadata and back
    /// </summary>
    public class TriggerActionToMetadataConverter : IValueConverter
    {
        public System.Collections.ObjectModel.ObservableCollection<TriggerMetadata>? AvailableTriggers { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string targetAction && !string.IsNullOrWhiteSpace(targetAction) && AvailableTriggers != null)
            {
                return AvailableTriggers.FirstOrDefault(t => t.TargetAction == targetAction);
            }
            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TriggerMetadata metadata)
            {
                return metadata.TargetAction;
            }
            return null;
        }
    }
}

