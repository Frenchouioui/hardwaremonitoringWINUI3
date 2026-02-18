using System;
using System.Linq;
using System.Text;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Hardware;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3.Hardware
{
    public static class DiagnosticHelper
    {
        public static bool CheckAdministratorRights(ILogger? logger = null)
        {
            try
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                logger?.LogError("Error checking administrator rights", ex);
                return false;
            }
        }

        public static string GenerateHardwareDiagnosticReport(Computer? computer, ILogger? logger = null)
        {
            var report = new StringBuilder();
            report.AppendLine("FULL HARDWARE DIAGNOSTIC");
            report.AppendLine("================================");
            report.AppendLine();

            bool isAdmin = CheckAdministratorRights(logger);
            report.AppendLine($"Administrator Rights: {(isAdmin ? "YES" : "NO")}");
            if (!isAdmin)
            {
                report.AppendLine("WARNING: Administrator rights are required to detect storage devices!");
            }
            report.AppendLine();

            if (computer == null)
            {
                report.AppendLine("ERROR: Computer object is null");
                return report.ToString();
            }

            GenerateStorageDiagnostic(computer, report);

            int totalHardwareCount = computer.Hardware.Count();
            report.AppendLine($"Total hardware detected: {totalHardwareCount}");

            foreach (var hardware in computer.Hardware)
            {
                if (hardware == null) continue;

                var sensors = hardware.Sensors;
                int sensorCount = sensors.Count();
                report.AppendLine($"\n{hardware.HardwareType}: {hardware.Name}");
                report.AppendLine($"   ID: {hardware.Identifier}");
                report.AppendLine($"   Sensors: {sensorCount}");

                foreach (var sensor in sensors.Take(5))
                {
                    if (sensor == null) continue;

                    string precision = sensor.SensorType.GetSensorPrecision();
                    string value = sensor.Value?.ToString(precision) ?? "N/A";
                    string unit = sensor.SensorType.GetSensorUnit();
                    report.AppendLine($"     {sensor.Name ?? "Unknown"}: {value} {unit}");
                }

                if (sensorCount > 5)
                {
                    report.AppendLine($"     ... and {sensorCount - 5} more sensors");
                }

                foreach (var subHardware in hardware.SubHardware)
                {
                    if (subHardware == null) continue;
                    int subSensorCount = subHardware.Sensors.Count();
                    report.AppendLine($"   Sub-system: {subHardware.Name} ({subSensorCount} sensors)");
                }
            }

            return report.ToString();
        }

        private static void GenerateStorageDiagnostic(Computer computer, StringBuilder report)
        {
            report.AppendLine("STORAGE DIAGNOSTIC");
            report.AppendLine("--------------------------------");

            var storageDevices = computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage).ToList();
            report.AppendLine($"Storage devices detected: {storageDevices.Count}");

            if (storageDevices.Count == 0)
            {
                report.AppendLine("NO STORAGE DETECTED");
                report.AppendLine();
                report.AppendLine("Possible solutions:");
                report.AppendLine("   1. Restart the application as administrator");
                report.AppendLine("   2. Verify that IsStorageEnabled = true");
                report.AppendLine("   3. Check storage drivers");
                report.AppendLine("   4. Restart the Windows Management Instrumentation service");
            }
            else
            {
                foreach (var storage in storageDevices)
                {
                    int sensorCount = storage.Sensors.Count();
                    report.AppendLine($"{storage.Name}");
                    report.AppendLine($"   Sensors: {sensorCount}");

                    foreach (var sensor in storage.Sensors)
                    {
                        string precision = sensor.SensorType.GetSensorPrecision();
                        string value = sensor.Value?.ToString(precision) ?? "N/A";
                        string unit = sensor.SensorType.GetSensorUnit();
                        report.AppendLine($"     {sensor.Name ?? "Unknown"}: {value} {unit}");
                    }
                }
            }
            report.AppendLine();
        }

        public static void LogHardwareDetection(Computer? computer, ILogger logger)
        {
            logger.LogInfo("=== FULL HARDWARE DIAGNOSTIC ===");
            if (computer != null)
            {
                foreach (var hardware in computer.Hardware)
                {
                    int sensorCount = hardware.Sensors.Count();
                    int subHardwareCount = hardware.SubHardware.Count();
                    logger.LogInfo($"Hardware: {hardware.Name} (Type: {hardware.HardwareType})");
                    logger.LogInfo($"   - Sensors: {sensorCount}");
                    logger.LogInfo($"   - SubHardware: {subHardwareCount}");

                    foreach (var sensor in hardware.Sensors)
                    {
                        logger.LogInfo($"     Sensor: {sensor.Name}: {sensor.Value} ({sensor.SensorType})");
                    }

                    foreach (var subHardware in hardware.SubHardware)
                    {
                        logger.LogInfo($"   SubHardware: {subHardware.Name} (Type: {subHardware.HardwareType})");
                        foreach (var subSensor in subHardware.Sensors)
                        {
                            logger.LogInfo($"       Sensor: {subSensor.Name}: {subSensor.Value} ({subSensor.SensorType})");
                        }
                    }
                }
            }
            logger.LogInfo("=== END DIAGNOSTIC ===");
        }

        public static void LogStorageDetection(IHardware hardware, ILogger logger)
        {
            if (hardware.HardwareType == HardwareType.Storage)
            {
                int sensorCount = hardware.Sensors.Count();
                int subHardwareCount = hardware.SubHardware.Count();
                logger.LogInfo($"STORAGE DETECTED: {hardware.Name}");
                logger.LogInfo($"   - Sensor count: {sensorCount}");
                logger.LogInfo($"   - Sub-hardware count: {subHardwareCount}");
                logger.LogInfo($"   - Identifier: {hardware.Identifier}");

                foreach (var sensor in hardware.Sensors)
                {
                    logger.LogInfo($"     Sensor: {sensor.Name} = {sensor.Value} ({sensor.SensorType})");
                }

                foreach (var sub in hardware.SubHardware)
                {
                    int subSensorCount = sub.Sensors.Count();
                    logger.LogInfo($"     Sub-hardware: {sub.Name} with {subSensorCount} sensors");
                }
            }
        }
    }
}
