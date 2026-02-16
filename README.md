# Hardware Monitor WinUI 3

[![Build](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/actions/workflows/build.yml/badge.svg)](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/actions/workflows/build.yml)
[![License](https://img.shields.io/badge/license-MPL--2.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2011-0078D4.svg)](https://www.microsoft.com/en-us/windows/)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![WinUI 3](https://img.shields.io/badge/WinUI%203-1.7-0078D4.svg)](https://docs.microsoft.com/windows/apps/winui/winui3/)

A modern hardware monitoring application built with **WinUI 3** and **.NET 8**, featuring real-time sensor tracking with a clean, native Windows 11 interface.

## Features

- **Real-time Monitoring** - Track CPU, GPU, Motherboard, Storage, Memory, Network, and Controllers
- **Modern UI** - Native Windows 11 design with Mica/Acrylic backdrop support
- **Adjustable Refresh Rate** - Ultra (250ms), Fast (500ms), Normal (1000ms)
- **Min/Max Tracking** - Monitor sensor value ranges with reset capability
- **Hardware Filtering** - Toggle visibility per hardware category
- **Diagnostic Mode** - Force hardware re-detection for troubleshooting
- **Performance Optimized** - Cached lookups, minimal allocations, thread-safe updates

## Supported Hardware

| Category | Vendors |
|----------|---------|
| **CPU** | Intel, AMD |
| **GPU** | NVIDIA, AMD, Intel |
| **Storage** | HDD, SSD, NVMe |
| **Motherboard** | All major manufacturers |
| **Memory** | RAM modules with SPD |
| **Network** | Ethernet, Wi-Fi adapters |
| **Controllers** | SuperIO, Embedded Controllers |

## Requirements

- **Windows 11** (Windows 10 19041+ supported)
- **.NET 8.0 Desktop Runtime** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PawnIO Driver** - [Download](https://github.com/namazso/PawnIO.Setup/releases)
- **Administrator Rights** - Required for hardware sensor access

## Download

Get the latest release from [Releases](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/releases).

## Architecture

```
📁 Core/                    → ViewModels and base classes
   └── AppViewModel.cs      → Main application logic
   └── BaseViewModel.cs     → INPC + IDisposable base

📁 Hardware/                → Hardware monitoring services
   └── HardwareService.cs   → LibreHardwareMonitor integration
   └── IHardwareService.cs  → Service interface
   └── DiagnosticHelper.cs  → Hardware diagnostics
   └── HardwareTypeExtensions.cs → Hardware type mapping

📁 Models/                  → Data models
   └── HardwareNode.cs      → Hardware item with sensors
   └── SensorData.cs        → Sensor value with min/max
   └── SensorGroup.cs       → Grouped sensors by type
   └── ISensorData.cs       → Sensor interface
   └── AppSettings.cs       → Application settings model
   └── UpdateVisitor.cs     → Hardware update visitor

📁 Services/                → Application services
   └── SettingsService.cs   → Settings persistence
   └── ISettingsService.cs  → Settings interface
   └── WindowService.cs     → Window state management
   └── IWindowService.cs    → Window service interface

📁 UI/                      → UI utilities
   └── Converters.cs        → XAML value converters
   └── UIExtensions.cs      → WinUI helpers
   └── UIConstants.cs       → UI constants
   └── RelayCommand.cs      → ICommand implementation

📁 Views/                   → XAML views
   └── MainWindow.xaml      → Main window

📁 Shared/                  → Shared utilities
   └── Logger.cs            → File + Trace logging
```

## Development

### Prerequisites

- Visual Studio 2022 17.8+ with:
  - .NET Desktop Development workload
  - Windows App SDK 1.7

### Build

```bash
# Clone the repository
git clone https://github.com/Frenchouioui/hardwaremonitoringWINUI3.git
cd hardwaremonitoringWINUI3

# Build Release
dotnet build HardwareMonitorWinUI3.csproj -c Release

# Run tests
dotnet test HardwareMonitorWinUI3.Tests/HardwareMonitorWinUI3.Tests.csproj
```

### Project Structure

| Project | Description |
|---------|-------------|
| `HardwareMonitorWinUI3` | Main WinUI 3 application |
| `HardwareMonitorWinUI3.Tests` | xUnit test project |

## Key Implementation Details

### Performance Optimizations

- **Sensor Cache**: Dictionary lookup O(1) instead of LINQ ToLookup O(n) per update
- **Minimal Allocations**: Reuse formatted strings, avoid redundant ToString()
- **Thread Safety**: SemaphoreSlim for update synchronization
- **DispatcherQueue**: Efficient UI thread marshalling

### MVVM Pattern

- Strict separation: Model → ViewModel → View
- Dependency Injection via `ServiceProvider`
- `RelayCommand` for ICommand implementation
- `INotifyPropertyChanged` via `BaseViewModel`

### Memory Management

- Proper `IDisposable` implementation
- No circular references (removed ISensor references)
- Event unsubscription on dispose
- CancellationTokenSource cleanup

## Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Runtime |
| WinUI 3 | 1.7.250310001 | UI Framework |
| LibreHardwareMonitorLib | 0.9.6-pre632 | Hardware Monitoring |
| Microsoft.Extensions.DependencyInjection | 8.0.1 | DI Container |
| xUnit | 2.9.3 | Testing |
| Moq | 4.20.72 | Mocking |
| coverlet | 6.0.0 | Code Coverage |

## Troubleshooting

### No storage devices detected

1. Run as Administrator
2. Install PawnIO driver
3. Restart Windows Management Instrumentation service

### Application crashes on startup

1. Verify .NET 8.0 Runtime is installed
2. Check logs in `%LOCALAPPDATA%\HardwareMonitorWinUI3\Logs`
3. Run Windows App SDK repair

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the [Mozilla Public License 2.0 (MPL-2.0)](LICENSE).

Uses [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) under MPL-2.0.

## Acknowledgments

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - Hardware monitoring library
- [WinUI 3](https://docs.microsoft.com/windows/apps/winui/winui3/) - Native Windows UI framework
- [PawnIO](https://github.com/namazso/PawnIO) - Kernel driver for hardware access
