using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;

namespace Schedule1ModdingTool.Utils
{
    /// <summary>
    /// Converter that returns Collapsed visibility when count is 0
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for boolean to visibility
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converter for inverting boolean values
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converter for string to visibility (visible if not null/empty)
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = value is string str && string.IsNullOrWhiteSpace(str);
            bool inverse = parameter?.ToString()?.Equals("Inverse", StringComparison.InvariantCultureIgnoreCase) == true;
            
            if (inverse)
            {
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for icon key string to PackIconKind enum value
    /// </summary>
    public class IconKeyToPackIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconKey && !string.IsNullOrEmpty(iconKey))
            {
                // Map icon keys to MaterialDesign PackIconKind values
                return iconKey switch
                {
                    "PlusIcon" => PackIconKind.Plus,
                    "FolderIcon" => PackIconKind.Folder,
                    "PadlockIcon" => PackIconKind.Lock,
                    "GlobeIcon" => PackIconKind.Web,
                    "SearchIcon" => PackIconKind.Magnify,
                    "FilterIcon" => PackIconKind.Filter,
                    "SortIcon" => PackIconKind.Sort,
                    "SettingsIcon" => PackIconKind.Cog,
                    "BuildIcon" => PackIconKind.Hammer,
                    "SaveIcon" => PackIconKind.ContentSave,
                    "DocumentIcon" => PackIconKind.FileDocument,
                    "FolderOpenIcon" => PackIconKind.FolderOpen,
                    "CubeIcon" => PackIconKind.Cube,
                    "QuestIcon" => PackIconKind.ClipboardCheck,
                    "NPCsIcon" => PackIconKind.AccountGroup,
                    "PhoneAppsIcon" => PackIconKind.Cellphone,
                    "ItemsIcon" => PackIconKind.Package,
                    "NewProjectIcon" => PackIconKind.FileDocumentPlus,
                    "SaveAsIcon" => PackIconKind.ContentSaveAll,
                    "CodeRefreshIcon" => PackIconKind.FileRefresh,
                    "DeleteIcon" => PackIconKind.Delete,
                    "EditIcon" => PackIconKind.Pencil,
                    "RefreshIcon" => PackIconKind.Refresh,
                    "ExportIcon" => PackIconKind.Download,
                    "CopyIcon" => PackIconKind.ContentCopy,
                    "EyeIcon" => PackIconKind.Eye,
                    _ => PackIconKind.QuestionMark
                };
            }
            return PackIconKind.QuestionMark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for icon key string to Geometry resource (deprecated - kept for backward compatibility)
    /// </summary>
    public class IconKeyToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconKey && !string.IsNullOrEmpty(iconKey))
            {
                var geometry = Application.Current.TryFindResource(iconKey) as Geometry;
                return geometry ?? Geometry.Empty;
            }
            return Geometry.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for boolean to navigation item style (selected vs normal)
    /// </summary>
    public class BooleanToNavigationItemStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return Application.Current.TryFindResource(
                    isSelected ? "SelectedNavigationItemStyle" : "NavigationItemStyle");
            }
            return Application.Current.TryFindResource("NavigationItemStyle");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for boolean to opacity (disabled items are semi-transparent)
    /// </summary>
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                return isEnabled ? 1.0 : 0.5;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Inverse boolean to visibility converter
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for null to boolean (false if null, true if not null)
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for null to visibility (Visible if null, Collapsed if not null)
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for count to visibility inverse (Visible when count is 0, Collapsed otherwise)
    /// </summary>
    public class CountToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for boolean to tab foreground (orange when selected, gray otherwise)
    /// </summary>
    public class BooleanToTabForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return Application.Current.TryFindResource(
                    isSelected ? "AccentOrangeBrush" : "MediumTextBrush") ?? System.Windows.Media.Brushes.Gray;
            }
            return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for boolean to GridLength (Star when true, zero pixels when false)
    /// </summary>
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
            }
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
            {
                return gridLength.GridUnitType == GridUnitType.Star && gridLength.Value > 0;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts a relative resource path and project file path into an ImageSource.
    /// </summary>
    public class RelativeResourcePathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                Debug.WriteLine("[RelativeResourcePathConverter] Missing binding values");
                return Binding.DoNothing;
            }

            var relativePath = values[0] as string;
            var projectFile = values[1] as string;
            if (string.IsNullOrWhiteSpace(relativePath) || string.IsNullOrWhiteSpace(projectFile))
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] Null path (relative: '{relativePath ?? "null"}', projectFile: '{projectFile ?? "null"}')");
                return Binding.DoNothing;
            }

            var projectDir = Path.GetDirectoryName(projectFile);
            if (string.IsNullOrWhiteSpace(projectDir))
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] Could not resolve project directory for '{projectFile}'");
                return Binding.DoNothing;
            }

            var absolutePath = ResolveAbsolutePath(relativePath, projectDir);
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] ResolveAbsolutePath returned empty for '{relativePath}'");
                return Binding.DoNothing;
            }

            if (!File.Exists(absolutePath))
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] File not found at '{absolutePath}'");
                return Binding.DoNothing;
            }

            try
            {
                var fileInfo = new FileInfo(absolutePath);
                Debug.WriteLine($"[RelativeResourcePathConverter] Preparing to load '{absolutePath}' ({fileInfo.Length} bytes)");
            }
            catch (Exception sizeEx)
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] Failed to inspect '{absolutePath}': {sizeEx.Message}");
            }

            try
            {
                var imageData = ReadAllBytesShared(absolutePath);
                if (imageData == null)
                {
                    Debug.WriteLine($"[RelativeResourcePathConverter] ReadAllBytesShared returned null for '{absolutePath}'");
                    return Binding.DoNothing;
                }

                using var memoryStream = new MemoryStream(imageData);
                memoryStream.Position = 0;
                var decoder = BitmapDecoder.Create(
                    memoryStream,
                    BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache,
                    BitmapCacheOption.OnLoad);

                var frame = decoder.Frames.FirstOrDefault();
                if (frame == null)
                {
                    Debug.WriteLine($"[RelativeResourcePathConverter] Decoder produced no frames for '{absolutePath}'");
                    return Binding.DoNothing;
                }

                if (frame.CanFreeze)
                {
                    frame.Freeze();
                }

                Debug.WriteLine($"[RelativeResourcePathConverter] Loaded '{absolutePath}' ({frame.PixelWidth}x{frame.PixelHeight})");
                return frame;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] Failed to load '{absolutePath}': {ex}");
                return Binding.DoNothing;
            }
        }

        private static string ResolveAbsolutePath(string relativePath, string projectDir)
        {
            try
            {
                return ResourcePathHelper.GetAbsolutePath(relativePath, projectDir);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] ResolveAbsolutePath fallback for '{relativePath}': {ex.Message}");
                if (Path.IsPathRooted(relativePath))
                {
                    return relativePath;
                }
                return string.Empty;
            }
        }

        private static byte[]? ReadAllBytesShared(string path)
        {
            try
            {
                using var fileStream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                using var memoryStream = new MemoryStream();
                fileStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RelativeResourcePathConverter] ReadAllBytesShared failed for '{path}': {ex}");
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Shows breadcrumb separator arrows for all items except the last.
    /// </summary>
    public class BreadcrumbSeparatorVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Visibility.Collapsed;

            if (values[0] is int index && values[1] is int count)
            {
                return index < count - 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for enum to boolean (for radio buttons)
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string checkValue = value.ToString() ?? "";
            string targetValue = parameter.ToString() ?? "";
            return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            if ((bool)value)
            {
                try
                {
                    return Enum.Parse(targetType, parameter.ToString() ?? "");
                }
                catch
                {
                    return Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converter for ExperienceLevel enum to display string
    /// </summary>
    public class ExperienceLevelToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ExperienceLevel level)
            {
                return level switch
                {
                    Models.ExperienceLevel.NoCodingExperience => "No coding experience",
                    Models.ExperienceLevel.SomeCoding => "Some coding experience",
                    Models.ExperienceLevel.ExperiencedCoder => "Experienced coder",
                    _ => level.ToString()
                };
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
