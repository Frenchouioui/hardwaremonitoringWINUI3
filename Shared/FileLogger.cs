using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HardwareMonitorWinUI3.Shared
{
    public sealed class FileLogger : ILogger, IDisposable
    {
        private readonly string _logDirectory;
        private readonly object _lock = new();
        private StreamWriter? _writer;
        private DateTime _currentLogDate;
        private const long MaxLogFileSize = 10 * 1024 * 1024;
        private bool _disposed;
        private int _isProcessing;
        
        private readonly Channel<string> _channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        public FileLogger()
        {
            _logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HardwareMonitorWinUI3", "Logs");

            try
            {
                Directory.CreateDirectory(_logDirectory);
            }
            catch (IOException)
            {
                _logDirectory = Path.GetTempPath();
            }
            catch (UnauthorizedAccessException)
            {
                _logDirectory = Path.GetTempPath();
            }

            _currentLogDate = DateTime.Today;
            StartBackgroundWriter();
        }

        private void StartBackgroundWriter()
        {
            if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
            {
                _ = ProcessLogQueueAsync();
            }
        }

        private async Task ProcessLogQueueAsync()
        {
            try
            {
                await foreach (var line in _channel.Reader.ReadAllAsync())
                {
                    try
                    {
                        lock (_lock)
                        {
                            CheckLogRotation();
                            EnsureWriter();
                            _writer?.WriteLine(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[Logger] Write failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Logger] Background writer crashed: {ex.Message}");
            }
        }

        public void LogInfo(string? message) => Write("INFO", message);
        public void LogSuccess(string? message) => Write("OK", message);
        public void LogWarning(string? message) => Write("WARN", message);

        public void LogError(string? message, Exception? exception = null)
        {
            Write("ERROR", message);
            if (exception != null)
            {
                Write("ERROR", $"  Type: {exception.GetType().Name}");
                if (!string.IsNullOrEmpty(exception.StackTrace))
                    Write("ERROR", $"  Stack: {exception.StackTrace}");
            }
        }

        public void LogCriticalError(string context, Exception exception)
        {
            Write("CRITICAL", $"{context}: {exception}");
            if (exception.StackTrace != null)
                Write("CRITICAL", $"  Stack: {exception.StackTrace}");
        }

        public void Close()
        {
            _channel.Writer.Complete();
            
            lock (_lock)
            {
                _writer?.Dispose();
                _writer = null;
            }
        }

        private void Write(string level, string? message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message ?? "(null)"}";
            Trace.WriteLine(line);
            _channel.Writer.TryWrite(line);
        }

        private void EnsureWriter()
        {
            if (_writer == null || _currentLogDate != DateTime.Today)
            {
                _writer?.Dispose();
                string actualPath = Path.Combine(_logDirectory, $"monitor_{DateTime.Now:yyyy-MM-dd}.log");
                _writer = new StreamWriter(actualPath, append: true) { AutoFlush = true };
                _currentLogDate = DateTime.Today;
            }
        }

        private void CheckLogRotation()
        {
            try
            {
                string currentPath = Path.Combine(_logDirectory, $"monitor_{DateTime.Now:yyyy-MM-dd}.log");
                if (File.Exists(currentPath))
                {
                    var fileInfo = new FileInfo(currentPath);
                    if (fileInfo.Length > MaxLogFileSize)
                    {
                        _writer?.Dispose();
                        _writer = null;

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string backupPath = Path.Combine(_logDirectory, $"monitor_{timestamp}.log.bak");
                        File.Move(currentPath, backupPath);

                        _ = CleanupOldLogsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Logger] CheckLogRotation failed: {ex.Message}");
            }
        }

        private async Task CleanupOldLogsAsync()
        {
            try
            {
                var logFiles = (await Task.Run(() => Directory.GetFiles(_logDirectory, "monitor_*.log*")))
                    .OrderByDescending(f => f)
                    .Skip(7);

                foreach (var file in logFiles)
                {
                    try
                    {
                        await Task.Run(() => File.Delete(file));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[Logger] Failed to delete old log {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Logger] CleanupOldLogsAsync failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Close();
        }
    }
}
