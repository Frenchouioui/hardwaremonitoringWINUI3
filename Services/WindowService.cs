using System;
using HardwareMonitorWinUI3.Shared;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;

namespace HardwareMonitorWinUI3.Services
{
    public class WindowService : IWindowService
    {
        private readonly ISettingsService _settingsService;

        public WindowService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public void RestoreWindowState(Window window)
        {
            if (window == null) return;

            try
            {
                var appWindow = GetAppWindow(window);
                if (appWindow == null) return;

                var settings = _settingsService.Settings;

                if (settings.WindowX >= 0 && settings.WindowY >= 0 &&
                    IsPositionOnScreen(settings.WindowX, settings.WindowY))
                {
                    appWindow.Move(new PointInt32(settings.WindowX, settings.WindowY));
                    appWindow.Resize(new SizeInt32(settings.WindowWidth, settings.WindowHeight));
                }
                else
                {
                    CenterWindow(window);
                }

                if (settings.IsMaximized)
                {
                    (appWindow.Presenter as OverlappedPresenter)?.Maximize();
                }

                Logger.LogInfo("Window state restored");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to restore window state", ex);
                CenterWindow(window);
            }
        }

        public void SaveWindowState(Window window)
        {
            if (window == null) return;

            try
            {
                var appWindow = GetAppWindow(window);
                if (appWindow == null) return;

                var settings = _settingsService.Settings;
                var presenter = appWindow.Presenter as OverlappedPresenter;

                if (presenter?.State != OverlappedPresenterState.Maximized)
                {
                    settings.WindowX = appWindow.Position.X;
                    settings.WindowY = appWindow.Position.Y;
                    settings.WindowWidth = appWindow.Size.Width;
                    settings.WindowHeight = appWindow.Size.Height;
                }

                settings.IsMaximized = presenter?.State == OverlappedPresenterState.Maximized;

                _settingsService.Save();
                Logger.LogInfo("Window state saved");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save window state", ex);
            }
        }

        public void CenterWindow(Window window)
        {
            if (window == null) return;

            try
            {
                var appWindow = GetAppWindow(window);
                if (appWindow == null) return;

                var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                if (displayArea == null) return;

                var workArea = displayArea.WorkArea;

                int centerX = workArea.X + (workArea.Width - appWindow.Size.Width) / 2;
                int centerY = workArea.Y + (workArea.Height - appWindow.Size.Height) / 2;

                appWindow.Move(new PointInt32(centerX, centerY));
                Logger.LogInfo("Window centered on screen");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to center window", ex);
            }
        }

        internal static AppWindow? GetAppWindow(Window window)
        {
            try
            {
                var hWnd = WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                return AppWindow.GetFromWindowId(windowId);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static bool IsPositionOnScreen(int x, int y)
        {
            try
            {
                foreach (var displayArea in DisplayArea.FindAll())
                {
                    var workArea = displayArea.WorkArea;
                    if (x >= workArea.X && x < workArea.X + workArea.Width &&
                        y >= workArea.Y && y < workArea.Y + workArea.Height)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
