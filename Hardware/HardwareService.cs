using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Shared;
using HardwareMonitorWinUI3.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace HardwareMonitorWinUI3.Hardware
{
    public class HardwareService : IHardwareService
    {
        #region Fields

        private Computer? _computer;
        private UpdateVisitor? _updateVisitor;
        private volatile bool _isInitialized;
        private readonly DispatcherQueue _dispatcherQueue;
        private CancellationTokenSource _cts = new();

        private DispatcherTimer? _timer;
        private volatile int _currentInterval = UIConstants.UltraInterval;
        private int _updateCount;
        private long _lastUpsTickCount;
        private int _currentUps;
        private readonly object _lockObject = new();
        private readonly SemaphoreSlim _updateLock = new(1, 1);
        private readonly SemaphoreSlim _computerLock = new(1, 1);

        #endregion

        #region Properties & Events

        public bool IsInitialized => _isInitialized;
        public int CurrentInterval => _currentInterval;
        public int CurrentUps => _currentUps;

        public int DetectedHardwareCount
        {
            get
            {
                _computerLock.Wait();
                try
                {
                    return _computer?.Hardware.Count() ?? 0;
                }
                finally
                {
                    _computerLock.Release();
                }
            }
        }

        public int DetectedStorageCount
        {
            get
            {
                _computerLock.Wait();
                try
                {
                    return _computer?.Hardware.Count(h => h.HardwareType == HardwareType.Storage) ?? 0;
                }
                finally
                {
                    _computerLock.Release();
                }
            }
        }

        public event EventHandler? TimerTick;
        public event EventHandler<int>? UpsUpdated;

        public ObservableCollection<HardwareNode> HardwareNodes { get; } = new();

        #endregion

        #region Constructor

        public HardwareService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        #endregion

        #region Hardware Monitoring

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

            Computer? localComputer = null;
            try
            {
                await Task.Run(() =>
                {
                    Logger.LogInfo("Initializing LibreHardwareMonitor...");

                    localComputer = new Computer
                    {
                        IsCpuEnabled = true,
                        IsGpuEnabled = true,
                        IsMemoryEnabled = true,
                        IsMotherboardEnabled = true,
                        IsControllerEnabled = true,
                        IsNetworkEnabled = true,
                        IsStorageEnabled = true
                    };

                    linked.Token.ThrowIfCancellationRequested();

                    Logger.LogInfo("Opening hardware connection...");
                    localComputer.Open();

                    Logger.LogInfo("Creating update visitor...");
                    _updateVisitor = new UpdateVisitor();

                    localComputer.Accept(_updateVisitor);

                    DiagnosticHelper.LogHardwareDetection(localComputer);

                    _computerLock.Wait(linked.Token);
                    try
                    {
                        _computer = localComputer;
                        localComputer = null;
                    }
                    finally
                    {
                        _computerLock.Release();
                    }

                    _isInitialized = true;
                    Logger.LogSuccess("LibreHardwareMonitor initialized successfully");
                }, linked.Token);
            }
            catch (OperationCanceledException)
            {
                localComputer?.Close();
                Logger.LogWarning("Hardware initialization cancelled");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                localComputer?.Close();
                Logger.LogError("ADMINISTRATOR RIGHTS ERROR", ex);
                Logger.LogWarning("The application must be run as administrator");
                throw new InvalidOperationException(
                    "LibreHardwareMonitor requires administrator rights. Please restart the application as administrator.", ex);
            }
            catch (Exception ex)
            {
                localComputer?.Close();
                Logger.LogCriticalError("INITIALIZATION", ex);
                throw;
            }
        }

        public async Task BuildHardwareStructureAsync(CancellationToken cancellationToken = default)
        {
            List<IHardware> hardwareList;
            _computerLock.Wait(cancellationToken);
            try
            {
                if (_computer?.Hardware == null) return;
                hardwareList = await Task.Run(() => _computer.Hardware.ToList(), cancellationToken);
            }
            finally
            {
                _computerLock.Release();
            }

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

            var tcs = new TaskCompletionSource();
            bool enqueued = _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    foreach (var hardwareNode in HardwareNodes)
                    {
                        hardwareNode.Dispose();
                    }
                    HardwareNodes.Clear();

                    foreach (var hardware in hardwareList)
                    {
                        DiagnosticHelper.LogStorageDetection(hardware);

                        bool hasDirectSensors = hardware.Sensors?.Any() == true;
                        bool shouldShowMainHardware = hasDirectSensors || hardware.HardwareType != HardwareType.Motherboard;

                        HardwareNode? hardwareNode = null;

                        if (shouldShowMainHardware)
                        {
                            hardwareNode = new HardwareNode
                            {
                                Name = hardware.Name,
                                Category = hardware.HardwareType.ToCategory(),
                                HardwareReference = hardware
                            };
                            ProcessSensors(hardware, hardwareNode);
                        }

                        if (hardware.SubHardware != null)
                        {
                            foreach (var subHardware in hardware.SubHardware)
                            {
                                string displayName = subHardware.HardwareType == HardwareType.SuperIO
                                    ? $"{hardware.Name} - Sensors"
                                    : subHardware.Name;

                                var subNode = new HardwareNode
                                {
                                    Name = displayName,
                                    Category = subHardware.HardwareType.ToCategory(),
                                    HardwareReference = subHardware
                                };

                                ProcessSensors(subHardware, subNode);

                                if (subNode.Sensors.Count > 0)
                                {
                                    subNode.OrganizeSensorsIntoGroups();
                                    if (hardwareNode != null)
                                    {
                                        hardwareNode.SubHardware.Add(subNode);
                                    }
                                    else
                                    {
                                        HardwareNodes.Add(subNode);
                                    }
                                }
                            }
                        }

                        if (hardwareNode != null && hardwareNode.Sensors.Count > 0)
                        {
                            hardwareNode.OrganizeSensorsIntoGroups();
                            HardwareNodes.Add(hardwareNode);
                        }
                    }

                    Logger.LogInfo($"TOTAL NODES ADDED: {HardwareNodes.Count}");
                    foreach (var hardwareNode in HardwareNodes)
                    {
                        Logger.LogInfo($"   - {hardwareNode.Name}");
                    }

                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            if (!enqueued)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue operation on dispatcher queue"));
            }

            await tcs.Task;
        }

        public async Task UpdateSensorValuesAsync(CancellationToken cancellationToken = default)
        {
            if (!await _updateLock.WaitAsync(0, cancellationToken).ConfigureAwait(false))
                return;

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

                _computerLock.Wait(linked.Token);
                try
                {
                    await Task.Run(() =>
                    {
                        if (_computer is not null && _updateVisitor is not null)
                        {
                            _computer.Accept(_updateVisitor);
                        }
                    }, linked.Token).ConfigureAwait(false);
                }
                finally
                {
                    _computerLock.Release();
                }

                var tcs = new TaskCompletionSource();
                bool enqueued = _dispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        foreach (var hardwareNode in HardwareNodes)
                        {
                            if (hardwareNode.HardwareReference != null)
                            {
                                UpdateNodeSensors(hardwareNode, hardwareNode.HardwareReference);
                                foreach (var subNode in hardwareNode.SubHardware)
                                {
                                    if (subNode.HardwareReference != null)
                                    {
                                        UpdateNodeSensors(subNode, subNode.HardwareReference);
                                    }
                                }
                            }
                        }
                        tcs.SetResult();
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                });

                if (!enqueued)
                {
                    tcs.SetException(new InvalidOperationException("Failed to enqueue update on dispatcher queue"));
                }

                await tcs.Task.ConfigureAwait(true);
            }
            finally
            {
                _updateLock.Release();
            }
        }

        private static void ProcessSensors(IHardware hardware, HardwareNode targetNode)
        {
            if (hardware.Sensors != null)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor != null)
                    {
                        var sensorData = sensor.CreateSensorData();
                        sensorData.SensorType = sensor.SensorType.ToString();
                        targetNode.Sensors.Add(sensorData);
                    }
                }
            }
        }

        private static void UpdateNodeSensors(HardwareNode node, IHardware hardware)
        {
            var sensors = hardware.Sensors;
            var cache = node.SensorCache;
            
            foreach (var sensor in sensors)
            {
                var key = $"{sensor.Name}|{sensor.SensorType}";
                
                if (cache.TryGetValue(key, out var sensorData))
                {
                    if (sensor.Value.HasValue)
                    {
                        string newFormattedValue = sensor.ToFormattedString();
                        var rawValue = sensor.Value.Value;

                        if (sensorData.Value != newFormattedValue)
                        {
                            sensorData.Value = newFormattedValue;

                            string unit = sensor.SensorType.GetSensorUnit();
                            string precision = sensor.SensorType.GetSensorPrecision();
                            sensorData.UpdateMinMax(rawValue, unit, precision);
                        }
                    }
                    else if (sensorData.Value != "N/A")
                    {
                        sensorData.Value = "N/A";
                    }
                }
            }
        }

        #endregion

        #region Timer

        public void StartTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromMilliseconds(_currentInterval);
                _timer.Start();
            }
            else
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(_currentInterval);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }

        public void ChangeInterval(int newInterval)
        {
            lock (_lockObject)
            {
                _currentInterval = newInterval;
                _updateCount = 0;
                _currentUps = 0;
                _lastUpsTickCount = Environment.TickCount64;
            }

            StartTimer();
        }

        private void Timer_Tick(object? sender, object e)
        {
            int? upsToReport = null;
            lock (_lockObject)
            {
                _updateCount++;

                var now = Environment.TickCount64;
                if (now - _lastUpsTickCount >= 1000)
                {
                    _currentUps = _updateCount;
                    _updateCount = 0;
                    _lastUpsTickCount = now;
                    upsToReport = _currentUps;
                }
            }

            if (upsToReport.HasValue)
                UpsUpdated?.Invoke(this, upsToReport.Value);

            TimerTick?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Diagnostic

        public async Task ForceHardwareRedetectionAsync(CancellationToken cancellationToken = default)
        {
            _isInitialized = false;

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

            Computer? newComputer = null;
            Computer? oldComputer = null;

            try
            {
                await Task.Run(() =>
                {
                    _computerLock.Wait(linked.Token);
                    try
                    {
                        oldComputer = _computer;
                        _computer = null;
                    }
                    finally
                    {
                        _computerLock.Release();
                    }

                    oldComputer?.Close();
                    oldComputer = null;

                    newComputer = new Computer
                    {
                        IsCpuEnabled = true,
                        IsGpuEnabled = true,
                        IsMemoryEnabled = true,
                        IsMotherboardEnabled = true,
                        IsControllerEnabled = true,
                        IsNetworkEnabled = true,
                        IsStorageEnabled = true
                    };

                    linked.Token.ThrowIfCancellationRequested();

                    newComputer.Open();
                    _updateVisitor = new UpdateVisitor();

                    foreach (var hardware in newComputer.Hardware)
                    {
                        hardware.Update();
                        foreach (var subHardware in hardware.SubHardware)
                        {
                            subHardware.Update();
                        }
                    }

                    _computerLock.Wait(linked.Token);
                    try
                    {
                        _computer = newComputer;
                        newComputer = null;
                    }
                    finally
                    {
                        _computerLock.Release();
                    }

                    _isInitialized = true;
                }, linked.Token);
            }
            catch (OperationCanceledException)
            {
                newComputer?.Close();
                throw;
            }
            catch (Exception ex)
            {
                newComputer?.Close();
                Logger.LogCriticalError("ForceHardwareRedetectionAsync", ex);
                throw;
            }
        }

        public string GenerateDiagnosticReport()
        {
            _computerLock.Wait();
            try
            {
                if (_computer == null)
                    return "Computer not initialized";

                return DiagnosticHelper.GenerateHardwareDiagnosticReport(_computer);
            }
            finally
            {
                _computerLock.Release();
            }
        }

        public async Task ForceHardwareRedetectionWithUIAsync(CancellationToken cancellationToken = default)
        {
            StopTimer();
            try
            {
                await ForceHardwareRedetectionAsync(cancellationToken);
                await BuildHardwareStructureAsync(cancellationToken);

                var report = GenerateDiagnosticReport();
                Logger.LogInfo(report);
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("ForceHardwareRedetectionWithUI", ex);
                throw;
            }
            finally
            {
                StartTimer();
            }
        }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _cts.Cancel();
                _cts.Dispose();

                if (_timer != null)
                {
                    _timer.Tick -= Timer_Tick;
                    _timer.Stop();
                    _timer = null;
                }

                _updateLock.Dispose();
                _computerLock.Dispose();

                foreach (var node in HardwareNodes)
                {
                    node.Dispose();
                }
                HardwareNodes.Clear();

                _computer?.Close();
                _computer = null;
            }

            _updateVisitor = null;
            _isInitialized = false;
        }

        #endregion
    }
}
