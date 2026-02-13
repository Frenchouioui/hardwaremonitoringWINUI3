# Hardware Monitor WinUI 3

[![Build](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/actions/workflows/build.yml/badge.svg)](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/actions)
[![License](https://img.shields.io/badge/license-MPL--2.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2011-orange.svg)](https://www.microsoft.com/en-us/windows/)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

Application de surveillance matérielle développée avec WinUI 3 et .NET 8.

## 💡 Fonctionnalités

Surveillez les informations des appareils suivants:

- Cartes mères
- Processeurs Intel et AMD
- Cartes graphiques NVIDIA, AMD et Intel
- Disques durs HDD, SSD et NVMe
- Cartes réseau
- Mémoire RAM
- Ventilateurs

## 📥 Téléchargement

Téléchargez la dernière version: [Releases](https://github.com/Frenchouioui/hardwaremonitoringWINUI3/releases).

**Note:** L'application nécessite les droits administrateur pour accéder à tous les capteurs hardware.

## Prérequis

- **Windows 11**
- **.NET 8.0 Desktop Runtime** - [Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PawnIO** - [Télécharger](https://github.com/namazso/PawnIO.Setup/releases/download/2.0.1/PawnIO_setup.exe)
- **Droits administrateur** - Requis pour accéder aux capteurs

## 🏗️ Architecture

```
📁 Core/          → ViewModels, coordination et base classes
📁 Hardware/      → Services et monitoring matériel
📁 UI/            → Extensions et utilitaires d'interface
📁 Models/        → Modèles de données avec notifications
📁 Shared/        → Utilitaires partagés (logging)
📁 Views/         → Vues et interfaces utilisateur
```

## 🔧 Développement

### Compilation
```bash
dotnet build HardwareMonitorWinUI3.csproj --configuration Release
```

## 📜 License

Ce projet est sous licence [Mozilla Public License 2.0 (MPL-2.0)](LICENSE).

Ce projet utilise [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) sous licence MPL-2.0.
