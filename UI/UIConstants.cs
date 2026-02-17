using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.UI
{
    public static class UIConstants
    {
        public const string ApplicationTitle = "Hardware Monitor WinUI 3";

        public const int UltraInterval = 250;
        public const int FastInterval = 500;
        public const int NormalInterval = 1000;

        public static string GetInitializationMessage() => "\U0001f527 Initializing hardware monitoring...";
        public static string GetBuildingInterfaceMessage() => "\U0001f3d7\ufe0f Building interface...";
        public static string GetActiveMonitoringMessage(int totalHardware, int storageCount) =>
            $"\u2705 Monitoring active - Hardware detected: {totalHardware} ({storageCount} storage devices)";
        public static string GetActiveMonitoringSimpleMessage() => "\u2705 Monitoring active - Hardware initialized";
        public static string GetErrorMessage(string error) => $"\u274c Error: {error}";
        public static string GetDiagnosticMessage() => "\U0001f527 Running hardware diagnostic...";
        public static string GetDiagnosticErrorMessage(string error) => $"\u274c Diagnostic error: {error}";

        public static string GetUpsIndicator(int ups, int intervalMs) => $"{ups} UPS ({intervalMs}ms)";
        public static string GetInitialUpsIndicator(int intervalMs) => $"0 UPS ({intervalMs}ms)";

        public static string GetActiveSpeedButton(int currentInterval) => currentInterval switch
        {
            UltraInterval => "Ultra",
            FastInterval => "Fast",
            NormalInterval => "Normal",
            _ => "Ultra"
        };

        public static string GetCategoryIcon(HardwareCategory category) => category switch
        {
            HardwareCategory.Cpu => "\uE950",
            HardwareCategory.Gpu => "\uE7F4",
            HardwareCategory.Motherboard => "\uEDA2",
            HardwareCategory.Storage => "\uE88E",
            HardwareCategory.Memory => "\uE9D9",
            HardwareCategory.Network => "\uE968",
            HardwareCategory.Controller => "\uE713",
            _ => "\uE950"
        };
    }
}
