// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// Copyright (c) 2024 HardwareMonitorWinUI3 Contributors

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;
using HardwareMonitorWinUI3.Shared;
using HardwareMonitorWinUI3.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace HardwareMonitorWinUI3.Hardware
{
    /// <summary>
    /// Service hardware unifié - monitoring hardware complet
    /// </summary>
    public class HardwareService : IDisposable
    {
        #region Fields

        private Computer? _computer;
        private UpdateVisitor? _updateVisitor;
        private volatile bool _isInitialized;
        private bool _disposed;
        private readonly DispatcherQueue _dispatcherQueue;

        private DispatcherTimer? _timer;
        private int _currentInterval = UIConstants.UltraInterval;
        private int _updateCount;
        private DateTime _lastUpsUpdate = DateTime.Now;
        private int _currentUps;
        private readonly object _lockObject = new();

        #endregion

        #region Properties & Events

        public bool IsInitialized => _isInitialized;
        public Computer? Computer => _computer;
        public int CurrentInterval => _currentInterval;
        public int CurrentUps => _currentUps;

        public event EventHandler? TimerTick;
        public event EventHandler<int>? UpsUpdated;

        public ObservableCollection<HardwareNode> HardwareNodes { get; }

        #endregion

        #region Constructor

        public HardwareService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
            HardwareNodes = new ObservableCollection<HardwareNode>();
        }

        #endregion

        #region Hardware Monitoring

        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    Logger.LogInfo("Tentative d'initialisation LibreHardwareMonitor...");

                    _computer = new Computer
                    {
                        IsCpuEnabled = true,
                        IsGpuEnabled = true,
                        IsMemoryEnabled = true,
                        IsMotherboardEnabled = true,
                        IsControllerEnabled = true,
                        IsNetworkEnabled = true,
                        IsStorageEnabled = true
                    };

                    Logger.LogInfo("Ouverture de la connexion hardware...");
                    _computer.Open();

                    Logger.LogInfo("Création du visiteur de mise à jour...");
                    _updateVisitor = new UpdateVisitor();

                    _computer.Accept(_updateVisitor);

                    DiagnosticHelper.LogHardwareDetection(_computer);

                    _isInitialized = true;
                    Logger.LogSuccess("Initialisation LibreHardwareMonitor réussie");
                }
                catch (UnauthorizedAccessException ex)
                {
                    Logger.LogError("ERREUR DROITS ADMINISTRATEUR", ex);
                    Logger.LogWarning("L'application doit être lancée en tant qu'administrateur");
                    throw new InvalidOperationException("LibreHardwareMonitor nécessite des droits administrateur. Veuillez relancer l'application en tant qu'administrateur.", ex);
                }
                catch (Exception ex)
                {
                    Logger.LogCriticalError("INITIALISATION", ex);
                    throw;
                }
            });
        }

        public async Task BuildHardwareStructureAsync()
        {
            if (_computer?.Hardware == null) return;

            var hardwareList = await Task.Run(() => _computer.Hardware.ToList());

            var tcs = new TaskCompletionSource();
            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
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
                                HardwareReference = hardware
                            };
                            ProcessSensors(hardware, hardwareNode);
                        }

                        if (hardware.SubHardware != null)
                        {
                            foreach (var subHardware in hardware.SubHardware)
                            {
                                string displayName = subHardware.HardwareType == HardwareType.SuperIO
                                    ? $"{hardware.Name} - Capteurs"
                                    : subHardware.Name;

                                var subNode = new HardwareNode
                                {
                                    Name = displayName,
                                    HardwareReference = subHardware
                                };

                                ProcessSensors(subHardware, subNode);

                                if (subNode.Sensors.Count > 0)
                                {
                                    subNode.OrganizeSensorsIntoGroups();
                                    hardwareNode?.SubHardware.Add(subNode);
                                    HardwareNodes.Add(subNode);
                                }
                            }
                        }

                        if (hardwareNode != null && hardwareNode.Sensors.Count > 0)
                        {
                            hardwareNode.OrganizeSensorsIntoGroups();
                            HardwareNodes.Add(hardwareNode);
                        }
                    }

                    Logger.LogInfo($"TOTAL NOEUDS AJOUTÉS: {HardwareNodes.Count}");
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

            await tcs.Task;
        }

        public async Task UpdateSensorValuesAsync()
        {
            await Task.Run(() =>
            {
                if (_computer is not null && _updateVisitor is not null)
                {
                    _computer.Accept(_updateVisitor);
                }
            });

            var tcs = new TaskCompletionSource();
            _dispatcherQueue.TryEnqueue(() =>
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

            await tcs.Task;
        }

        private void ProcessSensors(IHardware hardware, HardwareNode targetNode)
        {
            if (hardware == null) throw new ArgumentNullException(nameof(hardware));
            if (targetNode == null) throw new ArgumentNullException(nameof(targetNode));

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

        private void UpdateNodeSensors(HardwareNode node, IHardware hardware)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (hardware == null) throw new ArgumentNullException(nameof(hardware));

            // Access the array directly instead of ToList() to avoid allocation per tick
            var sensors = hardware.Sensors;
            int count = Math.Min(node.Sensors.Count, sensors.Length);
            for (int sensorIndex = 0; sensorIndex < count; sensorIndex++)
            {
                var sensor = sensors[sensorIndex];
                var sensorData = node.Sensors[sensorIndex];

                if (sensor.Value.HasValue)
                {
                    string newFormattedValue = sensor.ToFormattedString();

                    if (sensorData.Value != newFormattedValue)
                    {
                        sensorData.Value = newFormattedValue;

                        var rawValue = sensor.Value.Value;
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
                _lastUpsUpdate = DateTime.Now;
            }

            StartTimer();
        }

        private void Timer_Tick(object? sender, object e)
        {
            // Copy values under lock, invoke events outside to avoid potential deadlocks
            int? upsToReport = null;
            lock (_lockObject)
            {
                _updateCount++;

                var now = DateTime.Now;
                if ((now - _lastUpsUpdate).TotalSeconds >= 1.0)
                {
                    _currentUps = _updateCount;
                    _updateCount = 0;
                    _lastUpsUpdate = now;
                    upsToReport = _currentUps;
                }
            }

            if (upsToReport.HasValue)
                UpsUpdated?.Invoke(this, upsToReport.Value);

            TimerTick?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Diagnostic

        public async Task ForceHardwareRedetectionAsync()
        {
            _isInitialized = false;

            await Task.Run(() =>
            {
                _computer?.Close();

                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = true,
                    IsControllerEnabled = true,
                    IsNetworkEnabled = true,
                    IsStorageEnabled = true
                };

                _computer.Open();
                _updateVisitor = new UpdateVisitor();

                foreach (var hardware in _computer.Hardware)
                {
                    hardware.Update();

                    foreach (var subHardware in hardware.SubHardware)
                    {
                        subHardware.Update();
                    }
                }

                _isInitialized = true;
            });
        }

        public string GenerateDiagnosticReport()
        {
            if (_computer == null)
                return "Computer non initialisé";

            return DiagnosticHelper.GenerateHardwareDiagnosticReport(_computer);
        }

        public async Task ForceHardwareRedetectionWithUIAsync()
        {
            // Stop timer during redetection to prevent stale IHardware references
            StopTimer();
            try
            {
                await ForceHardwareRedetectionAsync();
                await BuildHardwareStructureAsync();

                var report = GenerateDiagnosticReport();
                Logger.LogInfo(report);

                StartTimer();
            }
            catch (Exception ex)
            {
                Logger.LogCriticalError("ForceHardwareRedetectionWithUI", ex);
                throw;
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_timer != null)
            {
                _timer.Tick -= Timer_Tick;
                _timer.Stop();
                _timer = null;
            }

            _computer?.Close();
            _computer = null;
            _updateVisitor = null;
            _isInitialized = false;
        }

        #endregion
    }
}
