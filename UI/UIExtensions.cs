using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using WinRT.Interop;
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

        public static async System.Threading.Tasks.Task ShowCriticalErrorDialog(Exception ex, Microsoft.UI.Xaml.XamlRoot? xamlRoot, ILogger? logger = null)
        {
            if (xamlRoot == null)
            {
                logger?.LogCriticalError("Cannot show error dialog - XamlRoot is null", ex);
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Critical Error",
                Content = $"The application could not initialize properly:\n\n{ex.Message}\n\nType: {ex.GetType().Name}",
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion
    }
}
