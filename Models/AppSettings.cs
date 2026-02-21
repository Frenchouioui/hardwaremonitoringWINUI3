using System.Threading;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HardwareMonitorWinUI3.Models
{
    public enum BackdropStyle
    {
        Acrylic = 0,
        Mica = 1,
        MicaAlt = 2
    }

    public enum TemperatureUnit
    {
        Celsius = 0,
        Fahrenheit = 1
    }

    public class ThreadSafeStringSet
    {
        private readonly HashSet<string> _set = new();
        private readonly object _lock = new();

        public bool Contains(string item)
        {
            lock (_lock)
            {
                return _set.Contains(item);
            }
        }

        public void Add(string item)
        {
            lock (_lock)
            {
                _set.Add(item);
            }
        }

        public bool Remove(string item)
        {
            lock (_lock)
            {
                return _set.Remove(item);
            }
        }

        public List<string> ToList()
        {
            lock (_lock)
            {
                return new List<string>(_set);
            }
        }

        public void InitializeFromList(List<string>? items)
        {
            if (items == null) return;
            lock (_lock)
            {
                _set.Clear();
                foreach (var item in items)
                {
                    _set.Add(item);
                }
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _set.Count;
                }
            }
        }
    }

    public class AppSettings
    {
        private readonly object _lock = new();
        private int _windowX = -1;
        private int _windowY = -1;
        private int _windowWidth = 1200;
        private int _windowHeight = 800;
        private int _isMaximized;
        private int _backdropStyle = (int)BackdropStyle.MicaAlt;
        private int _refreshInterval = 250;
        private int _showCPU = 1;
        private int _showGPU = 1;
        private int _showMotherboard = 1;
        private int _showStorage = 1;
        private int _showMemory = 1;
        private int _showNetwork = 1;
        private int _showController = 1;
        private int _viewMode = (int)ViewMode.Cards;
        private int _temperatureUnit;
        private int _showBattery = 1;
        private int _showPsu = 1;

        private readonly ThreadSafeStringSet _collapsedHardwareNodes = new();
        private readonly ThreadSafeStringSet _collapsedSensorGroups = new();

        public int WindowX { get => _windowX; set => _windowX = value; }
        public int WindowY { get => _windowY; set => _windowY = value; }
        public int WindowWidth { get => _windowWidth; set => _windowWidth = value; }
        public int WindowHeight { get => _windowHeight; set => _windowHeight = value; }
        public bool IsMaximized { get => Interlocked.CompareExchange(ref _isMaximized, 0, 0) == 1; set => Interlocked.Exchange(ref _isMaximized, value ? 1 : 0); }

        public BackdropStyle BackdropStyle
        {
            get => (BackdropStyle)Interlocked.CompareExchange(ref _backdropStyle, 0, 0);
            set => Interlocked.Exchange(ref _backdropStyle, (int)value);
        }

        public TemperatureUnit TemperatureUnit
        {
            get => (TemperatureUnit)Interlocked.CompareExchange(ref _temperatureUnit, 0, 0);
            set => Interlocked.Exchange(ref _temperatureUnit, (int)value);
        }

        public int RefreshInterval
        {
            get => Interlocked.CompareExchange(ref _refreshInterval, 0, 0);
            set => Interlocked.Exchange(ref _refreshInterval, value);
        }

        public bool ShowCPU { get => Interlocked.CompareExchange(ref _showCPU, 0, 0) == 1; set => Interlocked.Exchange(ref _showCPU, value ? 1 : 0); }
        public bool ShowGPU { get => Interlocked.CompareExchange(ref _showGPU, 0, 0) == 1; set => Interlocked.Exchange(ref _showGPU, value ? 1 : 0); }
        public bool ShowMotherboard { get => Interlocked.CompareExchange(ref _showMotherboard, 0, 0) == 1; set => Interlocked.Exchange(ref _showMotherboard, value ? 1 : 0); }
        public bool ShowStorage { get => Interlocked.CompareExchange(ref _showStorage, 0, 0) == 1; set => Interlocked.Exchange(ref _showStorage, value ? 1 : 0); }
        public bool ShowMemory { get => Interlocked.CompareExchange(ref _showMemory, 0, 0) == 1; set => Interlocked.Exchange(ref _showMemory, value ? 1 : 0); }
        public bool ShowNetwork { get => Interlocked.CompareExchange(ref _showNetwork, 0, 0) == 1; set => Interlocked.Exchange(ref _showNetwork, value ? 1 : 0); }
        public bool ShowController { get => Interlocked.CompareExchange(ref _showController, 0, 0) == 1; set => Interlocked.Exchange(ref _showController, value ? 1 : 0); }
        public bool ShowBattery { get => Interlocked.CompareExchange(ref _showBattery, 0, 0) == 1; set => Interlocked.Exchange(ref _showBattery, value ? 1 : 0); }
        public bool ShowPsu { get => Interlocked.CompareExchange(ref _showPsu, 0, 0) == 1; set => Interlocked.Exchange(ref _showPsu, value ? 1 : 0); }

        [JsonIgnore]
        public ThreadSafeStringSet CollapsedHardwareNodes => _collapsedHardwareNodes;
        [JsonIgnore]
        public ThreadSafeStringSet CollapsedSensorGroups => _collapsedSensorGroups;

        [System.Text.Json.Serialization.JsonPropertyName("collapsedHardwareNodes")]
        public List<string> CollapsedHardwareNodesJson
        {
            get => _collapsedHardwareNodes.ToList();
            set => _collapsedHardwareNodes.InitializeFromList(value);
        }

        [System.Text.Json.Serialization.JsonPropertyName("collapsedSensorGroups")]
        public List<string> CollapsedSensorGroupsJson
        {
            get => _collapsedSensorGroups.ToList();
            set => _collapsedSensorGroups.InitializeFromList(value);
        }

        public ViewMode ViewMode
        {
            get => (ViewMode)Interlocked.CompareExchange(ref _viewMode, 0, 0);
            set => Interlocked.Exchange(ref _viewMode, (int)value);
        }

        public AppSettings CreateSnapshot()
        {
            lock (_lock)
            {
                var snapshot = new AppSettings
                {
                    WindowX = _windowX,
                    WindowY = _windowY,
                    WindowWidth = _windowWidth,
                    WindowHeight = _windowHeight,
                    IsMaximized = _isMaximized == 1,
                    BackdropStyle = (BackdropStyle)_backdropStyle,
                    RefreshInterval = _refreshInterval,
                    ShowCPU = _showCPU == 1,
                    ShowGPU = _showGPU == 1,
                    ShowMotherboard = _showMotherboard == 1,
                    ShowStorage = _showStorage == 1,
                    ShowMemory = _showMemory == 1,
                    ShowNetwork = _showNetwork == 1,
                    ShowController = _showController == 1,
                    ViewMode = (ViewMode)_viewMode,
                    TemperatureUnit = (TemperatureUnit)_temperatureUnit,
                    ShowBattery = _showBattery == 1,
                    ShowPsu = _showPsu == 1
                };
                snapshot._collapsedHardwareNodes.InitializeFromList(_collapsedHardwareNodes.ToList());
                snapshot._collapsedSensorGroups.InitializeFromList(_collapsedSensorGroups.ToList());
                return snapshot;
            }
        }
    }
}
