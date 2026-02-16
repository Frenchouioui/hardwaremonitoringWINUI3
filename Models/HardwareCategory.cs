namespace HardwareMonitorWinUI3.Models
{
    public enum HardwareCategory
    {
        Cpu,
        Gpu,
        Motherboard,
        Storage,
        Memory,
        Network,
        Controller,
        Other
    }

    public static class HardwareCategoryExtensions
    {
        public static string GetIcon(this HardwareCategory category) => category switch
        {
            HardwareCategory.Cpu => "\uE950",
            HardwareCategory.Gpu => "\uE950",
            HardwareCategory.Motherboard => "\uE950",
            HardwareCategory.Storage => "\uE88E",
            HardwareCategory.Memory => "\uE9D9",
            HardwareCategory.Network => "\uE968",
            HardwareCategory.Controller => "\uE713",
            _ => "\uE950"
        };
    }
}
