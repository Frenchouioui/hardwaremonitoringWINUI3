# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2025-02-16

### Added

- Services layer with `SettingsService` and `WindowService` for better separation of concerns
- `AppSettings` model for persistent application configuration
- `IWindowService` interface for window state management
- `HardwareTypeExtensions` for cleaner hardware type to category mapping
- Directory.Build.props/targets for centralized build configuration
- Additional unit tests for `AppSettings`, `SettingsService`, `UIConstants`, and `UIExtensions`

### Changed

- Refactored dependency injection setup in `App.xaml.cs`
- Improved build configuration for unpackaged WinUI 3 application
- Updated GitHub Actions workflow with improved build process
- Enhanced Logger with log rotation and automatic cleanup (keeps 7 days)

### Fixed

- GitHub Actions workflow now uses correct `softprops/action-gh-release@v2`
- Build issues with Windows App SDK PRI generation for unpackaged apps

## [1.1.0] - 2025-02-15

### Added

- `ISensorData` interface for improved testability and abstraction
- Sensor cache in `HardwareNode` for O(1) lookup performance
- Comprehensive unit tests for `SensorData`, `HardwareNode`, and `SensorGroup`
- EditorConfig for consistent code style across IDEs
- XML documentation on all public API surfaces

### Changed

- Improved thread safety with `SemaphoreSlim` replacing `volatile` flag
- Optimized `UpdateNodeSensors` to use cached dictionary instead of LINQ ToLookup
- Refactored `async void` methods to `async Task` with proper fire-and-forget pattern
- Enhanced `UpdateMinMax` to avoid redundant string allocations
- Updated README with comprehensive documentation and badges

### Fixed

- Race condition in `UpdateSensorValuesAsync` could cause duplicate updates
- `BoolToAccentBrushConverter.ConvertBack` now returns proper value instead of throwing
- Potential exception swallowing in `async void` event handlers

## [1.0.0] - 2024-02-13

### Added

- Real-time hardware monitoring with LibreHardwareMonitorLib
- WinUI 3 native Windows 11 interface
- Mica and Acrylic backdrop support
- Adjustable refresh rates: Ultra (250ms), Fast (500ms), Normal (1000ms)
- Min/Max value tracking with reset capability
- Hardware category filtering (CPU, GPU, Motherboard, Storage, Memory, Network, Controllers)
- Diagnostic mode for hardware re-detection
- File logging to `%LOCALAPPDATA%\HardwareMonitorWinUI3\Logs`
- MVVM architecture with dependency injection
- xUnit test project with Moq

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
- Architecture: x64
- Required: Administrator rights for full sensor access

### Dependencies

- Microsoft.WindowsAppSDK 1.7.250310001
- LibreHardwareMonitorLib 0.9.6-pre632
- Microsoft.Extensions.DependencyInjection 8.0.1

[1.2.0]: https://github.com/Frenchouioui/hardwaremonitoringWINUI3/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/Frenchouioui/hardwaremonitoringWINUI3/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/Frenchouioui/hardwaremonitoringWINUI3/releases/tag/v1.0.0
