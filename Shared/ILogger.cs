using System;

namespace HardwareMonitorWinUI3.Shared
{
    public interface ILogger
    {
        void LogInfo(string? message);
        void LogSuccess(string? message);
        void LogWarning(string? message);
        void LogError(string? message, Exception? exception = null);
        void LogCriticalError(string context, Exception exception);
        void Close();
    }
}
