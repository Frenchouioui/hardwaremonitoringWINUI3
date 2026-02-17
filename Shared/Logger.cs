using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HardwareMonitorWinUI3.Shared
{
    public static class Logger
    {
        private static readonly string LogDirectory;
        private static readonly string LogFilePath;
        private static readonly object Lock = new();
        private static StreamWriter? _writer;
        private static DateTime _currentLogDate;
        private const long MaxLogFileSize = 10 * 1024 * 1024;
        private static bool _cleanupRunning;

        static Logger()
        {
            LogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HardwareMonitorWinUI3", "Logs");

            try
            {
                Directory.CreateDirectory(LogDirectory);
            }
            catch (IOException)
            {
                LogDirectory = Path.GetTempPath();
            }
            catch (UnauthorizedAccessException)
            {
                LogDirectory = Path.GetTempPath();
            }

            LogFilePath = Path.Combine(LogDirectory, $"monitor_{DateTime.Now:yyyy-MM-dd}.log");
            _currentLogDate = DateTime.Today;
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
            Write("CRITICAL", $"{context}: {exception}");
            if (exception.StackTrace != null)
                Write("CRITICAL", $"  Stack: {exception.StackTrace}");
        }

        public static void Close()
        {
            lock (Lock)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }

        private static void Write(string level, string? message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message ?? "(null)"}";
            Trace.WriteLine(line);

            try
            {
                lock (Lock)
                {
                    CheckLogRotation();
                    EnsureWriter();
                    _writer?.WriteLine(line);
                    _writer?.Flush();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Logger] Write failed: {ex.Message}");
            }
        }

        private static void EnsureWriter()
        {
            if (_writer == null || _currentLogDate != DateTime.Today)
            {
                _writer?.Dispose();
                string actualPath = Path.Combine(LogDirectory, $"monitor_{DateTime.Now:yyyy-MM-dd}.log");
                _writer = new StreamWriter(actualPath, append: true);
                _currentLogDate = DateTime.Today;
            }
        }

        private static void CheckLogRotation()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    var fileInfo = new FileInfo(LogFilePath);
                    if (fileInfo.Length > MaxLogFileSize)
                    {
                        _writer?.Dispose();
                        _writer = null;
                        
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string backupPath = Path.Combine(LogDirectory, $"monitor_{timestamp}.log.bak");
                        File.Move(LogFilePath, backupPath);
                        
                        ScheduleCleanupOldLogs();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Logger] CheckLogRotation failed: {ex.Message}");
            }
        }

        private static void ScheduleCleanupOldLogs()
        {
            if (_cleanupRunning) return;
            
            lock (Lock)
            {
                if (_cleanupRunning) return;
                _cleanupRunning = true;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await CleanupOldLogsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Logger] CleanupOldLogsAsync failed: {ex.Message}");
                }
                finally
                {
                    _cleanupRunning = false;
                }
            });
        }

        private static async Task CleanupOldLogsAsync()
        {
            var logFiles = (await Task.Run(() => Directory.GetFiles(LogDirectory, "monitor_*.log*")).ConfigureAwait(false))
                .OrderByDescending(f => f)
                .Skip(7);

            foreach (var file in logFiles)
            {
                try
                {
                    await Task.Run(() => File.Delete(file)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Logger] Failed to delete old log {file}: {ex.Message}");
                }
            }
        }
    }
}
