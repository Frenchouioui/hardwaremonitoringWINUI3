# Hardware Monitor WinUI 3

[![Build](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/actions/workflows/build.yml/badge.svg)](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/actions/workflows/build.yml)
[![License](https://img.shields.io/badge/license-MPL--2.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2011-0078D4.svg)](https://www.microsoft.com/en-us/windows/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![WinUI 3](https://img.shields.io/badge/WinUI%203-1.7-0078D4.svg)](https://docs.microsoft.com/windows/apps/winui/winui3/)

A modern hardware monitoring application built with **WinUI 3** and **.NET 10**, featuring real-time sensor tracking with a clean, native Windows 11 interface.

![Hardware Monitor Screenshot](Assets/app.gif)

## Features

- **Real-time Monitoring** - Track CPU, GPU, Motherboard, Storage, Memory, Network, Controllers, Battery, and PSU
- **Dual View Modes** - Switch between Cards view and compact Tree view
- **Modern UI** - Native Windows 11 design with Mica/Acrylic backdrop support
- **Adjustable Refresh Rate** - Ultra (250ms), Fast (500ms), Normal (1000ms)
- **Min/Max Tracking** - Monitor sensor value ranges with reset capability
- **Hardware Filtering** - Toggle visibility per category and hide individual items via right-click
- **Temperature Unit** - Toggle between Celsius and Fahrenheit
- **State Persistence** - Expand/collapse states saved across sessions
- **Diagnostic Mode** - Force hardware re-detection for troubleshooting

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
| **Battery** | Laptop batteries |
| **PSU** | Corsair, MSI power supplies |

## Download

Get the latest [release](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/releases).

## Requirements

- **Windows 11** (Windows 10 19041+ supported)
- **.NET 10.0 Desktop Runtime** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **PawnIO Driver** - [Download](https://github.com/namazso/PawnIO.Setup/releases/download/2.0.1/PawnIO_setup.exe)
- **Administrator Rights** - Required for hardware sensor access

## Architecture

```
📁 Core/                    → ViewModels and base classes
📁 Hardware/                → Hardware monitoring services (LibreHardwareMonitor)
📁 Models/                  → Data models (HardwareNode, SensorData, SensorGroup, AppSettings)
📁 Services/                → Settings persistence, Window state management
📁 UI/                      → Converters, Constants, Extensions
📁 Views/                   → MainWindow.xaml
📁 Shared/                  → Logger
📁 Assets/                  → App icons and images
📁 .github/workflows/       → GitHub Actions CI/CD
```

## Development

### Prerequisites

- Visual Studio 2022 17.8+ with:
  - .NET Desktop Development workload
  - Windows App SDK 1.7

### Build

```bash
git clone https://github.com/Frenchouioui/hardwaremonitoringWINUI3.git
cd hardwaremonitoringWINUI3
dotnet build HardwareMonitorWinUI3.csproj -c Release
```

## Troubleshooting

### No storage devices detected

1. Run as Administrator
2. Install PawnIO driver
3. Restart Windows Management Instrumentation service

### Application crashes on startup

1. Verify .NET 10.0 Runtime is installed
2. Check logs in `%LOCALAPPDATA%\HardwareMonitorWinUI3\Logs`
3. Run Windows App SDK repair

## License

This project is licensed under the [Mozilla Public License 2.0 (MPL-2.0)](LICENSE).

Uses [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) under MPL-2.0.

## Acknowledgments

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - Hardware monitoring library
- [WinUI 3](https://docs.microsoft.com/windows/apps/winui/winui3/) - Native Windows UI framework
- [PawnIO](https://github.com/namazso/PawnIO) - Kernel driver for hardware access
