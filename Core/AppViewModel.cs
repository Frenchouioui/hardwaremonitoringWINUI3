using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.Services;
using HardwareMonitorWinUI3.Shared;
using HardwareMonitorWinUI3.UI;

namespace HardwareMonitorWinUI3.Core
{
    public class AppViewModel : BaseViewModel
    {
        #region Constants

        private const string SpeedUltra = "Ultra";
        private const string SpeedFast = "Fast";
        private const string SpeedNormal = "Normal";

        #endregion

        #region Fields

        private readonly IHardwareService _hardwareService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private readonly Func<Action, bool> _dispatch;
        private CancellationTokenSource _cts = new();

        private string _systemStatusText = "Initializing hardware monitoring...";
        private string _upsIndicator = string.Empty;
        private string _backdropIndicator = "\u2022 Mica Alt";
        private bool _isUIBusy;

        private bool _showCPU = true;
        private bool _showGPU = true;
        private bool _showMotherboard = true;
        private bool _showStorage = true;
        private bool _showMemory = true;
        private bool _showNetwork = true;
        private bool _showController = true;
        private bool _showBattery = true;
        private bool _showPsu = true;
        private ViewMode _currentViewMode = ViewMode.Cards;
        private TemperatureUnit _temperatureUnit = TemperatureUnit.Celsius;

        private int _pendingSaveCount;
        private readonly object _saveLock = new();

        #endregion

        #region Properties

        private readonly ObservableCollection<HardwareNode> _filteredHardwareNodes;
        public ObservableCollection<HardwareNode> HardwareNodes => _filteredHardwareNodes;

        private void UpdateFilteredHardwareNodes()
        {
            HardwareNode[] nodesSnapshot;
            lock (_saveLock)
            {
                nodesSnapshot = _hardwareService.HardwareNodes.ToArray();
            }

            var desired = new HashSet<HardwareNode>(nodesSnapshot.Where(ShouldShowHardwareNode));

            bool dispatched = _dispatch(() =>
            {
                for (int i = _filteredHardwareNodes.Count - 1; i >= 0; i--)
                {
                    if (!desired.Contains(_filteredHardwareNodes[i]))
                        _filteredHardwareNodes.RemoveAt(i);
                }

                foreach (var node in nodesSnapshot)
                {
                    if (desired.Contains(node) && !_filteredHardwareNodes.Contains(node))
                        _filteredHardwareNodes.Add(node);
                }
            });

            if (!dispatched)
            {
                _logger.LogWarning("Failed to dispatch UpdateFilteredHardwareNodes");
            }
        }

        private bool ShouldShowHardwareNode(HardwareNode hardwareNode)
        {
            return hardwareNode.Category switch
            {
                HardwareCategory.Cpu => ShowCPU,
                HardwareCategory.Gpu => ShowGPU,
                HardwareCategory.Motherboard => ShowMotherboard,
                HardwareCategory.Storage => ShowStorage,
                HardwareCategory.Memory => ShowMemory,
                HardwareCategory.Network => ShowNetwork,
                HardwareCategory.Controller => ShowController,
                HardwareCategory.Battery => ShowBattery,
                HardwareCategory.Psu => ShowPsu,
                _ => true
            };
        }

        public string SystemStatusText
        {
            get => _systemStatusText;
            set => SetProperty(ref _systemStatusText, value);
        }

        public string UpsIndicator
        {
            get => _upsIndicator;
            set => SetProperty(ref _upsIndicator, value);
        }

        public string BackdropIndicator
        {
            get => _backdropIndicator;
            set => SetProperty(ref _backdropIndicator, value);
        }

        public bool IsUIBusy
        {
            get => _isUIBusy;
            set => SetProperty(ref _isUIBusy, value);
        }

        public bool ShowCPU
        {
            get => _showCPU;
            set => SetFilterProperty(ref _showCPU, value, (s, v) => s.ShowCPU = v);
        }

        public bool ShowGPU
        {
            get => _showGPU;
            set => SetFilterProperty(ref _showGPU, value, (s, v) => s.ShowGPU = v);
        }

        public bool ShowMotherboard
        {
            get => _showMotherboard;
            set => SetFilterProperty(ref _showMotherboard, value, (s, v) => s.ShowMotherboard = v);
        }

        public bool ShowStorage
        {
            get => _showStorage;
            set => SetFilterProperty(ref _showStorage, value, (s, v) => s.ShowStorage = v);
        }

        public bool ShowMemory
        {
            get => _showMemory;
            set => SetFilterProperty(ref _showMemory, value, (s, v) => s.ShowMemory = v);
        }

        public bool ShowNetwork
        {
            get => _showNetwork;
            set => SetFilterProperty(ref _showNetwork, value, (s, v) => s.ShowNetwork = v);
        }

        public bool ShowController
        {
            get => _showController;
            set => SetFilterProperty(ref _showController, value, (s, v) => s.ShowController = v);
        }

        public bool ShowBattery
        {
            get => _showBattery;
            set => SetFilterProperty(ref _showBattery, value, (s, v) => s.ShowBattery = v);
        }

        public bool ShowPsu
        {
            get => _showPsu;
            set => SetFilterProperty(ref _showPsu, value, (s, v) => s.ShowPsu = v);
        }

        public TemperatureUnit TemperatureUnit
        {
            get => _temperatureUnit;
            set
            {
                if (SetProperty(ref _temperatureUnit, value))
                {
                    _settingsService.Settings.TemperatureUnit = value;
                    SensorExtensions.CurrentTemperatureUnit = value;
                    ScheduleSave();
                    OnPropertyChanged(nameof(IsCelsius));
                    OnPropertyChanged(nameof(IsFahrenheit));
                }
            }
        }
        
        public bool IsCelsius
        {
            get => _temperatureUnit == TemperatureUnit.Celsius;
            set
            {
                if (value)
                {
                    TemperatureUnit = TemperatureUnit.Celsius;
                }
            }
        }
        
        public bool IsFahrenheit
        {
            get => _temperatureUnit == TemperatureUnit.Fahrenheit;
            set
            {
                if (value)
                {
                    TemperatureUnit = TemperatureUnit.Fahrenheit;
                }
            }
        }

        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    _settingsService.Settings.ViewMode = value;
                    ScheduleSave();
                }
            }
        }

        private bool SetFilterProperty(ref bool field, bool value, Action<AppSettings, bool> settingsSetter, [CallerMemberName] string? name = null)
        {
            if (!SetProperty(ref field, value, name)) return false;
            settingsSetter(_settingsService.Settings, value);
            ScheduleSave();
            UpdateFilteredHardwareNodes();
            return true;
        }

        private void OnExpansionStateChanged(object? sender, string key)
        {
            var settings = _settingsService.Settings;
            
            if (key.StartsWith("group:"))
            {
                var groupKey = key.Substring(6);
                var collapsed = settings.CollapsedSensorGroups;
                
                if (collapsed.Contains(groupKey))
                    collapsed.Remove(groupKey);
                else
                    collapsed.Add(groupKey);
            }
            else
            {
                var collapsed = settings.CollapsedHardwareNodes;
                
                if (collapsed.Contains(key))
                    collapsed.Remove(key);
                else
                    collapsed.Add(key);
            }
            
            ScheduleSave();
        }
        
        private void RestoreExpansionStates()
        {
            var settings = _settingsService.Settings;
            var collapsedHardware = settings.CollapsedHardwareNodes;
            var collapsedGroups = settings.CollapsedSensorGroups;

            foreach (var node in _hardwareService.HardwareNodes)
            {
                if (collapsedHardware.Contains(node.Name))
                {
                    node.IsExpanded = false;
                }

                foreach (var group in node.SensorGroups)
                {
                    var groupKey = $"{node.Name}|{group.CategoryName}";
                    if (collapsedGroups.Contains(groupKey))
                    {
                        group.IsExpanded = false;
                    }
                }

                foreach (var subNode in node.SubHardware)
                {
                    if (collapsedHardware.Contains(subNode.Name))
                    {
                        subNode.IsExpanded = false;
                    }

                    foreach (var group in subNode.SensorGroups)
                    {
                        var groupKey = $"{subNode.Name}|{group.CategoryName}";
                        if (collapsedGroups.Contains(groupKey))
                        {
                            group.IsExpanded = false;
                        }
                    }
                }
            }

            _hardwareService.ExpansionStateChanged += OnExpansionStateChanged;
        }

        public bool IsInitialized => _hardwareService.IsInitialized;
        public int CurrentInterval => _hardwareService.CurrentInterval;

        public int UltraInterval => UIConstants.UltraInterval;
        public int FastInterval => UIConstants.FastInterval;
        public int NormalInterval => UIConstants.NormalInterval;

        public string ActiveSpeedButton => UIConstants.GetActiveSpeedButton(_settingsService.Settings.RefreshInterval);

        public bool IsUltraActive => ActiveSpeedButton == SpeedUltra;
        public bool IsFastActive => ActiveSpeedButton == SpeedFast;
        public bool IsNormalActive => ActiveSpeedButton == SpeedNormal;

        public bool IsCardsView => CurrentViewMode == ViewMode.Cards;
        public bool IsTreeView => CurrentViewMode == ViewMode.Tree;

        public bool IsBackdropAcrylic => _settingsService.Settings.BackdropStyle == BackdropStyle.Acrylic;
        public bool IsBackdropMica => _settingsService.Settings.BackdropStyle == BackdropStyle.Mica;
        public bool IsBackdropMicaAlt => _settingsService.Settings.BackdropStyle == BackdropStyle.MicaAlt;

        #endregion

        #region Commands

        public IRelayCommand<object?> ChangeSpeedCommand { get; }
        public IRelayCommand ResetMinMaxCommand { get; }
        public IRelayCommand RunDiagnosticCommand { get; }
        public IRelayCommand<object?> ChangeBackdropCommand { get; }
        public IRelayCommand<object?> SetViewModeCommand { get; }
        public IRelayCommand<object?> SetTemperatureUnitCommand { get; }

        #endregion

        #region Constructor

        public AppViewModel(
            IHardwareService hardwareService,
            ISettingsService settingsService,
            ILogger logger,
            Func<Action, bool>? dispatcher = null)
        {
            _hardwareService = hardwareService ?? throw new ArgumentNullException(nameof(hardwareService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dispatch = dispatcher ?? (action => { action(); return true; });
            _filteredHardwareNodes = new ObservableCollection<HardwareNode>();

            ChangeSpeedCommand = new RelayCommand<object?>(ExecuteChangeSpeed);
            ResetMinMaxCommand = new RelayCommand(ExecuteResetMinMax);
            RunDiagnosticCommand = new RelayCommand(ExecuteRunDiagnostic);
            ChangeBackdropCommand = new RelayCommand<object?>(ExecuteChangeBackdrop);
            SetViewModeCommand = new RelayCommand<object?>(ExecuteSetViewMode);
            SetTemperatureUnitCommand = new RelayCommand<object?>(ExecuteSetTemperatureUnit);

            _hardwareService.TimerTick += OnTimerTick;
            _hardwareService.UpsUpdated += OnUpsUpdated;
            _hardwareService.HardwareNodes.CollectionChanged += OnHardwareNodesChanged;

            LoadSettings();

            _upsIndicator = UIConstants.GetInitialUpsIndicator(_settingsService.Settings.RefreshInterval);
            _logger.LogInfo($"Configuration loaded - Default interval: {_settingsService.Settings.RefreshInterval}ms");
        }

        private void LoadSettings()
        {
            var settings = _settingsService.Settings;

            _showCPU = settings.ShowCPU;
            _showGPU = settings.ShowGPU;
            _showMotherboard = settings.ShowMotherboard;
            _showStorage = settings.ShowStorage;
            _showMemory = settings.ShowMemory;
            _showNetwork = settings.ShowNetwork;
            _showController = settings.ShowController;
            _showBattery = settings.ShowBattery;
            _showPsu = settings.ShowPsu;
            _currentViewMode = settings.ViewMode;
            _temperatureUnit = settings.TemperatureUnit;
            
            SensorExtensions.CurrentTemperatureUnit = _temperatureUnit;

            _backdropIndicator = UIExtensions.GetBackdropDisplayName(settings.BackdropStyle);
        }

        public void NotifySettingsLoaded()
        {
            OnPropertyChanged(nameof(ShowCPU));
            OnPropertyChanged(nameof(ShowGPU));
            OnPropertyChanged(nameof(ShowMotherboard));
            OnPropertyChanged(nameof(ShowStorage));
            OnPropertyChanged(nameof(ShowMemory));
            OnPropertyChanged(nameof(ShowNetwork));
            OnPropertyChanged(nameof(ShowController));
            OnPropertyChanged(nameof(ShowBattery));
            OnPropertyChanged(nameof(ShowPsu));
            OnPropertyChanged(nameof(TemperatureUnit));
            OnPropertyChanged(nameof(ActiveSpeedButton));
            OnPropertyChanged(nameof(IsUltraActive));
            OnPropertyChanged(nameof(IsFastActive));
            OnPropertyChanged(nameof(IsNormalActive));
            OnPropertyChanged(nameof(CurrentViewMode));
        }

        private void ScheduleSave()
        {
            lock (_saveLock)
            {
                _pendingSaveCount++;
                if (_pendingSaveCount == 1)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        lock (_saveLock)
                        {
                            _settingsService.Save();
                            _pendingSaveCount = 0;
                        }
                    });
                }
            }
        }

        #endregion

        #region Initialization

        public async Task InitializeAsync()
        {
            try
            {
                IsUIBusy = true;
                SystemStatusText = UIConstants.GetInitializationMessage();

                await _hardwareService.InitializeAsync(_cts.Token);

                SystemStatusText = UIConstants.GetBuildingInterfaceMessage();

                await _hardwareService.BuildHardwareStructureAsync(_cts.Token);

                RestoreExpansionStates();

                if (_settingsService.Settings.RefreshInterval != UIConstants.UltraInterval)
                {
                    _hardwareService.ChangeInterval(_settingsService.Settings.RefreshInterval);
                }

                UpdateFilteredHardwareNodes();

                var totalCount = _hardwareService.DetectedHardwareCount;
                var storageCount = _hardwareService.DetectedStorageCount;

                SystemStatusText = totalCount > 0
                    ? UIConstants.GetActiveMonitoringMessage(totalCount, storageCount)
                    : UIConstants.GetActiveMonitoringSimpleMessage();

                _hardwareService.StartTimer();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Initialization cancelled");
            }
            catch (Exception ex)
            {
                SystemStatusText = UIConstants.GetErrorMessage(ex.Message);
                _logger.LogCriticalError("INITIALIZATION", ex);
                throw;
            }
            finally
            {
                IsUIBusy = false;
            }
        }

        #endregion

        #region Command Implementations

        private void ExecuteChangeSpeed(object? parameter)
        {
            if (parameter?.ToString() is string intervalStr && int.TryParse(intervalStr, out int interval))
            {
                ChangeRefreshSpeed(interval);
            }
        }

        private void ExecuteResetMinMax()
        {
            ResetAllMinMax();
        }

        private void ExecuteRunDiagnostic()
        {
            _ = RunHardwareDiagnosticAsync();
        }

        private void ExecuteChangeBackdrop(object? parameter)
        {
            if (parameter is int index && Enum.IsDefined(typeof(BackdropStyle), index))
            {
                var backdropStyle = (BackdropStyle)index;
                _settingsService.Settings.BackdropStyle = backdropStyle;
                _settingsService.Save();
                SetBackdropIndicator(UIExtensions.GetBackdropDisplayName(backdropStyle));
                OnPropertyChanged(nameof(IsBackdropAcrylic));
                OnPropertyChanged(nameof(IsBackdropMica));
                OnPropertyChanged(nameof(IsBackdropMicaAlt));
            }
        }

        private void ExecuteSetViewMode(object? parameter)
        {
            if (parameter is int index && Enum.IsDefined(typeof(ViewMode), index))
            {
                CurrentViewMode = (ViewMode)index;
            }
        }

        private void ExecuteSetTemperatureUnit(object? parameter)
        {
            if (parameter is int index && Enum.IsDefined(typeof(TemperatureUnit), index))
            {
                TemperatureUnit = (TemperatureUnit)index;
            }
        }

        #endregion

        #region Business Logic

        public void ChangeRefreshSpeed(int intervalMs)
        {
            _hardwareService.ChangeInterval(intervalMs);
            _settingsService.Settings.RefreshInterval = intervalMs;
            _settingsService.Save();

            UpsIndicator = UIConstants.GetInitialUpsIndicator(intervalMs);
            OnPropertyChanged(nameof(ActiveSpeedButton));
            OnPropertyChanged(nameof(IsUltraActive));
            OnPropertyChanged(nameof(IsFastActive));
            OnPropertyChanged(nameof(IsNormalActive));
        }

        public void ResetAllMinMax()
        {
            foreach (var hardwareNode in _hardwareService.HardwareNodes)
            {
                foreach (var sensor in hardwareNode.Sensors)
                {
                    sensor.ResetMinMax();
                }

                foreach (var subNode in hardwareNode.SubHardware)
                {
                    foreach (var sensor in subNode.Sensors)
                    {
                        sensor.ResetMinMax();
                    }
                }
            }

            _logger.LogSuccess("Min/Max values reset");
        }

        public async Task RunHardwareDiagnosticAsync()
        {
            try
            {
                SystemStatusText = UIConstants.GetDiagnosticMessage();

                await _hardwareService.ForceHardwareRedetectionWithUIAsync(_cts.Token).ConfigureAwait(true);

                UpdateFilteredHardwareNodes();

                var totalCount = _hardwareService.DetectedHardwareCount;
                var storageCount = _hardwareService.DetectedStorageCount;
                SystemStatusText = totalCount > 0
                    ? UIConstants.GetActiveMonitoringMessage(totalCount, storageCount)
                    : UIConstants.GetActiveMonitoringSimpleMessage();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Diagnostic cancelled");
            }
            catch (Exception ex)
            {
                SystemStatusText = UIConstants.GetDiagnosticErrorMessage(ex.Message);
                _logger.LogError("Error during diagnostic", ex);
            }
        }

        public void SetBackdropIndicator(string displayName)
        {
            if (displayName == null) throw new ArgumentNullException(nameof(displayName));
            BackdropIndicator = displayName;
        }

        #endregion

        #region Event Handlers

        private async void OnTimerTick(object? sender, EventArgs e)
        {
            if (_isUIBusy || !IsInitialized) return;

            try
            {
                await _hardwareService.UpdateSensorValuesAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogCriticalError("OnTimerTick", ex);
                _hardwareService.StopTimer();
                SystemStatusText = "\u274c Monitoring stopped due to critical error";
            }
        }

        private void OnUpsUpdated(object? sender, int currentUps)
        {
            UpsIndicator = UIConstants.GetUpsIndicator(currentUps, _hardwareService.CurrentInterval);
        }

        private void OnHardwareNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFilteredHardwareNodes();
        }

        #endregion

        #region BaseViewModel Implementation

        protected override void DisposeManaged()
        {
            try
            {
                _settingsService.Save();
            }
            catch { }

            _cts.Cancel();
            _cts.Dispose();

            _hardwareService.TimerTick -= OnTimerTick;
            _hardwareService.UpsUpdated -= OnUpsUpdated;
            _hardwareService.HardwareNodes.CollectionChanged -= OnHardwareNodesChanged;
            _hardwareService.ExpansionStateChanged -= OnExpansionStateChanged;
        }

        #endregion
    }
}
