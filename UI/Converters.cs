using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
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
                return boolValue ? "\uE70D" : "\uE70E"; // Down arrow : Right arrow
            }
            return "\uE70E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value?.ToString() == "\uE70D";
        }
    }
} 