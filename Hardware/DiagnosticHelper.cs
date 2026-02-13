using System;
using System.Linq;
using System.Text;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.UI;
using HardwareMonitorWinUI3.Shared;

namespace HardwareMonitorWinUI3.Hardware
{
    public static class DiagnosticHelper
    {
        public static bool CheckAdministratorRights()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                Logger.LogError("Erreur lors de la vérification des droits administrateur", ex);
                return false;
            }
        }

        public static string GenerateHardwareDiagnosticReport(Computer computer)
        {
            var report = new StringBuilder();
            report.AppendLine("DIAGNOSTIC HARDWARE COMPLET");
            report.AppendLine("================================");
            report.AppendLine();

            bool isAdmin = CheckAdministratorRights();
            report.AppendLine($"Droits Administrateur: {(isAdmin ? "OUI" : "NON")}");
            if (!isAdmin)
            {
                report.AppendLine("ATTENTION: Les droits administrateur sont requis pour détecter les disques !");
            }
            report.AppendLine();

            if (computer == null)
            {
                report.AppendLine("ERREUR: Objet Computer null");
                return report.ToString();
            }

            GenerateStorageDiagnostic(computer, report);

            report.AppendLine($"Total hardware détecté: {computer.Hardware.Count()}");
            
            foreach (var hardware in computer.Hardware)
            {
                if (hardware == null) continue;
                
                report.AppendLine($"\n{hardware.HardwareType}: {hardware.Name}");
                report.AppendLine($"   ID: {hardware.Identifier}");
                report.AppendLine($"   Capteurs: {hardware.Sensors.Count()}");
                
                foreach (var sensor in hardware.Sensors.Take(5)) // Limiter à 5 pour la lisibilité
                {
                    if (sensor == null) continue;

                    string precision = sensor.SensorType.GetSensorPrecision();
                    string value = sensor.Value?.ToString(precision) ?? "N/A";
                    string unit = sensor.SensorType.GetSensorUnit();
                    report.AppendLine($"     {sensor.Name ?? "Unknown"}: {value} {unit}");
                }
                
                if (hardware.Sensors.Count() > 5)
                {
                    report.AppendLine($"     ... et {hardware.Sensors.Count() - 5} autres capteurs");
                }
                
                foreach (var subHardware in hardware.SubHardware)
                {
                    if (subHardware == null) continue;
                    report.AppendLine($"   Sous-système: {subHardware.Name} ({subHardware.Sensors.Count()} capteurs)");
                }
            }

            return report.ToString();
        }

        private static void GenerateStorageDiagnostic(Computer computer, StringBuilder report)
        {
            report.AppendLine("DIAGNOSTIC STORAGE SPÉCIALISÉ");
            report.AppendLine("--------------------------------");

            // Matérialiser une seule fois pour éviter double exécution du query
            var storageDevices = computer.Hardware.Where(h => h.HardwareType == HardwareType.Storage).ToList();
            report.AppendLine($"Nombre de périphériques de stockage détectés: {storageDevices.Count}");

            if (storageDevices.Count == 0)
            {
                report.AppendLine("AUCUN DISQUE DÉTECTÉ");
                report.AppendLine();
                report.AppendLine("Solutions possibles:");
                report.AppendLine("   1. Relancer l'application en tant qu'administrateur");
                report.AppendLine("   2. Vérifier que IsStorageEnabled = true");
                report.AppendLine("   3. Vérifier les pilotes de stockage");
                report.AppendLine("   4. Redémarrer le service Windows Management Instrumentation");
            }
            else
            {
                foreach (var storage in storageDevices)
                {
                    report.AppendLine($"{storage.Name}");
                    report.AppendLine($"   Capteurs: {storage.Sensors.Count()}");

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

        /// <summary>
        /// Log la détection hardware sans interférer avec la logique métier
        /// </summary>
        public static void LogHardwareDetection(Computer? computer)
        {
            Logger.LogInfo("=== DIAGNOSTIC HARDWARE COMPLET ===");
            if (computer != null)
            {
                foreach (var hardware in computer.Hardware)
                {
                    Logger.LogInfo($"Hardware: {hardware.Name} (Type: {hardware.HardwareType})");
                    Logger.LogInfo($"   - Sensors: {hardware.Sensors.Count()}");
                    Logger.LogInfo($"   - SubHardware: {hardware.SubHardware.Count()}");
                    
                    // Affichage des capteurs principaux
                    foreach (var sensor in hardware.Sensors)
                    {
                        Logger.LogInfo($"     Sensor: {sensor.Name}: {sensor.Value} ({sensor.SensorType})");
                    }
                    
                    // Affichage des sous-hardware
                    foreach (var subHardware in hardware.SubHardware)
                    {
                        Logger.LogInfo($"   SubHardware: {subHardware.Name} (Type: {subHardware.HardwareType})");
                        foreach (var subSensor in subHardware.Sensors)
                        {
                            Logger.LogInfo($"       Sensor: {subSensor.Name}: {subSensor.Value} ({subSensor.SensorType})");
                        }
                    }
                }
            }
            Logger.LogInfo("=== FIN DIAGNOSTIC ===");
        }

        /// <summary>
        /// Log spécifiquement la détection storage
        /// </summary>
        public static void LogStorageDetection(IHardware hardware)
        {
            if (hardware.HardwareType == HardwareType.Storage)
            {
                Logger.LogInfo($"STORAGE DÉTECTÉ: {hardware.Name}");
                Logger.LogInfo($"   - Nombre de capteurs: {hardware.Sensors.Count()}");
                Logger.LogInfo($"   - Nombre de sub-hardware: {hardware.SubHardware.Count()}");
                Logger.LogInfo($"   - Identifier: {hardware.Identifier}");
                
                foreach (var sensor in hardware.Sensors)
                {
                    Logger.LogInfo($"     Capteur: {sensor.Name} = {sensor.Value} ({sensor.SensorType})");
                }
                
                foreach (var sub in hardware.SubHardware)
                {
                    Logger.LogInfo($"     Sub-hardware: {sub.Name} avec {sub.Sensors.Count()} capteurs");
                }
            }
        }
    }
} 