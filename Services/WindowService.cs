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
        private readonly ILogger _logger;

        public WindowService(ISettingsService settingsService, ILogger logger)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                _logger.LogInfo("Window state restored");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to restore window state", ex);
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
                _logger.LogInfo("Window state saved");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save window state", ex);
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
                var adaptiveSize = CalculateAdaptiveSize(workArea);

                appWindow.Resize(adaptiveSize);

                int centerX = workArea.X + (workArea.Width - adaptiveSize.Width) / 2;
                int centerY = workArea.Y + (workArea.Height - adaptiveSize.Height) / 2;

                appWindow.Move(new PointInt32(centerX, centerY));
                _logger.LogInfo("Window centered on screen with adaptive size");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to center window", ex);
            }
        }

        public SizeInt32 CalculateAdaptiveSize(RectInt32 workArea)
        {
            int width = (int)(workArea.Width * 0.8);
            int height = (int)(workArea.Height * 0.8);
            
            const int maxWidth = 1600;
            const int maxHeight = 1000;
            const int minWidth = 800;
            const int minHeight = 600;
            
            width = Math.Clamp(width, minWidth, maxWidth);
            height = Math.Clamp(height, minHeight, maxHeight);
            
            return new SizeInt32(width, height);
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

        private bool IsPositionOnScreen(int x, int y)
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
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to check screen position: {ex.Message}");
                return true;
            }
        }
    }
}
