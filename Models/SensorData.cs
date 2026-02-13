using System;
using HardwareMonitorWinUI3.Core;

namespace HardwareMonitorWinUI3.Models
{
    /// <summary>
    /// Modèle de données pour un capteur - Fusion de SensorNode + SensorViewModel
    /// Responsabilité unique : données capteur avec notification UI
    /// </summary>
    public class SensorData : BaseViewModel
    {
        #region Fields

        private string _name = string.Empty;
        private string _icon = string.Empty;
        private string _value = string.Empty;
        private string _minValue = "Min: N/A";
        private string _maxValue = "Max: N/A";

        private float? _minRaw;
        private float? _maxRaw;

        // Propriété pour le type de capteur
        private string _sensorType = string.Empty;

        #endregion

        #region Properties avec notification

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string MinValue
        {
            get => _minValue;
            set => SetProperty(ref _minValue, value);
        }

        public string MaxValue
        {
            get => _maxValue;
            set => SetProperty(ref _maxValue, value);
        }

        // SUPPRIMÉ: ISensor? SensorReference pour éviter les memory leaks
        // La référence circulaire empêchait le garbage collection

        public string SensorType
        {
            get => _sensorType;
            set => SetProperty(ref _sensorType, value);
        }

        /// <summary>
        /// Détermine le groupe de catégorie du capteur
        /// Mapping exact selon l'énumération SensorType de LibreHardwareMonitor
        /// </summary>
        public string SensorCategory
        {
            get
            {
                return _sensorType switch
                {
                    "Voltage" => "Voltages",
                    "Clock" => "Clocks",
                    "Temperature" => "Temperatures",
                    "Load" => "Loads",
                    "Fan" => "Fans",
                    "Flow" => "Flows",
                    "Control" => "Controls",
                    "Level" => "Levels",
                    "Factor" => "Factors",
                    "Power" => "Powers",
                    "Data" => "Data",
                    "SmallData" => "Small Data",
                    "Frequency" => "Frequencies",
                    "Throughput" => "Throughput",
                    "Current" => "Current",
                    _ => "Others"
                };
            }
        }

        /// <summary>
        /// Retourne l'icône pour la catégorie
        /// </summary>
        public string CategoryIcon
        {
            get
            {
                return SensorCategory switch
                {
                    "Loads" => "📊",
                    "Temperatures" => "🌡️",
                    "Fans" => "💨",
                    "Powers" => "⚡",
                    "Voltages" => "🔌",
                    "Clocks" => "⏱️",
                    "Frequencies" => "⏱️",
                    "Data" => "📡",
                    "Small Data" => "📡",
                    "Flows" => "📡",
                    "Throughput" => "📡",
                    "Levels" => "📈",
                    "Controls" => "🎛️",
                    "Factors" => "🔧",
                    "Current" => "🔌",
                    "Others" => "📋",
                    _ => "📋"
                };
            }
        }

        #endregion

        #region Méthodes métier - Code réutilisé de SensorNode

        public void UpdateMinMax(float currentValue, string unit, string precision = "F1")
        {
            // Validation des paramètres critiques
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (precision == null) throw new ArgumentNullException(nameof(precision));
            
            // Validation : pour certains types de capteurs, ignorer les valeurs négatives impossibles
            // Note: Les températures négatives sont possibles (systèmes refroidis), tensions négatives existent
            if ((unit == "MB/s" || unit == "GB") && currentValue < 0)
            {
                return;
            }

            // Optimisation : conversion ToString() une seule fois si nécessaire
            string? formattedValue = null;

            // Mise à jour du minimum
            if (!_minRaw.HasValue || currentValue < _minRaw.Value)
            {
                _minRaw = currentValue;
                formattedValue ??= currentValue.ToString(precision);
                MinValue = $"Min: {formattedValue}{unit}";
            }

            // Mise à jour du maximum  
            if (!_maxRaw.HasValue || currentValue > _maxRaw.Value)
            {
                _maxRaw = currentValue;
                formattedValue ??= currentValue.ToString(precision);
                MaxValue = $"Max: {formattedValue}{unit}";
            }
        }

        public void ResetMinMax()
        {
            _minRaw = null;
            _maxRaw = null;
            MinValue = "Min: N/A";
            MaxValue = "Max: N/A";
        }

        #endregion
    }
} 