using System;
using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.Hardware
{
    public static class SensorExtensions
    {
        private static int _currentTemperatureUnit = (int)TemperatureUnit.Celsius;

        public static TemperatureUnit CurrentTemperatureUnit
        {
            get => (TemperatureUnit)Interlocked.CompareExchange(ref _currentTemperatureUnit, 0, 0);
            set => Interlocked.Exchange(ref _currentTemperatureUnit, (int)value);
        }

        public static string GetSensorUnit(this SensorType type) => type switch
        {
            SensorType.Temperature => "\u00b0C",
            SensorType.Clock => "MHz",
            SensorType.Voltage => "V",
            SensorType.Current => "A",
            SensorType.Power => "W",
            SensorType.Data => "GB",
            SensorType.SmallData => "MB",
            SensorType.Load => "%",
            SensorType.Fan => "RPM",
            SensorType.Flow => "L/h",
            SensorType.Control => "%",
            SensorType.Level => "%",
            SensorType.Factor => "",
            SensorType.Frequency => "Hz",
            SensorType.Throughput => "B/s",
            SensorType.TimeSpan => "",
            SensorType.Timing => "ns",
            SensorType.Energy => "mWh",
            SensorType.Noise => "dBA",
            SensorType.Conductivity => "\u00b5S/cm",
            SensorType.Humidity => "%",
            _ => ""
        };

        public static string GetSensorPrecision(this SensorType type) => type switch
        {
            SensorType.Temperature => "F1",
            SensorType.Clock => "F1",
            SensorType.Voltage => "F3",
            SensorType.Current => "F3",
            SensorType.Power => "F1",
            SensorType.Data => "F1",
            SensorType.SmallData => "F1",
            SensorType.Load => "F1",
            SensorType.Fan => "F0",
            SensorType.Flow => "F1",
            SensorType.Control => "F1",
            SensorType.Level => "F1",
            SensorType.Frequency => "F1",
            SensorType.Throughput => "F1",
            SensorType.TimeSpan => "g",
            SensorType.Timing => "F3",
            SensorType.Energy => "F0",
            SensorType.Noise => "F0",
            SensorType.Conductivity => "F1",
            SensorType.Humidity => "F0",
            SensorType.Factor => "F3",
            _ => "F1"
        };

        public static string GetSensorIcon(this SensorType type) => type switch
        {
            SensorType.Temperature => "\uE9CA",
            SensorType.Clock => "\uE823",
            SensorType.Voltage => "\uE945",
            SensorType.Current => "\uE945",
            SensorType.Power => "\uE83E",
            SensorType.Data => "\uE8B7",
            SensorType.SmallData => "\uE8B7",
            SensorType.Load => "\uE9D9",
            SensorType.Fan => "\uE71E",
            SensorType.Flow => "\uE81E",
            SensorType.Control => "\uE713",
            SensorType.Level => "\uE9D9",
            SensorType.Frequency => "\uE823",
            SensorType.Throughput => "\uE8AB",
            SensorType.TimeSpan => "\uE823",
            SensorType.Timing => "\uE823",
            SensorType.Energy => "\uE83E",
            SensorType.Noise => "\uE7F4",
            SensorType.Conductivity => "\uE71E",
            SensorType.Humidity => "\uE759",
            SensorType.Factor => "\uE950",
            _ => "\uE950"
        };

        public static string ToFormattedString(this ISensor sensor)
        {
            if (sensor is null || !sensor.Value.HasValue)
                return "N/A";
            
            float value = sensor.Value.Value;
            
            switch (sensor.SensorType)
            {
                case SensorType.Temperature:
                    return FormatTemperature(value);
                    
                case SensorType.Throughput:
                    return FormatThroughput(value, sensor.Name);
                    
                case SensorType.TimeSpan:
                    return System.TimeSpan.FromSeconds(value).ToString("g");
                    
                default:
                    return $"{value.ToString(sensor.SensorType.GetSensorPrecision())} {sensor.SensorType.GetSensorUnit()}".TrimEnd();
            }
        }
        
        private static string FormatTemperature(float celsius)
        {
            if (CurrentTemperatureUnit == TemperatureUnit.Fahrenheit)
            {
                float fahrenheit = celsius * 1.8f + 32f;
                return $"{fahrenheit:F1} \u00b0F";
            }
            return $"{celsius:F1} \u00b0C";
        }
        
        public static float CelsiusToFahrenheit(float celsius) => celsius * 1.8f + 32f;
        
        private static string FormatThroughput(float value, string sensorName)
        {
            const int KB = 1024;
            const int MB = 1048576;
            
            if (sensorName == "Connection Speed")
            {
                if (value < KB)
                    return $"{value:F0} bps";
                else if (value < MB)
                    return $"{value / KB:F1} Kbps";
                else if (value < 1073741824)
                    return $"{value / MB:F1} Mbps";
                else
                    return $"{value / 1073741824:F1} Gbps";
            }
            
            return value < MB 
                ? $"{value / KB:F1} KB/s" 
                : $"{value / MB:F1} MB/s";
        }
        
        public static string FormatThroughputValue(float value, string sensorName)
        {
            return FormatThroughput(value, sensorName);
        }

        public static SensorData CreateSensorData(this ISensor sensor)
        {
            if (sensor == null)
            {
                return new SensorData
                {
                    Name = "Unknown Sensor",
                    Icon = SensorType.Load.GetSensorIcon(),
                    Value = "N/A"
                };
            }

            var sensorData = new SensorData
            {
                Name = sensor.Name ?? "Unknown Sensor",
                Icon = sensor.SensorType.GetSensorIcon()
            };

            sensorData.Value = sensor.ToFormattedString();

            if (sensor.Value.HasValue)
            {
                sensorData.SensorType = sensor.SensorType.ToString();
                
                if (sensor.SensorType == SensorType.Throughput)
                {
                    sensorData.UpdateMinMaxThroughput(sensor.Value.Value, sensor.Name ?? "");
                }
                else if (sensor.SensorType == SensorType.TimeSpan)
                {
                    sensorData.UpdateMinMaxTimeSpan(sensor.Value.Value);
                }
                else if (sensor.SensorType == SensorType.Temperature)
                {
                    sensorData.UpdateMinMaxTemperature(sensor.Value.Value);
                }
                else
                {
                    string unit = sensor.SensorType.GetSensorUnit();
                    string precision = sensor.SensorType.GetSensorPrecision();
                    sensorData.UpdateMinMax(sensor.Value.Value, unit, precision);
                }
            }

            return sensorData;
        }
    }
}
