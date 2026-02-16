using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.Services
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }
        void Load();
        void Save();
        void Reset();
    }
}
