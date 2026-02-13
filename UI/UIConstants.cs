namespace HardwareMonitorWinUI3.UI
{
    /// <summary>
    /// Application-wide UI constants and message templates
    /// </summary>
    public static class UIConstants
    {
        public const string ApplicationTitle = "Hardware Monitor WinUI 3";

        public const string HardwareIcon = "\uE950";
        public const string ExpandDownIcon = "\uE70D";
        public const string ExpandRightIcon = "\uE70E";
        public const string ResetIcon = "\uE845";
        public const string DiagnosticIcon = "\uE9D9";

        public const int UltraInterval = 250;
        public const int RapideInterval = 500;
        public const int NormalInterval = 1000;

        public const int DefaultBackdropIndex = 2;

        public static string GetInitializationMessage() => "\U0001f527 Initialisation du monitoring hardware...";
        public static string GetBuildingInterfaceMessage() => "\U0001f3d7\ufe0f Construction de l'interface...";
        public static string GetActiveMonitoringMessage(int totalHardware, int storageCount) =>
            $"\u2705 Monitoring actif - Hardware d\u00e9tect\u00e9: {totalHardware} (dont {storageCount} disques)";
        public static string GetActiveMonitoringSimpleMessage() => "\u2705 Monitoring actif - Hardware initialis\u00e9";
        public static string GetErrorMessage(string error) => $"\u274c Erreur: {error}";
        public static string GetDiagnosticMessage() => "\U0001f527 Lancement du diagnostic hardware...";
        public static string GetForceRedetectionMessage() => "\U0001f527 Force re-d\u00e9tection hardware en cours...";
        public static string GetDiagnosticCompleteMessage(int totalSensors) => $"\U0001f527 Diagnostic termin\u00e9 - {totalSensors} capteurs total";
        public static string GetDiagnosticErrorMessage(string error) => $"\u274c Erreur diagnostic: {error}";
        public static string GetRedetectionErrorMessage(string error) => $"\u274c Erreur re-d\u00e9tection: {error}";

        public static string GetUpsIndicator(int ups, int intervalMs) => $"{ups} UPS ({intervalMs}ms)";
        public static string GetInitialUpsIndicator(int intervalMs) => $"0 UPS ({intervalMs}ms)";

        public static string GetActiveSpeedButton(int currentInterval) => currentInterval switch
        {
            UltraInterval => "Ultra",
            RapideInterval => "Rapide",
            NormalInterval => "Normal",
            _ => "Ultra"
        };
    }
}
