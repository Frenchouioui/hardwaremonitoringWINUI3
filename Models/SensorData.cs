using System;
using HardwareMonitorWinUI3.Core;
using HardwareMonitorWinUI3.Hardware;

namespace HardwareMonitorWinUI3.Models
{
    public sealed class SensorData : BaseViewModel, ISensorData
    {
        private static readonly string[] SensorCategories;
        private static readonly string[] CategoryIcons;

        static SensorData()
        {
            var categories = new[]
            {
                "Voltages", "Clocks", "Temperatures", "Loads", "Fans", "Flows",
                "Controls", "Levels", "Factors", "Powers", "Data", "Small Data",
                "Frequencies", "Throughput", "Current", "Time Span", "Timings",
                "Energy", "Noise", "Conductivity", "Humidity", "Others"
            };
            SensorCategories = categories;

            var icons = new[]
            {
                "\uE945", "\uE823", "\uE9CA", "\uE9D9", "\uE71E", "\uE81E",
                "\uE713", "\uE9D9", "\uE713", "\uE83E", "\uE8B7", "\uE8B7",
                "\uE823", "\uE8AB", "\uE945", "\uE823", "\uE823",
                "\uE83E", "\uE7F4", "\uE71E", "\uE759", "\uE950"
            };
            CategoryIcons = icons;
        }

        private string _name = string.Empty;
        private string _icon = string.Empty;
        private string _value = string.Empty;
        private string _minValue = "N/A";
        private string _maxValue = "N/A";

        private float? _minRaw;
        private float? _maxRaw;
        private float? _minTempCelsius;
        private float? _maxTempCelsius;

        private int _sensorTypeIndex = -1;
        private float? _rawValue;
        private bool _subscribedToTemperatureChange;

        internal float? RawValue
        {
            get => _rawValue;
            set => _rawValue = value;
        }

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
            get
            {
                if (_sensorTypeIndex == 2 && _minTempCelsius.HasValue)
                    return $"Min: {FormatTemperature(_minTempCelsius.Value)}";
                return _minValue;
            }
            set => SetProperty(ref _minValue, value);
        }

        public string MaxValue
        {
            get
            {
                if (_sensorTypeIndex == 2 && _maxTempCelsius.HasValue)
                    return $"Max: {FormatTemperature(_maxTempCelsius.Value)}";
                return _maxValue;
            }
            set => SetProperty(ref _maxValue, value);
        }

        private static string FormatTemperature(float celsius)
        {
            if (SensorExtensions.CurrentTemperatureUnit == TemperatureUnit.Fahrenheit)
            {
                float fahrenheit = celsius * 1.8f + 32f;
                return $"{fahrenheit:F1} \u00b0F";
            }
            return $"{celsius:F1} \u00b0C";
        }

        private void EnsureTemperatureSubscription()
        {
            if (_subscribedToTemperatureChange) return;

            _subscribedToTemperatureChange = true;
            SensorExtensions.TemperatureUnitChanged += OnTemperatureUnitChanged;
        }

        private void OnTemperatureUnitChanged(object? sender, EventArgs e)
        {
            if (_sensorTypeIndex == 2)
            {
                OnPropertyChanged(nameof(MinValue));
                OnPropertyChanged(nameof(MaxValue));

                if (_rawValue.HasValue)
                    Value = FormatTemperature(_rawValue.Value);
            }
        }

        private static int GetSensorTypeIndex(string typeName) => typeName switch
        {
            "Voltage" => 0,
            "Clock" => 1,
            "Temperature" => 2,
            "Load" => 3,
            "Fan" => 4,
            "Flow" => 5,
            "Control" => 6,
            "Level" => 7,
            "Factor" => 8,
            "Power" => 9,
            "Data" => 10,
            "SmallData" => 11,
            "Frequency" => 12,
            "Throughput" => 13,
            "Current" => 14,
            "TimeSpan" => 15,
            "Timing" => 16,
            "Energy" => 17,
            "Noise" => 18,
            "Conductivity" => 19,
            "Humidity" => 20,
            _ => 21
        };

        public string SensorType
        {
            get => _sensorTypeIndex >= 0 && _sensorTypeIndex < SensorCategories.Length - 1
                ? SensorCategories[_sensorTypeIndex].TrimEnd('s')
                : string.Empty;
            set
            {
                var newIndex = GetSensorTypeIndex(value);
                if (_sensorTypeIndex != newIndex)
                {
                    _sensorTypeIndex = newIndex;
                    OnPropertyChanged(nameof(SensorCategory));
                    OnPropertyChanged(nameof(CategoryIcon));
                }
            }
        }

        public string SensorCategory =>
            _sensorTypeIndex >= 0 && _sensorTypeIndex < SensorCategories.Length
                ? SensorCategories[_sensorTypeIndex]
                : "Others";

        public string CategoryIcon =>
            _sensorTypeIndex >= 0 && _sensorTypeIndex < CategoryIcons.Length
                ? CategoryIcons[_sensorTypeIndex]
                : "\uE950";

        public void UpdateMinMax(float currentValue, string unit, string precision = "F1")
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (precision == null) throw new ArgumentNullException(nameof(precision));

            if ((unit == "MB/s" || unit == "GB") && currentValue < 0)
                return;

            bool minUpdated = false;
            bool maxUpdated = false;

            if (!_minRaw.HasValue || currentValue < _minRaw.Value)
            {
                _minRaw = currentValue;
                minUpdated = true;
            }

            if (!_maxRaw.HasValue || currentValue > _maxRaw.Value)
            {
                _maxRaw = currentValue;
                maxUpdated = true;
            }

            if (minUpdated || maxUpdated)
            {
                string formattedValue = currentValue.ToString(precision);
                if (minUpdated) MinValue = $"Min: {formattedValue}{unit}";
                if (maxUpdated) MaxValue = $"Max: {formattedValue}{unit}";
            }
        }

        public void UpdateMinMaxThroughput(float currentValue, string sensorName)
        {
            if (currentValue < 0) return;

            bool minUpdated = false;
            bool maxUpdated = false;

            if (!_minRaw.HasValue || currentValue < _minRaw.Value)
            {
                _minRaw = currentValue;
                minUpdated = true;
            }

            if (!_maxRaw.HasValue || currentValue > _maxRaw.Value)
            {
                _maxRaw = currentValue;
                maxUpdated = true;
            }

            if (minUpdated || maxUpdated)
            {
                const int KB = 1024;
                const int MB = 1048576;

                string formatted = currentValue < MB
                    ? $"{currentValue / KB:F1} KB/s"
                    : $"{currentValue / MB:F1} MB/s";

                if (minUpdated) MinValue = $"Min: {formatted}";
                if (maxUpdated) MaxValue = $"Max: {formatted}";
            }
        }

        public void UpdateMinMaxTimeSpan(float currentValue)
        {
            bool minUpdated = false;
            bool maxUpdated = false;

            if (!_minRaw.HasValue || currentValue < _minRaw.Value)
            {
                _minRaw = currentValue;
                minUpdated = true;
            }

            if (!_maxRaw.HasValue || currentValue > _maxRaw.Value)
            {
                _maxRaw = currentValue;
                maxUpdated = true;
            }

            if (minUpdated || maxUpdated)
            {
                string formatted = TimeSpan.FromSeconds(currentValue).ToString("g");
                if (minUpdated) MinValue = $"Min: {formatted}";
                if (maxUpdated) MaxValue = $"Max: {formatted}";
            }
        }

        public void UpdateMinMaxTemperature(float currentValue)
        {
            EnsureTemperatureSubscription();

            bool minUpdated = false;
            bool maxUpdated = false;

            if (!_minTempCelsius.HasValue || currentValue < _minTempCelsius.Value)
            {
                _minTempCelsius = currentValue;
                _minRaw = currentValue;
                minUpdated = true;
            }

            if (!_maxTempCelsius.HasValue || currentValue > _maxTempCelsius.Value)
            {
                _maxTempCelsius = currentValue;
                _maxRaw = currentValue;
                maxUpdated = true;
            }

            if (minUpdated) OnPropertyChanged(nameof(MinValue));
            if (maxUpdated) OnPropertyChanged(nameof(MaxValue));
        }

        public void ResetMinMax()
        {
            _minRaw = null;
            _maxRaw = null;
            _minTempCelsius = null;
            _maxTempCelsius = null;
            _minValue = "N/A";
            _maxValue = "N/A";
            OnPropertyChanged(nameof(MinValue));
            OnPropertyChanged(nameof(MaxValue));
        }

        protected override void DisposeManaged()
        {
            if (_subscribedToTemperatureChange)
            {
                SensorExtensions.TemperatureUnitChanged -= OnTemperatureUnitChanged;
                _subscribedToTemperatureChange = false;
            }
            base.DisposeManaged();
        }
    }
}
