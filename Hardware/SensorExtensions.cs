using LibreHardwareMonitor.Hardware;
using HardwareMonitorWinUI3.Models;

namespace HardwareMonitorWinUI3.Hardware
{
    public static class SensorExtensions
    {
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
            SensorType.Throughput => "MB/s",
            _ => ""
        };

        public static string GetSensorPrecision(this SensorType type) => type switch
        {
            SensorType.Temperature => "F1",
            SensorType.Clock => "F0",
            SensorType.Voltage => "F3",
            SensorType.Current => "F2",
            SensorType.Power => "F1",
            SensorType.Data => "F1",
            SensorType.SmallData => "F1",
            SensorType.Load => "F1",
            SensorType.Fan => "F0",
            SensorType.Flow => "F1",
            SensorType.Control => "F1",
            SensorType.Level => "F1",
            SensorType.Frequency => "F0",
            SensorType.Throughput => "F0",
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
            _ => "\uE950"
        };

        public static string ToFormattedString(this ISensor sensor)
        {
            if (sensor is null || !sensor.Value.HasValue)
                return "N/A";
            
            return $"{sensor.Value!.Value.ToString(sensor.SensorType.GetSensorPrecision())}{sensor.SensorType.GetSensorUnit()}";
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
                string unit = sensor.SensorType.GetSensorUnit();
                string precision = sensor.SensorType.GetSensorPrecision();
                sensorData.UpdateMinMax(sensor.Value.Value, unit, precision);
            }

            return sensorData;
        }
    }
}
