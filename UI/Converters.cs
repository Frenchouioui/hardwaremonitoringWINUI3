using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace HardwareMonitorWinUI3.UI
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    public class BoolToExpandIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "\uE70D" : "\uE70E";
            }
            return "\uE70E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value?.ToString() == "\uE70D";
        }
    }

    public class BoolToAccentBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush DefaultAccentBrush = new(Microsoft.UI.Colors.Transparent);
        private static readonly SolidColorBrush DefaultDividerBrush = new(Microsoft.UI.Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive && isActive)
            {
                return TryGetResource("AccentFillColorDefaultBrush", DefaultAccentBrush);
            }
            return TryGetResource("DividerStrokeColorDefaultBrush", DefaultDividerBrush);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush brush)
            {
                var accentBrush = TryGetResource("AccentFillColorDefaultBrush", DefaultAccentBrush) as SolidColorBrush;
                if (accentBrush != null && brush.Color.Equals(accentBrush.Color))
                {
                    return true;
                }
            }
            return false;
        }

        private static object TryGetResource(string key, object defaultValue)
        {
            try
            {
                if (Application.Current?.Resources?.TryGetValue(key, out var resource) == true)
                {
                    return resource ?? defaultValue;
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to get resource: {key}");
            }
            return defaultValue;
        }
    }
}
