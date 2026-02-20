using System;
using HardwareMonitorWinUI3.Core;

namespace HardwareMonitorWinUI3.Models
{
    public class SensorData : BaseViewModel, ISensorData
    {
        #region Fields

        private string _name = string.Empty;
        private string _icon = string.Empty;
        private string _value = string.Empty;
        private string _minValue = "Min: N/A";
        private string _maxValue = "Max: N/A";

        private float? _minRaw;
        private float? _maxRaw;

        private string _sensorType = string.Empty;
        private string? _cachedSensorCategory;
        private string? _cachedCategoryIcon;
        private string? _sensorNameForThroughput;

        #endregion

        #region Properties

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

        public string SensorType
        {
            get => _sensorType;
            set
            {
                if (SetProperty(ref _sensorType, value))
                {
                    _cachedSensorCategory = null;
                    _cachedCategoryIcon = null;
                    OnPropertyChanged(nameof(SensorCategory));
                    OnPropertyChanged(nameof(CategoryIcon));
                }
            }
        }

        public string SensorCategory
        {
            get
            {
                if (_cachedSensorCategory != null)
                    return _cachedSensorCategory;

                _cachedSensorCategory = _sensorType switch
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
                    "TimeSpan" => "Time Span",
                    "Timing" => "Timings",
                    "Energy" => "Energy",
                    "Noise" => "Noise",
                    "Conductivity" => "Conductivity",
                    "Humidity" => "Humidity",
                    _ => "Others"
                };
                return _cachedSensorCategory;
            }
        }

        public string CategoryIcon
        {
            get
            {
                if (_cachedCategoryIcon != null)
                    return _cachedCategoryIcon;

                _cachedCategoryIcon = SensorCategory switch
                {
                    "Loads" => "\uE9D9",
                    "Temperatures" => "\uE9CA",
                    "Fans" => "\uE71E",
                    "Powers" => "\uE83E",
                    "Voltages" => "\uE945",
                    "Clocks" => "\uE823",
                    "Frequencies" => "\uE823",
                    "Data" => "\uE8B7",
                    "Small Data" => "\uE8B7",
                    "Flows" => "\uE81E",
                    "Throughput" => "\uE8AB",
                    "Levels" => "\uE9D9",
                    "Controls" => "\uE713",
                    "Factors" => "\uE713",
                    "Current" => "\uE945",
                    "Time Span" => "\uE823",
                    "Timings" => "\uE823",
                    "Energy" => "\uE83E",
                    "Noise" => "\uE7F4",
                    "Conductivity" => "\uE71E",
                    "Humidity" => "\uE759",
                    "Others" => "\uE950",
                    _ => "\uE950"
                };
                return _cachedCategoryIcon;
            }
        }

        #endregion

        #region ISensorData Implementation

public void UpdateMinMax(float currentValue, string unit, string precision = "F1")
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (precision == null) throw new ArgumentNullException(nameof(precision));

            if ((unit == "MB/s" || unit == "GB") && currentValue < 0)
            {
                // Negative throughput/data values indicate idle state or sensor error.
                // These should NOT affect min/max tracking as they don't represent
                // actual data transfer. For example, network adapters may report
                // negative values when no traffic is flowing.
                return;
            }

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
            
            _sensorNameForThroughput = sensorName;
            
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
                
                string FormatValue(float val)
                {
                    return val < MB 
                        ? $"{val / KB:F1} KB/s" 
                        : $"{val / MB:F1} MB/s";
                }
                
                if (minUpdated) MinValue = $"Min: {FormatValue(currentValue)}";
                if (maxUpdated) MaxValue = $"Max: {FormatValue(currentValue)}";
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
                string formattedValue = TimeSpan.FromSeconds(currentValue).ToString("g");
                if (minUpdated) MinValue = $"Min: {formattedValue}";
                if (maxUpdated) MaxValue = $"Max: {formattedValue}";
            }
        }
        
        public void UpdateMinMaxTemperature(float currentValue)
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
                string FormatTemperature(float celsius)
                {
                    if (Hardware.SensorExtensions.CurrentTemperatureUnit == TemperatureUnit.Fahrenheit)
                    {
                        float fahrenheit = celsius * 1.8f + 32f;
                        return $"{fahrenheit:F1} \u00b0F";
                    }
                    return $"{celsius:F1} \u00b0C";
                }
                
                if (minUpdated) MinValue = $"Min: {FormatTemperature(currentValue)}";
                if (maxUpdated) MaxValue = $"Max: {FormatTemperature(currentValue)}";
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
