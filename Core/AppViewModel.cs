using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Dispatching;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.Shared;
using HardwareMonitorWinUI3.UI;

namespace HardwareMonitorWinUI3.Core
{
    /// <summary>
    /// ViewModel principal - coordination application et état UI
    /// </summary>
    public class AppViewModel : BaseViewModel
    {
        #region Fields

        private readonly HardwareService _hardwareService;
        private readonly DispatcherQueue _dispatcherQueue;

        private string _systemStatusText = "Initialisation du monitoring hardware...";
        private string _upsIndicator = string.Empty;
        private string _backdropIndicator = "\u2022 Mica Alt";
        private bool _isUIBusy;
        private int _isUpdating; // 0 or 1, used with Interlocked

        private bool _showCPU = true;
        private bool _showGPU = true;
        private bool _showMotherboard = true;
        private bool _showStorage = true;
        private bool _showMemory = true;
        private bool _showNetwork = true;
        private bool _showController = true;

        #endregion

        #region Properties

        private readonly ObservableCollection<HardwareNode> _filteredHardwareNodes;
        public ObservableCollection<HardwareNode> HardwareNodes => _filteredHardwareNodes;

        private void UpdateFilteredHardwareNodes()
        {
            var nodesToAdd = new List<HardwareNode>();

            foreach (var hardwareNode in _hardwareService.HardwareNodes)
            {
                if (ShouldShowHardwareNode(hardwareNode))
                {
                    nodesToAdd.Add(hardwareNode);
                }
            }

            _dispatcherQueue.TryEnqueue(() =>
            {
                _filteredHardwareNodes.Clear();
                foreach (var hardwareNode in nodesToAdd)
                {
                    _filteredHardwareNodes.Add(hardwareNode);
                }
            });
        }

        private bool ShouldShowHardwareNode(HardwareNode hardwareNode)
        {
            if (hardwareNode.HardwareReference == null) return true;

            var hardwareType = hardwareNode.HardwareReference.HardwareType;

            return hardwareType switch
            {
                LibreHardwareMonitor.Hardware.HardwareType.Cpu => ShowCPU,
                LibreHardwareMonitor.Hardware.HardwareType.GpuNvidia => ShowGPU,
                LibreHardwareMonitor.Hardware.HardwareType.GpuAmd => ShowGPU,
                LibreHardwareMonitor.Hardware.HardwareType.GpuIntel => ShowGPU,
                LibreHardwareMonitor.Hardware.HardwareType.Motherboard => ShowMotherboard,
                LibreHardwareMonitor.Hardware.HardwareType.Storage => ShowStorage,
                LibreHardwareMonitor.Hardware.HardwareType.Memory => ShowMemory,
                LibreHardwareMonitor.Hardware.HardwareType.Network => ShowNetwork,
                LibreHardwareMonitor.Hardware.HardwareType.SuperIO => ShowMotherboard,
                LibreHardwareMonitor.Hardware.HardwareType.EmbeddedController => ShowController,
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
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool ShowGPU
        {
            get => _showGPU;
            set
            {
                if (SetProperty(ref _showGPU, value))
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool ShowMotherboard
        {
            get => _showMotherboard;
            set
            {
                if (SetProperty(ref _showMotherboard, value))
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool ShowStorage
        {
            get => _showStorage;
            set
            {
                if (SetProperty(ref _showStorage, value))
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool ShowMemory
        {
            get => _showMemory;
            set
            {
                if (SetProperty(ref _showMemory, value))
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool ShowNetwork
        {
            get => _showNetwork;
            set
            {
                if (SetProperty(ref _showNetwork, value))
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool ShowController
        {
            get => _showController;
            set
            {
                if (SetProperty(ref _showController, value))
                    UpdateFilteredHardwareNodes();
            }
        }

        public bool IsInitialized => _hardwareService.IsInitialized;
        public int CurrentInterval => _hardwareService.CurrentInterval;

        public int UltraInterval => UIConstants.UltraInterval;
        public int RapideInterval => UIConstants.RapideInterval;
        public int NormalInterval => UIConstants.NormalInterval;

        public int DefaultBackdropIndex => UIConstants.DefaultBackdropIndex;

        public string ActiveSpeedButton => UIConstants.GetActiveSpeedButton(CurrentInterval);

        #endregion

        #region Commands

        public ICommand ChangeSpeedCommand { get; }
        public ICommand ResetMinMaxCommand { get; }
        public ICommand RunDiagnosticCommand { get; }
        public ICommand ChangeBackdropCommand { get; }

        #endregion

        #region Constructor

        public AppViewModel(HardwareService hardwareService, DispatcherQueue dispatcherQueue)
        {
            _hardwareService = hardwareService ?? throw new ArgumentNullException(nameof(hardwareService));
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
            _filteredHardwareNodes = new ObservableCollection<HardwareNode>();

            ChangeSpeedCommand = new RelayCommand<object>(ExecuteChangeSpeed);
            ResetMinMaxCommand = new RelayCommand(ExecuteResetMinMax);
            RunDiagnosticCommand = new RelayCommand(ExecuteRunDiagnostic);
            ChangeBackdropCommand = new RelayCommand<object>(ExecuteChangeBackdrop);

            _hardwareService.TimerTick += OnTimerTick;
            _hardwareService.UpsUpdated += OnUpsUpdated;
            _hardwareService.HardwareNodes.CollectionChanged += OnHardwareNodesChanged;

            _upsIndicator = UIConstants.GetInitialUpsIndicator(UIConstants.UltraInterval);
            Logger.LogInfo($"Configuration chargée - Intervalle par défaut: {UIConstants.UltraInterval}ms");
        }

        #endregion

        #region Initialization

        public async Task InitializeAsync()
        {
            try
            {
                IsUIBusy = true;
                SystemStatusText = UIConstants.GetInitializationMessage();

                await _hardwareService.InitializeAsync();

                SystemStatusText = UIConstants.GetBuildingInterfaceMessage();

                await _hardwareService.BuildHardwareStructureAsync();

                UpdateFilteredHardwareNodes();

                if (_hardwareService.Computer != null)
                {
                    var storageCount = _hardwareService.Computer.Hardware.Count(h => h.HardwareType == LibreHardwareMonitor.Hardware.HardwareType.Storage);
                    var totalHardwareCount = _hardwareService.Computer.Hardware.Count();

                    SystemStatusText = UIConstants.GetActiveMonitoringMessage(totalHardwareCount, storageCount);
                }
                else
                {
                    SystemStatusText = UIConstants.GetActiveMonitoringSimpleMessage();
                }

                _hardwareService.StartTimer();
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

        private async void ExecuteRunDiagnostic()
        {
            try
            {
                await RunHardwareDiagnosticAsync();
            }
            catch (Exception ex)
            {
                SystemStatusText = UIConstants.GetDiagnosticErrorMessage(ex.Message);
                Logger.LogError("Erreur lors du diagnostic", ex);
            }
        }

        private void ExecuteChangeBackdrop(object? parameter)
        {
            if (parameter is int index)
            {
                SetBackdropIndicator(UIExtensions.GetBackdropDisplayName(index));
            }
        }

        #endregion

        #region Business Logic

        public void ChangeRefreshSpeed(int intervalMs)
        {
            _hardwareService.ChangeInterval(intervalMs);
            UpsIndicator = UIConstants.GetInitialUpsIndicator(intervalMs);
            OnPropertyChanged(nameof(ActiveSpeedButton));
        }

        public void ResetAllMinMax()
        {
            // Iterate ALL nodes (not just filtered) to reset everything
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

            Logger.LogSuccess("Valeurs Min/Max réinitialisées");
        }

        public async Task RunHardwareDiagnosticAsync()
        {
            SystemStatusText = UIConstants.GetDiagnosticMessage();

            await _hardwareService.ForceHardwareRedetectionWithUIAsync();

            UpdateFilteredHardwareNodes();

            if (_hardwareService.Computer != null)
            {
                var totalHardwareCount = _hardwareService.Computer.Hardware.Count();
                SystemStatusText = UIConstants.GetActiveMonitoringMessage(totalHardwareCount, 0);
            }
            else
            {
                SystemStatusText = UIConstants.GetActiveMonitoringSimpleMessage();
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

            // Atomic check-and-set to prevent concurrent updates
            if (Interlocked.CompareExchange(ref _isUpdating, 1, 0) != 0) return;

            try
            {
                await _hardwareService.UpdateSensorValuesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("OnTimerTick", ex);
                _hardwareService.StopTimer();
                SystemStatusText = "\u274c Monitoring arr\u00eat\u00e9 suite \u00e0 une erreur critique";
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
            _hardwareService.TimerTick -= OnTimerTick;
            _hardwareService.UpsUpdated -= OnUpsUpdated;
            _hardwareService.HardwareNodes.CollectionChanged -= OnHardwareNodesChanged;
            _hardwareService.Dispose();
        }

        #endregion
    }
}
