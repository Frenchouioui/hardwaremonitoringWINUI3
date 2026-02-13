using System;
using System.Diagnostics;
using System.IO;

namespace HardwareMonitorWinUI3.Shared
{
    /// <summary>
    /// Logger with timestamped output to both Trace and a log file.
    /// Log files are stored in %LOCALAPPDATA%\HardwareMonitorWinUI3\Logs
    /// </summary>
    public static class Logger
    {
        private static readonly string _logFilePath;
        private static readonly object _lock = new();

        static Logger()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HardwareMonitorWinUI3", "Logs");

            try
            {
                Directory.CreateDirectory(logDir);
            }
            catch
            {
                logDir = Path.GetTempPath();
            }

            _logFilePath = Path.Combine(logDir, $"monitor_{DateTime.Now:yyyy-MM-dd}.log");
        }

        public static void LogInfo(string? message)
            => Write("INFO", message);

        public static void LogSuccess(string? message)
            => Write("OK", message);

        public static void LogWarning(string? message)
            => Write("WARN", message);

        public static void LogError(string? message, Exception? exception = null)
        {
            Write("ERROR", message);
            if (exception != null)
            {
                Write("ERROR", $"  Type: {exception.GetType().Name}");
                if (!string.IsNullOrEmpty(exception.StackTrace))
                    Write("ERROR", $"  Stack: {exception.StackTrace}");
            }
        }

        public static void LogCriticalError(string context, Exception exception)
        {
            Write("CRITICAL", $"{context}: {exception.Message}");
            if (exception.StackTrace != null)
                Write("CRITICAL", $"  Stack: {exception.StackTrace}");
        }

        private static void Write(string level, string? message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message ?? "(null)"}";
            Trace.WriteLine(line);

            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine);
                }
            }
            catch
            {
                // Never crash the app because of logging
            }
        }
    }
}
