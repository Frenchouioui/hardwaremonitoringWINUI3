using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Shared;
using System;

namespace HardwareMonitorWinUI3.UI
{
    public static class UIExtensions
    {
        #region Constants

        private const int TitleBarHeight = 32;
        private const int TitleBarDragWidth = 5000;

        private const byte ButtonHoverOpacity = 20;
        private const byte ButtonPressedOpacity = 30;
        private const byte FullOpacity = 255;

        #endregion

        #region Sensor Formatting

        public static string GetSensorUnit(this SensorType type) => type switch
        {
            SensorType.Temperature => "\u00b0C",
            SensorType.Clock => "MHz",
            SensorType.Voltage => "V",
            SensorType.Current => "A",
            SensorType.Power => "W",
            SensorType.Data => "GB",
            SensorType.SmallData => "MB",
            SensorType.Load => "%",
            SensorType.Fan => "RPM",
            SensorType.Flow => "L/h",
            SensorType.Control => "%",
            SensorType.Level => "%",
            SensorType.Factor => "",
            SensorType.Frequency => "Hz",
            SensorType.Throughput => "MB/s",
            _ => ""
        };

        public static string GetSensorPrecision(this SensorType type) => type switch
        {
            SensorType.Temperature => "F1",
            SensorType.Clock => "F0",
            SensorType.Voltage => "F3",
            SensorType.Current => "F2",
            SensorType.Power => "F1",
            SensorType.Data => "F1",
            SensorType.SmallData => "F1",
            SensorType.Load => "F1",
            SensorType.Fan => "F0",
            SensorType.Flow => "F1",
            SensorType.Control => "F1",
            SensorType.Level => "F1",
            SensorType.Frequency => "F0",
            SensorType.Throughput => "F0",
            _ => "F1"
        };

        public static string GetSensorIcon(this SensorType type) => type switch
        {
            SensorType.Temperature => "\uE9CA",
            SensorType.Clock => "\uE823",
            SensorType.Voltage => "\uE945",
            SensorType.Current => "\uE945",
            SensorType.Power => "\uE83E",
            SensorType.Data => "\uE8B7",
            SensorType.SmallData => "\uE8B7",
            SensorType.Load => "\uE9D9",
            SensorType.Fan => "\uE71E",
            SensorType.Flow => "\uE81E",
            SensorType.Control => "\uE713",
            SensorType.Level => "\uE9D9",
            SensorType.Frequency => "\uE823",
            SensorType.Throughput => "\uE8AB",
            _ => "\uE950"
        };

        public static string ToFormattedString(this ISensor sensor)
        {
            if (sensor is null || !sensor.Value.HasValue)
                return "N/A";
            
            return $"{sensor.Value!.Value.ToString(sensor.SensorType.GetSensorPrecision())}{sensor.SensorType.GetSensorUnit()}";
        }

        public static SensorData CreateSensorData(this ISensor sensor)
        {
            if (sensor == null)
            {
                return new SensorData
                {
                    Name = "Unknown Sensor",
                    Icon = SensorType.Load.GetSensorIcon(),
                    Value = "N/A"
                };
            }

            var sensorData = new SensorData
            {
                Name = sensor.Name ?? "Unknown Sensor",
                Icon = sensor.SensorType.GetSensorIcon()
            };

            sensorData.Value = sensor.ToFormattedString();

            if (sensor.Value.HasValue)
            {
                string unit = sensor.SensorType.GetSensorUnit();
                string precision = sensor.SensorType.GetSensorPrecision();
                sensorData.UpdateMinMax(sensor.Value.Value, unit, precision);
            }

            return sensorData;
        }

        #endregion

        #region Backdrop Extensions

        public static void SetMicaBackdrop(this Window window, MicaKind kind)
        {
            window.SystemBackdrop = new MicaBackdrop { Kind = kind };
        }

        public static void SetAcrylicBackdrop(this Window window)
        {
            window.SystemBackdrop = new DesktopAcrylicBackdrop();
        }

        public static string GetBackdropDisplayName(BackdropStyle style) => style switch
        {
            BackdropStyle.Acrylic => "\u2022 Acrylic",
            BackdropStyle.Mica => "\u2022 Mica",
            BackdropStyle.MicaAlt => "\u2022 Mica Alt",
            _ => "\u2022 Mica Alt"
        };

        public static void ApplySelectedBackdrop(this Window window, BackdropStyle style)
        {
            switch (style)
            {
                case BackdropStyle.Acrylic:
                    window.SetAcrylicBackdrop();
                    break;
                case BackdropStyle.Mica:
                    window.SetMicaBackdrop(MicaKind.Base);
                    break;
                case BackdropStyle.MicaAlt:
                default:
                    window.SetMicaBackdrop(MicaKind.BaseAlt);
                    break;
            }
        }

        #endregion

        #region TitleBar Extensions

        public static void SetupModernTitleBar(this Window window)
        {
            var appWindow = Services.WindowService.GetAppWindow(window);
            if (appWindow != null)
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                appWindow.TitleBar.ButtonHoverBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(ButtonHoverOpacity, FullOpacity, FullOpacity, FullOpacity);
                appWindow.TitleBar.ButtonPressedBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(ButtonPressedOpacity, FullOpacity, FullOpacity, FullOpacity);

                appWindow.TitleBar.SetDragRectangles(new[] { new Windows.Graphics.RectInt32 { X = 0, Y = 0, Width = TitleBarDragWidth, Height = TitleBarHeight } });
            }
        }

        #endregion

        #region Error Dialog

        public static void ShowCriticalErrorDialog(Exception ex, Microsoft.UI.Xaml.XamlRoot? xamlRoot)
        {
            if (xamlRoot == null)
            {
                Logger.LogCriticalError("Cannot show error dialog - XamlRoot is null", ex);
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Critical Error",
                Content = $"The application could not initialize properly:\n\n{ex.Message}\n\nType: {ex.GetType().Name}",
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };

            _ = dialog.ShowAsync();
        }

        #endregion
    }
}
