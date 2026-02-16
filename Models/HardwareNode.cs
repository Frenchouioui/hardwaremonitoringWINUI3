using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using HardwareMonitorWinUI3.Core;
using HardwareMonitorWinUI3.UI;
using System.Collections.Generic;

namespace HardwareMonitorWinUI3.Models
{
    public class HardwareNode : BaseViewModel
    {
        #region Category Order Constants

        private const int VoltageOrder = 1;
        private const int ClockOrder = 2;
        private const int TemperatureOrder = 3;
        private const int LoadOrder = 4;
        private const int FanOrder = 5;
        private const int FlowOrder = 6;
        private const int ControlOrder = 7;
        private const int LevelOrder = 8;
        private const int FactorOrder = 9;
        private const int PowerOrder = 10;
        private const int DataOrder = 11;
        private const int SmallDataOrder = 12;
        private const int FrequencyOrder = 13;
        private const int ThroughputOrder = 14;
        private const int CurrentOrder = 15;
        private const int DefaultOrder = 99;

        #endregion

        private string _name = string.Empty;
        private bool _isExpanded = true;
        private Dictionary<string, SensorData>? _sensorCache;

        public HardwareNode()
        {
            Sensors.CollectionChanged += OnSensorsCollectionChanged;
        }

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

        public HardwareCategory Category { get; init; }

        public string CategoryIcon => UIConstants.GetCategoryIcon(Category);

        public int SensorCount => Sensors.Count;

        public ObservableCollection<SensorData> Sensors { get; } = new();
        public ObservableCollection<HardwareNode> SubHardware { get; } = new();
        public ObservableCollection<SensorGroup> SensorGroups { get; } = new();

        internal Dictionary<string, SensorData> SensorCache => _sensorCache ??= BuildSensorCache();

        private Dictionary<string, SensorData> BuildSensorCache()
        {
            var cache = new Dictionary<string, SensorData>(Sensors.Count);
            foreach (var sensor in Sensors)
            {
                cache[$"{sensor.Name}|{sensor.SensorType}"] = sensor;
            }
            return cache;
        }

        internal void InvalidateSensorCache()
        {
            _sensorCache = null;
        }

        private void OnSensorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SensorCount));
            _sensorCache = null;
        }

        public void OrganizeSensorsIntoGroups()
        {
            foreach (var group in SensorGroups)
            {
                group.Dispose();
            }
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

        protected override void DisposeManaged()
        {
            Sensors.CollectionChanged -= OnSensorsCollectionChanged;
            
            foreach (var group in SensorGroups)
            {
                group.Dispose();
            }
            
            foreach (var subNode in SubHardware)
            {
                subNode.Dispose();
            }
            
            base.DisposeManaged();
        }

        private static int GetCategoryOrder(string category)
        {
            return category switch
            {
                "Voltages" => VoltageOrder,
                "Clocks" => ClockOrder,
                "Temperatures" => TemperatureOrder,
                "Loads" => LoadOrder,
                "Fans" => FanOrder,
                "Flows" => FlowOrder,
                "Controls" => ControlOrder,
                "Levels" => LevelOrder,
                "Factors" => FactorOrder,
                "Powers" => PowerOrder,
                "Data" => DataOrder,
                "Small Data" => SmallDataOrder,
                "Frequencies" => FrequencyOrder,
                "Throughput" => ThroughputOrder,
                "Current" => CurrentOrder,
                "Others" => DefaultOrder,
                _ => DefaultOrder
            };
        }
    }
}
