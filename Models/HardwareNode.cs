using System.Collections.ObjectModel;
using LibreHardwareMonitor.Hardware;
using System.Linq;
using HardwareMonitorWinUI3.Core;

namespace HardwareMonitorWinUI3.Models
{
    /// <summary>
    /// Model representant un composant hardware avec notifications PropertyChanged
    /// </summary>
    public class HardwareNode : BaseViewModel
    {
        #region Category Order Constants

        private const int VOLTAGE_ORDER = 1;
        private const int CLOCK_ORDER = 2;
        private const int TEMPERATURE_ORDER = 3;
        private const int LOAD_ORDER = 4;
        private const int FAN_ORDER = 5;
        private const int FLOW_ORDER = 6;
        private const int CONTROL_ORDER = 7;
        private const int LEVEL_ORDER = 8;
        private const int FACTOR_ORDER = 9;
        private const int POWER_ORDER = 10;
        private const int DATA_ORDER = 11;
        private const int SMALL_DATA_ORDER = 12;
        private const int FREQUENCY_ORDER = 13;
        private const int THROUGHPUT_ORDER = 14;
        private const int CURRENT_ORDER = 15;
        private const int DEFAULT_ORDER = 99;
        
        #endregion
        
        private string _name = string.Empty;
        private bool _isExpanded = true;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
        
        public ObservableCollection<SensorData> Sensors { get; } = new();
        public ObservableCollection<HardwareNode> SubHardware { get; } = new();
        public ObservableCollection<SensorGroup> SensorGroups { get; } = new();
        public IHardware? HardwareReference { get; set; }

        /// <summary>
        /// Organise les capteurs en groupes par catégorie
        /// </summary>
        public void OrganizeSensorsIntoGroups()
        {
            SensorGroups.Clear();

            var groupedSensors = Sensors.GroupBy(s => s.SensorCategory)
                                       .OrderBy(g => GetCategoryOrder(g.Key));
            
            foreach (var sensorCategoryGroup in groupedSensors)
            {
                var firstSensorInCategory = sensorCategoryGroup.First();

                var sensorGroup = new SensorGroup
                {
                    CategoryName = sensorCategoryGroup.Key,
                    CategoryIcon = firstSensorInCategory.CategoryIcon
                };
                
                foreach (var sensor in sensorCategoryGroup)
                {
                    sensorGroup.Sensors.Add(sensor);
                }
                
                SensorGroups.Add(sensorGroup);
            }
        }

        /// <summary>
        /// Détermine l'ordre d'affichage des catégories
        /// Ordre officiel LibreHardwareMonitor selon l'énumération SensorType
        /// </summary>
        private int GetCategoryOrder(string category)
        {
            return category switch
            {
                "Voltages" => VOLTAGE_ORDER,
                "Clocks" => CLOCK_ORDER,
                "Temperatures" => TEMPERATURE_ORDER,
                "Loads" => LOAD_ORDER,
                "Fans" => FAN_ORDER,
                "Flows" => FLOW_ORDER,
                "Controls" => CONTROL_ORDER,
                "Levels" => LEVEL_ORDER,
                "Factors" => FACTOR_ORDER,
                "Powers" => POWER_ORDER,
                "Data" => DATA_ORDER,
                "Small Data" => SMALL_DATA_ORDER,
                "Frequencies" => FREQUENCY_ORDER,
                "Throughput" => THROUGHPUT_ORDER,
                "Current" => CURRENT_ORDER,
                "Others" => DEFAULT_ORDER,
                _ => DEFAULT_ORDER
            };
        }
    }
}
