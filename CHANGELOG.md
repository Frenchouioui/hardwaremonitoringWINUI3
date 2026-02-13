# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-02-13

### Added
- Initial release of HardwareMonitorWinUI3
- Complete hardware monitoring using LibreHardwareMonitorLib
- Support for CPU, GPU, Motherboard, Memory, Storage, and Network sensors
- Real-time sensor updates with configurable refresh rates (Ultra/Rapide/Normal)
- Min/Max value tracking for all sensors
- Hardware diagnostic tool for redetection
- Modern WinUI 3 interface with Mica backdrop support
- MVVM architecture with clean separation of concerns
- Thread-safe hardware monitoring with DispatcherQueue
- Filtering system to show/hide hardware types
- Multiple backdrop styles (Mica, Acrylic, Transparent)

### Supported Hardware
- **CPU**: Temperature, Load, Clock, Power (Intel/AMD)
- **GPU**: Temperature, Load, Clock, Power, Throughput (NVIDIA/AMD/Intel)
- **Motherboard**: SuperIO sensors (Temperature, Voltage, Fan)
- **Memory**: Load, Data
- **Storage**: Temperature, Throughput (HDD/SSD/NVMe)
- **Network**: Throughput, Load

### Technical Details
- Framework: .NET 8.0 with WinUI 3
- Minimum Windows: Windows 10 version 1809 (build 17763)
- Architecture: x64
- Required: Administrator rights for full sensor access

### Dependencies
- Microsoft.WindowsAppSDK 1.7.250310001
- LibreHardwareMonitorLib 0.9.6-pre632
