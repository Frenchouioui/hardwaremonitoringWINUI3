using Microsoft.UI.Xaml;

namespace HardwareMonitorWinUI3.Services
{
    public interface IWindowService
    {
        void RestoreWindowState(Window window);
        void SaveWindowState(Window window);
        void CenterWindow(Window window);
    }
}
