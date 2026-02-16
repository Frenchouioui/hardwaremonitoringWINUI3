# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-02-16

### Added

- Real-time hardware monitoring with LibreHardwareMonitorLib
- WinUI 3 native Windows 11 interface with Mica and Acrylic backdrop support
- Adjustable refresh rates: Ultra (250ms), Fast (500ms), Normal (1000ms)
- Min/Max value tracking with reset capability
- Hardware category filtering (CPU, GPU, Motherboard, Storage, Memory, Network, Controllers)
- Diagnostic mode for hardware re-detection
- File logging to `%LOCALAPPDATA%\HardwareMonitorWinUI3\Logs`
- Settings persistence (window position, backdrop, refresh rate, filters)
- MVVM architecture with dependency injection
- Unit tests with xUnit and Moq
- SubHardware support (SuperIO sensors under Motherboard)

### Supported Hardware

- Intel and AMD CPUs
- NVIDIA, AMD, and Intel GPUs
- HDD, SSD, and NVMe storage
- Motherboard sensors
- RAM modules
- Network adapters
- SuperIO and Embedded Controllers

### Technical Details

- Framework: .NET 8.0 with WinUI 3
- Minimum Windows: Windows 10 version 1809 (build 17763)
- Architecture: x64, ARM64
- Required: Administrator rights for full sensor access

### Dependencies

- Microsoft.WindowsAppSDK 1.7.250310001
- LibreHardwareMonitorLib 0.9.6-pre632
- Microsoft.Extensions.DependencyInjection 8.0.1

[1.0.0]: https://github.com/Frenchouioui/hardwaremonitoringWINUI3/releases/tag/v1.0.0
