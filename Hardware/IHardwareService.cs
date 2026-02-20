using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.Hardware
{
    public interface IHardwareService : IDisposable
    {
        bool IsInitialized { get; }
        int CurrentInterval { get; }
        int CurrentUps { get; }
        int DetectedHardwareCount { get; }
        int DetectedStorageCount { get; }

        ObservableCollection<HardwareNode> HardwareNodes { get; }

        event EventHandler? TimerTick;
        event EventHandler<int>? UpsUpdated;
        event EventHandler<string>? ExpansionStateChanged;

        Task InitializeAsync(CancellationToken cancellationToken = default);
        Task BuildHardwareStructureAsync(CancellationToken cancellationToken = default);
        Task UpdateSensorValuesAsync(CancellationToken cancellationToken = default);
        Task ForceHardwareRedetectionWithUIAsync(CancellationToken cancellationToken = default);
        string GenerateDiagnosticReport();

        void StartTimer();
        void StopTimer();
        void ChangeInterval(int newInterval);
    }
}
