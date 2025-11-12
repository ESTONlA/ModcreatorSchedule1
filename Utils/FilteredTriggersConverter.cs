using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using Schedule1ModdingTool.Models;
using Schedule1ModdingTool.Services;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Multi-value converter that filters triggers by TriggerType
    /// </summary>
    public class FilteredTriggersConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;

            if (values[0] is ObservableCollection<TriggerMetadata> triggers && values[1] is QuestTriggerType triggerType)
            {
                return triggers.Where(t => t.TriggerType == triggerType).ToList();
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

