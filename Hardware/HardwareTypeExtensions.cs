using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.Hardware
{
    internal static class HardwareTypeExtensions
    {
        internal static HardwareCategory ToCategory(this HardwareType type) => type switch
        {
            HardwareType.Cpu => HardwareCategory.Cpu,
            HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel => HardwareCategory.Gpu,
            HardwareType.Motherboard or HardwareType.SuperIO => HardwareCategory.Motherboard,
            HardwareType.Storage => HardwareCategory.Storage,
            HardwareType.Memory => HardwareCategory.Memory,
            HardwareType.Network => HardwareCategory.Network,
            HardwareType.EmbeddedController => HardwareCategory.Controller,
            _ => HardwareCategory.Other
        };
    }
}
