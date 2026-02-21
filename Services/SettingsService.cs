using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3.Services
{
    public sealed class SettingsService : ISettingsService, IDisposable
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HardwareMonitorWinUI3");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");
        private static readonly string BackupFilePath = Path.Combine(SettingsDirectory, "settings.json.bak");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private readonly ILogger _logger;
        private AppSettings _settings = new();
        private readonly SemaphoreSlim _saveLock = new(1, 1);
        private CancellationTokenSource? _saveCts;
        private bool _disposed;

        public AppSettings Settings => _settings;

        public SettingsService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Load();
        }

        private void ValidateSettings()
        {
            if (_settings.RefreshInterval is < 100 or > 5000)
                _settings.RefreshInterval = 250;

            if (_settings.WindowWidth < 400)
                _settings.WindowWidth = 1200;
            if (_settings.WindowHeight < 300)
                _settings.WindowHeight = 800;

            if (!Enum.IsDefined(typeof(BackdropStyle), _settings.BackdropStyle))
                _settings.BackdropStyle = BackdropStyle.MicaAlt;
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var fileInfo = new FileInfo(SettingsFilePath);
                    if (fileInfo.Length > 1024 * 1024)
                    {
                        _logger.LogWarning($"Settings file too large ({fileInfo.Length} bytes), using defaults");
                        _settings = new AppSettings();
                        return;
                    }

                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

                    if (settings != null)
                    {
                        _settings = settings;
                        ValidateSettings();
                        _logger.LogInfo($"Settings loaded from {SettingsFilePath}");
                        return;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning($"Invalid settings JSON, attempting backup: {ex.Message}");
                TryLoadBackup();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load settings, using defaults", ex);
            }

            _settings = new AppSettings();
            _logger.LogInfo("Using default settings");
        }

        private void TryLoadBackup()
        {
            try
            {
                if (!File.Exists(BackupFilePath)) return;

                var json = File.ReadAllText(BackupFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

                if (settings != null)
                {
                    _settings = settings;
                    ValidateSettings();
                    _logger.LogInfo("Settings restored from backup");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load backup settings", ex);
            }

            _settings = new AppSettings();
        }

        public void Save()
        {
            SaveAsync().GetAwaiter().GetResult();
        }

        public async Task SaveAsync()
        {
            await _saveLock.WaitAsync().ConfigureAwait(false);
            try
            {
                Directory.CreateDirectory(SettingsDirectory);

                if (File.Exists(SettingsFilePath))
                {
                    try
                    {
                        File.Copy(SettingsFilePath, BackupFilePath, overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to create backup: {ex.Message}");
                    }
                }

                var json = JsonSerializer.Serialize(_settings, JsonOptions);
                var tempPath = SettingsFilePath + ".tmp";

                await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);
                File.Move(tempPath, SettingsFilePath, overwrite: true);

                _logger.LogInfo($"Settings saved to {SettingsFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save settings", ex);
            }
            finally
            {
                _saveLock.Release();
            }
        }

        public void SaveThrottled(int delayMs = 500)
        {
            _saveCts?.Cancel();

            _saveCts = new CancellationTokenSource();
            var token = _saveCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs, token).ConfigureAwait(false);
                    if (!token.IsCancellationRequested)
                    {
                        await SaveAsync().ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError("Throttled save failed", ex);
                }
            }, token);
        }

        public void Reset()
        {
            _settings = new AppSettings();
            Save();
            _logger.LogInfo("Settings reset to defaults");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _saveCts?.Cancel();
            _saveCts?.Dispose();
            _saveLock.Dispose();
        }
    }
}
