using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HardwareMonitorWinUI3.UI
{
    public static class UIExtensions
    {
        #region Constants

        private const double ICON_FONT_SIZE = 12.0;
        private const double TEXT_FONT_SIZE = 12.0;
        private const double SMALL_TEXT_FONT_SIZE = 11.0;

        private const int TITLE_BAR_HEIGHT = 32;
        private const int TITLE_BAR_DRAG_WIDTH = 10000;

        private const byte BUTTON_HOVER_OPACITY = 20;
        private const byte BUTTON_PRESSED_OPACITY = 30;
        private const byte FULL_OPACITY = 255;

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
            SensorType.Temperature => "\U0001f321\ufe0f",
            SensorType.Clock => "\u26a1",
            SensorType.Voltage => "\U0001f50b",
            SensorType.Current => "\u26a1",
            SensorType.Power => "\U0001f4a1",
            SensorType.Data => "\U0001f4be",
            SensorType.SmallData => "\U0001f4be",
            SensorType.Load => "\U0001f4ca",
            SensorType.Fan => "\U0001f300",
            SensorType.Flow => "\U0001f4a7",
            SensorType.Control => "\U0001f39b\ufe0f",
            SensorType.Level => "\U0001f4cf",
            SensorType.Frequency => "\U0001f4f6",
            SensorType.Throughput => "\U0001f504",
            _ => "\U0001f4ca"
        };

        public static string ToFormattedString(this ISensor sensor)
        {
            return sensor?.Value.HasValue != true
                ? "N/A"
                : $"{sensor.Value!.Value.ToString(sensor.SensorType.GetSensorPrecision())}{sensor.SensorType.GetSensorUnit()}";
        }

        public static SensorData CreateSensorData(this ISensor sensor)
        {
            if (sensor == null)
            {
                return new SensorData()
                {
                    Name = "Unknown Sensor",
                    Icon = SensorType.Load.GetSensorIcon(),
                    Value = "N/A"
                };
            }

            var sensorData = new SensorData()
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
            window.SystemBackdrop = new MicaBackdrop() { Kind = kind };
        }

        public static void SetAcrylicBackdrop(this Window window)
        {
            window.SystemBackdrop = new DesktopAcrylicBackdrop();
        }

        public static string GetBackdropDisplayName(int selectedIndex) => selectedIndex switch
        {
            0 => "\u2022 Acrylic",
            1 => "\u2022 Mica",
            2 => "\u2022 Mica Alt",
            _ => "\u2022 Mica Alt"
        };

        public static void ApplySelectedBackdrop(this Window window, int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0:
                    window.SetAcrylicBackdrop();
                    break;
                case 1:
                    window.SetMicaBackdrop(MicaKind.Base);
                    break;
                case 2:
                    window.SetMicaBackdrop(MicaKind.BaseAlt);
                    break;
            }
        }

        public static List<ComboBoxItem> GetBackdropOptions()
        {
            return new List<ComboBoxItem>
            {
                new ComboBoxItem
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            new FontIcon { Glyph = "\uE91C", FontSize = ICON_FONT_SIZE },
                            new TextBlock { Text = "Acrylic", FontSize = TEXT_FONT_SIZE }
                        }
                    }
                },
                new ComboBoxItem
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            new FontIcon { Glyph = "\uE7E8", FontSize = ICON_FONT_SIZE },
                            new TextBlock { Text = "Mica", FontSize = TEXT_FONT_SIZE }
                        }
                    }
                },
                new ComboBoxItem
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            new FontIcon { Glyph = "\uE7E8", FontSize = ICON_FONT_SIZE },
                            new TextBlock { Text = "Mica Alt", FontSize = TEXT_FONT_SIZE }
                        }
                    }
                }
            };
        }

        #endregion

        #region TitleBar Extensions

        public static void SetupModernTitleBar(this Window window)
        {
            var appWindow = window.GetAppWindow();
            if (appWindow != null)
            {
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                appWindow.TitleBar.ButtonHoverBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(BUTTON_HOVER_OPACITY, FULL_OPACITY, FULL_OPACITY, FULL_OPACITY);
                appWindow.TitleBar.ButtonPressedBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(BUTTON_PRESSED_OPACITY, FULL_OPACITY, FULL_OPACITY, FULL_OPACITY);

                appWindow.TitleBar.SetDragRectangles(new[] { new Windows.Graphics.RectInt32() { X = 0, Y = 0, Width = TITLE_BAR_DRAG_WIDTH, Height = TITLE_BAR_HEIGHT } });
            }
        }

        private static AppWindow? GetAppWindow(this Window window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }

        #endregion

        #region UI Setup

        public static void SetupModernInterface(this Window window, ComboBox backdropSelector,
            FontIcon hardwareIcon, Button ultraButton, Button rapideButton,
            Button normalButton, Button resetButton, Button hardwareDiagButton)
        {
            window.Title = UIConstants.ApplicationTitle;

            backdropSelector.InitializeBackdropSelector();

            hardwareIcon.Glyph = UIConstants.HardwareIcon;

            ultraButton.Content = CreateButtonContent("", "ULTRA");
            rapideButton.Content = CreateButtonContent("", "RAPIDE");
            normalButton.Content = CreateButtonContent("", "NORMAL");
            resetButton.Content = CreateButtonContent(UIConstants.ResetIcon, "RESET");
            hardwareDiagButton.Content = CreateButtonContent(UIConstants.DiagnosticIcon, "DIAG");
        }

        private static void InitializeBackdropSelector(this ComboBox backdropSelector)
        {
            var backdropOptions = GetBackdropOptions();
            backdropSelector.Items.Clear();
            foreach (var option in backdropOptions)
            {
                backdropSelector.Items.Add(option);
            }

            backdropSelector.SelectedIndex = UIConstants.DefaultBackdropIndex;
        }

        private static StackPanel CreateButtonContent(string glyph, string text)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };

            if (!string.IsNullOrEmpty(glyph))
            {
                stackPanel.Children.Add(new FontIcon
                {
                    Glyph = glyph,
                    FontSize = ICON_FONT_SIZE,
                    Margin = new Thickness(0, 0, 4, 0)
                });
            }

            stackPanel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = SMALL_TEXT_FONT_SIZE,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            return stackPanel;
        }

        public static void UpdateSpeedButtonVisualState(Button ultraButton, Button rapideButton,
            Button normalButton, string activeSpeedButton)
        {
            ultraButton.Style = Application.Current.Resources["SpeedButtonInactiveStyle"] as Style;
            rapideButton.Style = Application.Current.Resources["SpeedButtonInactiveStyle"] as Style;
            normalButton.Style = Application.Current.Resources["SpeedButtonInactiveStyle"] as Style;

            switch (activeSpeedButton)
            {
                case "Ultra":
                    ultraButton.Style = Application.Current.Resources["SpeedButtonActiveStyle"] as Style;
                    break;
                case "Rapide":
                    rapideButton.Style = Application.Current.Resources["SpeedButtonActiveStyle"] as Style;
                    break;
                case "Normal":
                    normalButton.Style = Application.Current.Resources["SpeedButtonActiveStyle"] as Style;
                    break;
            }
        }

        public static void ShowCriticalErrorDialog(Exception ex, Microsoft.UI.Xaml.XamlRoot? xamlRoot)
        {
            var dialog = new ContentDialog
            {
                Title = "Erreur Critique",
                Content = $"L'application n'a pas pu s'initialiser correctement:\n\n{ex.Message}\n\nType: {ex.GetType().Name}",
                CloseButtonText = "OK"
            };

            if (xamlRoot != null)
            {
                dialog.XamlRoot = xamlRoot;
            }

            // Fire-and-forget is acceptable here: we're showing an error dialog
            // and the app is already in a failed state
            _ = dialog.ShowAsync();
        }

        #endregion
    }
}
