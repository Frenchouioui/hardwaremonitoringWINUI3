using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private readonly Func<Action, bool> _dispatch;
        private CancellationTokenSource _cts = new();

        private string _systemStatusText = "Initializing hardware monitoring...";
        private string _upsIndicator = string.Empty;
        private string _backdropIndicator = "\u2022 Mica Alt";
        private bool _isUIBusy;
        private int _isUpdating;

        private bool _showCPU = true;
        private bool _showGPU = true;
        private bool _showMotherboard = true;
        private bool _showStorage = true;
        private bool _showMemory = true;
        private bool _showNetwork = true;
        private bool _showController = true;

        private int _pendingSaveCount;
        private readonly object _saveLock = new();

        #endregion

        #region Properties

        private readonly ObservableCollection<HardwareNode> _filteredHardwareNodes;
        public ObservableCollection<HardwareNode> HardwareNodes => _filteredHardwareNodes;

        private void UpdateFilteredHardwareNodes()
        {
            var desired = new HashSet<HardwareNode>(
                _hardwareService.HardwareNodes.Where(ShouldShowHardwareNode));

            bool dispatched = _dispatch(() =>
            {
                for (int i = _filteredHardwareNodes.Count - 1; i >= 0; i--)
                {
                    if (!desired.Contains(_filteredHardwareNodes[i]))
                        _filteredHardwareNodes.RemoveAt(i);
                }

                foreach (var node in _hardwareService.HardwareNodes)
                {
                    if (desired.Contains(node) && !_filteredHardwareNodes.Contains(node))
                        _filteredHardwareNodes.Add(node);
                }
            });

            if (!dispatched)
            {
                Logger.LogWarning("Failed to dispatch UpdateFilteredHardwareNodes");
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
            set
            {
                if (SetProperty(ref _showCPU, value))
                {
                    _settingsService.Settings.ShowCPU = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
        }

        public bool ShowGPU
        {
            get => _showGPU;
            set
            {
                if (SetProperty(ref _showGPU, value))
                {
                    _settingsService.Settings.ShowGPU = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
        }

        public bool ShowMotherboard
        {
            get => _showMotherboard;
            set
            {
                if (SetProperty(ref _showMotherboard, value))
                {
                    _settingsService.Settings.ShowMotherboard = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
        }

        public bool ShowStorage
        {
            get => _showStorage;
            set
            {
                if (SetProperty(ref _showStorage, value))
                {
                    _settingsService.Settings.ShowStorage = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
        }

        public bool ShowMemory
        {
            get => _showMemory;
            set
            {
                if (SetProperty(ref _showMemory, value))
                {
                    _settingsService.Settings.ShowMemory = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
        }

        public bool ShowNetwork
        {
            get => _showNetwork;
            set
            {
                if (SetProperty(ref _showNetwork, value))
                {
                    _settingsService.Settings.ShowNetwork = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
        }

        public bool ShowController
        {
            get => _showController;
            set
            {
                if (SetProperty(ref _showController, value))
                {
                    _settingsService.Settings.ShowController = value;
                    ScheduleSave();
                    UpdateFilteredHardwareNodes();
                }
            }
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

        #endregion

        #region Commands

        public ICommand ChangeSpeedCommand { get; }
        public ICommand ResetMinMaxCommand { get; }
        public ICommand RunDiagnosticCommand { get; }
        public ICommand ChangeBackdropCommand { get; }

        #endregion

        #region Constructor

        public AppViewModel(
            IHardwareService hardwareService,
            ISettingsService settingsService,
            Func<Action, bool>? dispatcher = null)
        {
            _hardwareService = hardwareService ?? throw new ArgumentNullException(nameof(hardwareService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _dispatch = dispatcher ?? (action => { action(); return true; });
            _filteredHardwareNodes = new ObservableCollection<HardwareNode>();

            ChangeSpeedCommand = new RelayCommand<object>(ExecuteChangeSpeed);
            ResetMinMaxCommand = new RelayCommand(ExecuteResetMinMax);
            RunDiagnosticCommand = new RelayCommand(ExecuteRunDiagnostic);
            ChangeBackdropCommand = new RelayCommand<object>(ExecuteChangeBackdrop);

            _hardwareService.TimerTick += OnTimerTick;
            _hardwareService.UpsUpdated += OnUpsUpdated;
            _hardwareService.HardwareNodes.CollectionChanged += OnHardwareNodesChanged;

            LoadSettings();

            _upsIndicator = UIConstants.GetInitialUpsIndicator(_settingsService.Settings.RefreshInterval);
            Logger.LogInfo($"Configuration loaded - Default interval: {_settingsService.Settings.RefreshInterval}ms");
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
            OnPropertyChanged(nameof(ActiveSpeedButton));
            OnPropertyChanged(nameof(IsUltraActive));
            OnPropertyChanged(nameof(IsFastActive));
            OnPropertyChanged(nameof(IsNormalActive));
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
                Logger.LogWarning("Initialization cancelled");
            }
            catch (Exception ex)
            {
                SystemStatusText = UIConstants.GetErrorMessage(ex.Message);
                Logger.LogCriticalError("INITIALIZATION", ex);
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

            Logger.LogSuccess("Min/Max values reset");
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
                Logger.LogWarning("Diagnostic cancelled");
            }
            catch (Exception ex)
            {
                SystemStatusText = UIConstants.GetDiagnosticErrorMessage(ex.Message);
                Logger.LogError("Error during diagnostic", ex);
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

            if (Interlocked.CompareExchange(ref _isUpdating, 1, 0) != 0) return;

            try
            {
                await _hardwareService.UpdateSensorValuesAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("OnTimerTick", ex);
                _hardwareService.StopTimer();
                SystemStatusText = "\u274c Monitoring stopped due to critical error";
            }
            finally
            {
                Interlocked.Exchange(ref _isUpdating, 0);
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
            _cts.Cancel();
            _cts.Dispose();

            _hardwareService.TimerTick -= OnTimerTick;
            _hardwareService.UpsUpdated -= OnUpsUpdated;
            _hardwareService.HardwareNodes.CollectionChanged -= OnHardwareNodesChanged;
        }

        #endregion
    }
}
