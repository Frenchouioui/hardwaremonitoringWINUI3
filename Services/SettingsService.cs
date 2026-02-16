using System;
using System.IO;
using System.Text.Json;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3.Services
{
    public class SettingsService : ISettingsService
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HardwareMonitorWinUI3");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private AppSettings _settings = new();

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            Load();
        }

        private void ValidateSettings()
        {
            if (_settings.RefreshInterval < 100 || _settings.RefreshInterval > 5000)
                _settings.RefreshInterval = 250;

            if (_settings.WindowWidth < 400)
                _settings.WindowWidth = 1200;
            if (_settings.WindowHeight < 300)
                _settings.WindowHeight = 800;

            if (_settings.BackdropStyle < 0 || _settings.BackdropStyle > 2)
                _settings.BackdropStyle = 2;
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

                    if (settings != null)
                    {
                        _settings = settings;
                        ValidateSettings();
                        Logger.LogInfo($"Settings loaded from {SettingsFilePath}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load settings, using defaults", ex);
            }

            _settings = new AppSettings();
            Logger.LogInfo("Using default settings");
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);

                var json = JsonSerializer.Serialize(_settings, JsonOptions);
                File.WriteAllText(SettingsFilePath, json);

                Logger.LogInfo($"Settings saved to {SettingsFilePath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save settings", ex);
            }
        }

        public void Reset()
        {
            _settings = new AppSettings();
            Save();
            Logger.LogInfo("Settings reset to defaults");
        }
    }
}
